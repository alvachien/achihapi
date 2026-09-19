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

            // Serialize check→act→save against concurrent writes on the same
            // (table, home) — the duplicate guard has no DB-level backstop; see
            // NameGuardLock for why that is a deliberate trade-off.
            var gate = NameGuardLock.For<LibraryBook>(existing.HomeID);
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
                // LibraryNameGuard). ISBN is NOT part of the rule; the message names the
                // field that actually collided.
                await LibraryNameGuard.EnsureNoDuplicateNameAsync(
                    _context.Books.Where(p => p.HomeID == existing.HomeID && p.Id != key)
                                  .Select(p => new LibraryNameGuard.NamePair(p.NativeName, p.ChineseName)),
                    update.NativeName, update.ChineseName,
                    existing.NativeName, existing.ChineseName, "A book");

                update.CreatedAt = existing.CreatedAt;
                update.Createdby = existing.Createdby;
                update.UpdatedAt = DateTime.Now;
                update.Updatedby = usrName;
                _context.Entry(existing).CurrentValues.SetValues(update);

                // The book <-> author/translator/press/category/location linkage tables have
                // no DbSet (see Delete below), so they are reconciled via raw SQL within the
                // same transaction: existing rows are cleared and the incoming set is
                // re-inserted. The linkages carry no mutable scalar fields, so this is
                // equivalent to a diff.
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
            }
            finally
            {
                gate.Release();
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

            // Serialize check→insert against concurrent writes on the same
            // (table, home) — the guard has no DB-level backstop (NameGuardLock).
            var gate = NameGuardLock.For<LibraryBook>(tbc.HomeID);
            await gate.WaitAsync();
            try
            {
                // Duplicate guard (same home): on a create every non-blank name field is
                // checked - it must not equal ANY existing row's NativeName or non-empty
                // ChineseName ("Cross" == " cross "), folded with C# Trim +
                // ToLowerInvariant over the per-home name pairs (see LibraryNameGuard).
                // Whitespace-only inputs match nothing; ISBN is NOT part of the rule; the
                // message names the field that actually collided.
                await LibraryNameGuard.EnsureNoDuplicateNameAsync(
                    _context.Books.Where(p => p.HomeID == tbc.HomeID)
                                  .Select(p => new LibraryNameGuard.NamePair(p.NativeName, p.ChineseName)),
                    tbc.NativeName, tbc.ChineseName, null, null, "A book");

                tbc.CreatedAt = DateTime.Now;
                tbc.Createdby = usrName;

                _context.Books.Add(tbc);
                await _context.SaveChangesAsync();
            }
            finally
            {
                gate.Release();
            }

            return Created(tbc);
        }

        // One distinct (book, associated entity) pair for a ranking walk.
        private readonly record struct RankingRow(Int32 BookId, Int32 Id, String Name);

        // Top-3 aggregation over (book, entity) pair rows taken from the
        // materialized Include graph: a pair is unique by the linkage tables'
        // composite PK, so counting rows per entity already means "distinct
        // books". Order: count desc, then name asc ordinal (the client used
        // localeCompare - close enough inside this app's name space).
        private static IList<LibraryRankingItem> TopThree(IEnumerable<RankingRow> rows)
        {
            return rows.GroupBy(r => r.Id)
                .Select(g => new LibraryRankingItem
                {
                    Key = g.Key.ToString(),
                    Name = g.First().Name ?? string.Empty,
                    Count = g.Count(),
                })
                .OrderByDescending(r => r.Count)
                .ThenBy(r => r.Name, StringComparer.Ordinal)
                .Take(3)
                .ToList();
        }

        // Library overview key figures: totals, this/last-month add + completion
        // counts and the top-3 category/author/press rankings in ONE request,
        // replacing the overview page's old client-side walk of the whole book
        // collection (every 100 rows with $expand). Month windows use the server
        // clock - API and users share one machine in this deployment (see the
        // Finance overview keyfigure for the same single-clock model).
        [HttpPost]
        public async Task<IActionResult> GetLibraryOverviewKeyFigure([FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (parameters == null || !parameters.ContainsKey("HomeID"))
            {
                return BadRequest("Missing required parameter: HomeID");
            }

            Int32 hid = (Int32)parameters["HomeID"];

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
            var hms = await _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).CountAsync();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Half-open month windows [1st, 1st of next month), mirroring the
            // client's date-fns startOfMonth/addMonths math.
            var now = DateTime.Now;
            var thisBegin = new DateTime(now.Year, now.Month, 1);
            var thisEnd = thisBegin.AddMonths(1);
            var lastBegin = thisBegin.AddMonths(-1);

            var figure = new LibraryOverviewKeyFigure { HomeID = hid };
            figure.TotalBooks = await _context.Books.CountAsync(p => p.HomeID == hid);
            figure.AddedThisMonth = await _context.Books.CountAsync(
                p => p.HomeID == hid && p.CreatedAt >= thisBegin && p.CreatedAt < thisEnd);
            figure.AddedLastMonth = await _context.Books.CountAsync(
                p => p.HomeID == hid && p.CreatedAt >= lastBegin && p.CreatedAt < thisBegin);

            // DISTINCT books whose reading COMPLETED in the window (open/aborted
            // excluded, exactly what the client computed from its record walk).
            figure.CompletedThisMonth = await _context.BookReadingRecords
                .Where(p => p.HomeID == hid && p.Status == LibraryBookReadingStatus.Completed
                            && p.ToDate >= thisBegin && p.ToDate < thisEnd)
                .Select(p => p.BookId).Distinct().CountAsync();
            figure.CompletedLastMonth = await _context.BookReadingRecords
                .Where(p => p.HomeID == hid && p.Status == LibraryBookReadingStatus.Completed
                            && p.ToDate >= lastBegin && p.ToDate < thisBegin)
                .Select(p => p.BookId).Distinct().CountAsync();

            // Rankings: the skip navigations (Categories/Authors/Presses,
            // hihDataContext UsingEntity config) are loaded with Include - the
            // same translation path $expand already uses; SelectMany-in-query would
            // need SQL APPLY, which SQLite rejects. The top-3 math runs in memory
            // on the materialized graph (Linkage composite PKs keep the pairs
            // distinct, so no dedup pass is needed beyond per-book containment).
            // AsNoTracking: a pure read - tracking would also attach the join
            // entities and poison any later SaveChanges on this context.
            var linkedBooks = await _context.Books.Where(b => b.HomeID == hid)
                .Include(b => b.Categories)
                .Include(b => b.Authors)
                .Include(b => b.Presses)
                .AsNoTracking()
                .ToListAsync();
            figure.TopCategories = TopThree(linkedBooks.SelectMany(b => b.Categories
                .Select(c => new RankingRow(b.Id, c.Id, c.Name))));
            figure.TopAuthors = TopThree(linkedBooks.SelectMany(b => b.Authors
                .Select(a => new RankingRow(b.Id, a.Id,
                    string.IsNullOrEmpty(a.NativeName) ? a.ChineseName : a.NativeName))));
            figure.TopPresses = TopThree(linkedBooks.SelectMany(b => b.Presses
                .Select(o => new RankingRow(b.Id, o.Id,
                    string.IsNullOrEmpty(o.NativeName) ? o.ChineseName : o.NativeName))));

            return Ok(new List<LibraryOverviewKeyFigure> { figure });
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
