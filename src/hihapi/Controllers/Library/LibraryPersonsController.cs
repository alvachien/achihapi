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
    public class LibraryPersonsController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryPersonsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /LibraryPersons
        [EnableQuery]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinanceAccount> option)
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
                      join person in _context.Persons
                        on hmems.HomeID equals person.HomeID
                      select person);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryPerson Get([FromODataUri] Int32 key)
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
            var ordquery = from ord in _context.Persons
                           where ord.Id == key
                           select ord;
            var rstquery = from ord in ordquery
                           join hid in hidquery
                           on ord.HomeID equals hid.HomeID
                           select ord;

            return rstquery.SingleOrDefault();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] LibraryPerson update)
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
            var existing = await _context.Persons.FindAsync(key);
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

            update.CreatedAt = existing.CreatedAt;
            update.Createdby = existing.Createdby;
            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(existing).CurrentValues.SetValues(update);

            // The person <-> role linkage table (t_lib_person_role) has no DbSet, so it is
            // reconciled via raw SQL within the same transaction: existing rows are cleared and
            // the incoming set is re-inserted. The linkage carries no mutable scalar fields.
            var roleIds = await ResolveRoleIdsAsync(
                update.PersonRoles?.Select(r => r.RoleId) ?? Enumerable.Empty<int>(), usrName);

            var param = new SqliteParameter("@id", key);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();

                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_person_role WHERE PERSON_ID = @id", param);
                InsertLinkages(roleIds, key,
                    "INSERT INTO t_lib_person_role (PERSON_ID, ROLE_ID) VALUES (@bookId, @foreignId)");

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Updated(update);
        }

        // Inserts one row per foreign ID into a person/org linkage table. The SQL statement
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

        // Keeps only role IDs that exist and are visible to the requesting user, mirroring
        // LibraryPersonRolesController.Get (shared rows with HomeID == null, plus rows of any
        // home the user is a member of). Blank linkage rows the UI used to submit arrive as
        // RoleId 0; feeding them to EF (graph insert) or to the FK-constrained linkage table
        // must not be possible.
        private async Task<List<int>> ResolveRoleIdsAsync(IEnumerable<int> roleIds, string usrName)
        {
            var ids = (from rid in roleIds
                       where rid > 0
                       select rid).Distinct().ToList();
            if (ids.Count == 0)
            {
                return ids;
            }

            var memberHomeIds = from hmem in _context.HomeMembers
                                where hmem.User == usrName
                                select hmem.HomeID;

            return await _context.PersonRoles
                .Where(r => ids.Contains(r.Id) &&
                            (r.HomeID == null || memberHomeIds.Contains(r.HomeID.Value)))
                .Select(r => r.Id)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryPerson tbc)
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

            // The linkage/join collections on an inbound entity must not enter the EF graph:
            // rows with an unresolved RoleId (e.g. the blank assignment rows the UI sends as
            // RoleId 0) crash SaveChanges with "The value of 'LibraryPersonRoleLinkage.RoleId'
            // is unknown". Mirror Put: insert only the person row, then reconcile the validated
            // linkage rows via raw SQL within the same transaction.
            var roleIds = await ResolveRoleIdsAsync(
                tbc.PersonRoles?.Select(r => r.RoleId) ?? Enumerable.Empty<int>(), usrName);
            tbc.PersonRoles = null;
            tbc.Roles = null;
            tbc.CurrentHome = null;
            tbc.WritenBooks = null;
            tbc.WrittenBooksByAuthor = null;
            tbc.TranslatedBooks = null;
            tbc.TranslatedBooksByTranslator = null;

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Persons.Add(tbc);
                await _context.SaveChangesAsync();

                InsertLinkages(roleIds, tbc.Id,
                    "INSERT INTO t_lib_person_role (PERSON_ID, ROLE_ID) VALUES (@bookId, @foreignId)");

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
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

            var tbd = await _context.Persons.FindAsync(key);
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

            _context.Persons.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
