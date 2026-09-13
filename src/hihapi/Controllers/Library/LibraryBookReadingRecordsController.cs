using System;
using System.Globalization;
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
using Microsoft.EntityFrameworkCore;

namespace hihapi.Controllers.Library
{
    [Authorize]
    public class LibraryBookReadingRecordsController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryBookReadingRecordsController(hihDataContext context)
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

            // Whole-home visibility: all records of the homes the caller is a
            // member of (HomeMembers has a unique index on (HomeID, User), so
            // the join cannot duplicate rows).
            return Ok(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { hmem.HomeID, hmem.User } into hmems
                      join record in _context.BookReadingRecords
                        on hmems.HomeID equals record.HomeID
                      select record);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryBookReadingRecord Get([FromODataUri] Int32 key)
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

            // Join against the caller's homes: a record outside them renders as
            // 404 exactly like a missing one (no existence leak).
            var hidquery = from hmem in _context.HomeMembers
                           where hmem.User == usrName
                           select new { HomeID = hmem.HomeID };
            var recquery = from record in _context.BookReadingRecords
                           where record.Id == key
                           select record;
            return (from rec in recquery
                    join hid in hidquery
                      on rec.HomeID equals hid.HomeID
                    select rec).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryBookReadingRecord tbc)
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
                    throw new UnauthorizedAccessException();
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

            // The token is the source of truth for the reader identity;
            // overwrite whatever the client sent.
            tbc.User = usrName;

            // The server owns the status: client-supplied Status is ignored.
            // Only FromDate -> Reading (start a book); both dates -> Completed
            // (backfill a finished historical reading). Aborted cannot be created via POST.
            tbc.Status = tbc.ToDate == null
                ? LibraryBookReadingStatus.Reading
                : LibraryBookReadingStatus.Completed;

            // The server owns the key too: a client-sent Id would turn the
            // insert into an explicit-key write (UNIQUE collision on an
            // existing id -> unhandled 500, or AUTOINCREMENT sequence skew).
            tbc.Id = 0;

            if (!tbc.IsValid(_context))
            {
                throw new BadRequestException("Not a valid object");
            }

            var overlapCount = await CountOverlappingRecordsAsync(tbc.HomeID, tbc.BookId, tbc.User,
                tbc.FromDate!.Value.Date,                        // IsValid guaranteed non-null
                (tbc.ToDate?.Date) ?? DateTime.MaxValue, 0);     // open new record = unbounded end
            if (overlapCount > 0)
            {
                throw new BadRequestException(
                    "Reading period overlaps an existing record of the same reader for this book");
            }

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            _context.BookReadingRecords.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        /// <summary>
        /// Complete a reading: Reading -> Completed with the end date.
        /// POST /odata/LibraryBookReadingRecords/Default.CompleteReading
        /// </summary>
        [HttpPost]
        public Task<IActionResult> CompleteReading([FromBody] ODataActionParameters parameters)
        {
            return FinalizeReadingAsync(LibraryBookReadingStatus.Completed, parameters);
        }

        /// <summary>
        /// Abort a reading: Reading -> Aborted; the end date is optional.
        /// POST /odata/LibraryBookReadingRecords/Default.AbortReading
        /// </summary>
        [HttpPost]
        public Task<IActionResult> AbortReading([FromBody] ODataActionParameters parameters)
        {
            return FinalizeReadingAsync(LibraryBookReadingStatus.Aborted, parameters);
        }

        // Shared body of CompleteReading / AbortReading: both finalize an open
        // (Reading) record; they differ only in whether ToDate is mandatory and
        // in the resulting status.
        private async Task<IActionResult> FinalizeReadingAsync(LibraryBookReadingStatus target,
            ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModelStateError(ModelState);
            }

            if (parameters == null
                || !parameters.TryGetValue("HomeID", out var oHomeID)
                || !parameters.TryGetValue("RecordID", out var oRecordID))
            {
                throw new BadRequestException("HomeID and RecordID are required");
            }
            Int32 hid = Convert.ToInt32(oHomeID, CultureInfo.InvariantCulture);
            Int32 recId = Convert.ToInt32(oRecordID, CultureInfo.InvariantCulture);

            // User
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

            // Check whether User assigned with specified Home ID
            var hms = await _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).CountAsync();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // The record must exist and live in the CLAIMED home; a cross-home
            // RecordID renders as 404 exactly like a missing one (no existence
            // leak, same philosophy as Get(key)). Intentional deviation from
            // CloseAccount's silent Ok(false): these actions return the entity,
            // so "not yours" must not be answerable with 200.
            var tbd = await _context.BookReadingRecords.FindAsync(recId);
            if (tbd == null || tbd.HomeID != hid)
            {
                return NotFound();
            }

            // A legacy row from the era before dates were mandatory may lack a
            // start date: it can never be finalized (and the ToDate comparison
            // below would NRE) - give it its own verdict instead of the
            // misleading status message.
            if (tbd.FromDate == null)
            {
                throw new BadRequestException("Cannot finalize a record without a start date");
            }

            // Lifecycle gate: only Reading records can be finalized; Completed
            // and Aborted are terminal (a re-abort therefore also lands here).
            if (!tbd.IsFinalizeAllowed(_context))
            {
                throw new BadRequestException(String.Format(
                    CultureInfo.InvariantCulture,
                    "Only a record in Reading status can be {0}",
                    target == LibraryBookReadingStatus.Completed ? "completed" : "aborted"));
            }

            // End date: mandatory for Completed, optional for Aborted.
            parameters.TryGetValue("ToDate", out var oToDate);
            DateTime? toDate = null;
            var strToDate = oToDate as String;
            if (!String.IsNullOrEmpty(strToDate))
            {
                if (!DateTime.TryParse(strToDate, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsed))
                {
                    throw new BadRequestException("ToDate is not a valid date");
                }
                toDate = parsed.Date;
            }
            if (target == LibraryBookReadingStatus.Completed && toDate == null)
            {
                throw new BadRequestException("ToDate is required to complete a reading");
            }
            if (toDate != null && toDate.Value < tbd.FromDate!.Value.Date)
            {
                throw new BadRequestException("ToDate must not be earlier than FromDate");
            }

            // Re-check overlap with the finalized range, excluding this record
            // (it is currently Reading, hence in the scanned set). An abort
            // without an end date closes nothing, so there is nothing to check.
            if (toDate != null)
            {
                var overlapCount = await CountOverlappingRecordsAsync(
                    tbd.HomeID, tbd.BookId, tbd.User,
                    tbd.FromDate.Value.Date, toDate.Value.Date, tbd.Id);
                if (overlapCount > 0)
                {
                    throw new BadRequestException(
                        "Reading period overlaps an existing record of the same reader for this book");
                }
            }

            tbd.ToDate = toDate;
            tbd.Status = target;
            tbd.UpdatedAt = DateTime.Now;
            tbd.Updatedby = usrName;
            await _context.SaveChangesAsync();

            return Ok(tbd);
        }

        // Counts non-aborted records of the same home/book/reader whose period
        // overlaps [fromD, toD]. Several reading logs of the same book by the
        // same reader are allowed, but their periods must not overlap (day
        // granularity). NOTE (accepted limitation): the count + insert/update
        // is not atomic (TOCTOU) - two concurrent writers could both pass.
        // Range overlap cannot be expressed as a SQLite constraint, and the
        // single-family workload makes the race negligible; documented in
        // docs/reading-records-review-2026-09-12.md (finding A6). A stored row without ToDate is open-ended (still being
        // read) and overlaps everything at/after its FromDate; for an open new
        // record the caller passes DateTime.MaxValue as toD - never a captured
        // DateTime?, as SQL "x <= NULL" silently disables the comparison.
        // Aborted attempts never block: giving up on a book must not prevent
        // logging a future re-read. excludeId = 0 excludes nothing (IDs start
        // at 1); rows stored with a NULL FromDate can never collide.
        private Task<int> CountOverlappingRecordsAsync(Int32 homeID, int bookId, String user,
            DateTime fromD, DateTime toD, Int32 excludeId)
        {
            return _context.BookReadingRecords.CountAsync(r =>
                r.HomeID == homeID && r.BookId == bookId && r.User == user &&
                r.Id != excludeId &&
                r.Status != LibraryBookReadingStatus.Aborted &&
                r.FromDate != null && r.FromDate <= toD &&
                (r.ToDate == null || r.ToDate >= fromD));
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
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Look the record up AMONG THE CALLER'S HOMES (never the request
            // body): a record outside them renders as 404 exactly like a
            // missing one (no existence leak - same philosophy as Get(key) and
            // the finalize actions). The former 401-for-non-members let any
            // authenticated user enumerate valid record IDs across all homes
            // by the status-code difference. Whole-home scope is unchanged:
            // any member of the record's home may delete it.
            var tbd = await (from record in _context.BookReadingRecords
                             where record.Id == key
                             where _context.HomeMembers.Any(m => m.HomeID == record.HomeID && m.User == usrName)
                             select record).SingleOrDefaultAsync();
            if (tbd == null)
            {
                return NotFound();
            }

            _context.BookReadingRecords.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
