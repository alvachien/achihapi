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
    public class LibraryBooksController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryBooksController(hihDataContext context)
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
                      join book in _context.Books
                        on hmems.HomeID equals book.HomeID
                      select book);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryBook Get([FromODataUri] Int32 key)
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
            var ordquery = from ord in _context.Books
                           where ord.Id == key
                           select ord;
            var rstquery = from ord in ordquery
                           join hid in hidquery
                           on ord.HomeID equals hid.HomeID
                           select ord;

            return rstquery.SingleOrDefault();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] LibraryBook update)
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
            var existing = await _context.Books.FindAsync(key);
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

            // The book <-> author/translator/press/category/location linkage tables have no
            // DbSet (see Delete below), so they are reconciled via raw SQL within the same
            // transaction: existing rows are cleared and the incoming set is re-inserted.
            // The linkages carry no mutable scalar fields, so this is equivalent to a diff.
            var param = new SqliteParameter("@id", key);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();

                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_author WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_ctgy WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_location WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_press WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_translator WHERE BOOK_ID = @id", param);

                InsertLinkages(update.BookAuthors?.Select(a => a.AuthorId) ?? Enumerable.Empty<int>(), key,
                    "INSERT INTO t_lib_book_author (BOOK_ID, AUTHOR_ID) VALUES (@bookId, @foreignId)");
                InsertLinkages(update.BookTranslators?.Select(t => t.TranslatorId) ?? Enumerable.Empty<int>(), key,
                    "INSERT INTO t_lib_book_translator (BOOK_ID, TRANSLATOR_ID) VALUES (@bookId, @foreignId)");
                InsertLinkages(update.BookPresses?.Select(p => p.PressId) ?? Enumerable.Empty<int>(), key,
                    "INSERT INTO t_lib_book_press (BOOK_ID, PRESS_ID) VALUES (@bookId, @foreignId)");
                InsertLinkages(update.BookCategories?.Select(c => c.CategoryId) ?? Enumerable.Empty<int>(), key,
                    "INSERT INTO t_lib_book_ctgy (BOOK_ID, CTGY_ID) VALUES (@bookId, @foreignId)");
                InsertLinkages(update.BookLocations?.Select(l => l.LocationId) ?? Enumerable.Empty<int>(), key,
                    "INSERT INTO t_lib_book_location (BOOK_ID, LOCATION_ID) VALUES (@bookId, @foreignId)");

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Updated(update);
        }

        // Inserts one row per foreign ID into a book linkage table. The SQL statement passed
        // in is a constant string (no interpolation), so it is not subject to injection; the
        // only variable values are the parameterized @bookId and @foreignId.
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
        public async Task<IActionResult> Post([FromBody] LibraryBook tbc)
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

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            _context.Books.Add(tbc);
            await _context.SaveChangesAsync();

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

            var tbd = await _context.Books.FindAsync(key);
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var param = new SqliteParameter("@id", key);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_author WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_ctgy WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_location WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_press WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_translator WHERE BOOK_ID = @id", param);
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_def WHERE ID = @id", param);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            //_context.Books.Remove(tbd);
            //await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
