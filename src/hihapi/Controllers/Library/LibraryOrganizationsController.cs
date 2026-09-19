using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Exceptions;
using hihapi.Models.Library;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Controllers.Library
{
    [Authorize]
    public class LibraryOrganizationsController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryOrganizationsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            return Ok(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { hmem.HomeID, hmem.User, hmem.IsChild } into hmems
                      join org in _context.Organizations
                        on hmems.HomeID equals org.HomeID
                      select org);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryOrganization Get([FromODataUri] Int32 key)
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var hidquery = from hmem in _context.HomeMembers
                           where hmem.User == usrName
                           select new { HomeID = hmem.HomeID };
            var ordquery = from ord in _context.Organizations
                           where ord.Id == key
                           select ord;
            var rstquery = from ord in ordquery
                           join hid in hidquery
                           on ord.HomeID equals hid.HomeID
                           select ord;

            return rstquery.SingleOrDefault();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] LibraryOrganization update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModelStateError(ModelState);
            }

            if (key != update.Id)
            {
                throw new BadRequestException("Inputted ID mismatched");
            }

            // User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Find the existing record first - membership is checked against the EXISTING home,
            // not the HomeID in the request body (prevents cross-tenant mass-assignment).
            var existing = await _context.Organizations.FindAsync(key);
            if (existing == null)
            {
                return NotFound();
            }

            // Check whether User assigned with the existing Home ID
            var hms = await _context.HomeMembers.Where(p => p.HomeID == existing.HomeID && p.User == usrName).CountAsync();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Reject HomeID changes via PUT
            if (update.HomeID != existing.HomeID)
            {
                return BadRequest("HomeID cannot be changed via PUT.");
            }

            // Serialize check→act→save against concurrent writes on the same
            // (table, home) — the duplicate guard has no DB-level backstop; see
            // NameGuardLock for why that is a deliberate trade-off.
            var gate = NameGuardLock.For<LibraryOrganization>(existing.HomeID);
            await gate.WaitAsync();
            try
            {
                // Duplicate guard (same home): only the name fields this request actually
                // CHANGES are checked, so a row that ALREADY collides with another row
                // (data predating the guard) stays savable when a non-name field is
                // edited. A changed NativeName or non-empty ChineseName must not equal
                // ANY OTHER row's NativeName or non-empty ChineseName ("Cross" ==
                // " cross "), folded with C# Trim + ToLowerInvariant over the per-home
                // name pairs - Unicode-complete, unlike SQLite lower()/trim() (see
                // LibraryNameGuard). The message names the field that actually collided.
                await LibraryNameGuard.EnsureNoDuplicateNameAsync(
                    _context.Organizations.Where(p => p.HomeID == existing.HomeID && p.Id != key)
                                          .Select(p => new LibraryNameGuard.NamePair(p.NativeName, p.ChineseName)),
                    update.NativeName, update.ChineseName,
                    existing.NativeName, existing.ChineseName, "An organization");

                update.CreatedAt = existing.CreatedAt;
                update.Createdby = existing.Createdby;
                update.UpdatedAt = DateTime.Now;
                update.Updatedby = usrName;
                _context.Entry(existing).CurrentValues.SetValues(update);

                // The organization <-> type linkage table (t_lib_org_type) has no DbSet, so
                // it is reconciled via raw SQL within the same transaction: existing rows
                // are cleared and the incoming set is re-inserted. The linkage carries no
                // mutable scalar fields.
                var param = new SqliteParameter("@id", key);

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.SaveChangesAsync();

                    _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_org_type WHERE ORG_ID = @id", param);
                    InsertLinkages(update.OrganizationTypes?.Select(t => t.TypeId) ?? Enumerable.Empty<int>(), key,
                        "INSERT INTO t_lib_org_type (ORG_ID, TYPE_ID) VALUES (@bookId, @foreignId)");

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                gate.Release();
            }

            return Updated(update);
        }

        // Inserts one row per foreign ID into an organization linkage table. The SQL statement
        // passed in is a constant string (no interpolation), so it is not subject to injection;
        // the only variable values are the parameterized @bookId and @foreignId.
        private void InsertLinkages(IEnumerable<int> foreignIds, int bookId, string insertSql)
        {
            foreach (var foreignId in foreignIds)
            {
                var bookParam = new SqliteParameter("@bookId", bookId);
                var foreignParam = new SqliteParameter("@foreignId", foreignId);
                _context.Database.ExecuteSqlRaw(insertSql, bookParam, foreignParam);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryOrganization tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModelStateError(ModelState);
            }

            // User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check whether User assigned with specified Home ID
            var hms = await _context.HomeMembers.Where(p => p.HomeID == tbc.HomeID && p.User == usrName).CountAsync();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Serialize check→insert against concurrent writes on the same
            // (table, home) — the guard has no DB-level backstop (NameGuardLock).
            var gate = NameGuardLock.For<LibraryOrganization>(tbc.HomeID);
            await gate.WaitAsync();
            try
            {
                // Duplicate guard (same home): on a create every non-blank name field is
                // checked - it must not equal ANY existing row's NativeName or non-empty
                // ChineseName ("Cross" == " cross "), folded with C# Trim +
                // ToLowerInvariant over the per-home name pairs (see LibraryNameGuard).
                // Whitespace-only inputs match nothing; the message names the field that
                // actually collided.
                await LibraryNameGuard.EnsureNoDuplicateNameAsync(
                    _context.Organizations.Where(p => p.HomeID == tbc.HomeID)
                                          .Select(p => new LibraryNameGuard.NamePair(p.NativeName, p.ChineseName)),
                    tbc.NativeName, tbc.ChineseName, null, null, "An organization");

                tbc.CreatedAt = DateTime.Now;
                tbc.Createdby = usrName;
                _context.Organizations.Add(tbc);
                await _context.SaveChangesAsync();
            }
            finally
            {
                gate.Release();
            }

            return Created(tbc);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            // User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var tbd = await _context.Organizations.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            // Check whether User assigned with specified Home ID
            var hms = await _context.HomeMembers.Where(p => p.HomeID == tbd.HomeID && p.User == usrName).CountAsync();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.Organizations.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

    }
}
