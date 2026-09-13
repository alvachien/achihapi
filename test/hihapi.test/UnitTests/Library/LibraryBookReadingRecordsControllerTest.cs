using System;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers.Library;
using hihapi.Exceptions;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Xunit;

namespace hihapi.unittest.UnitTests.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryBookReadingRecordsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryBookReadingRecordsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        private static LibraryBookReadingRecordsController CreateController(
            hihDataContext context, string currentUser)
        {
            var control = new LibraryBookReadingRecordsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(currentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };
            return control;
        }

        private static LibraryBookReadingRecord BuildRecord(int hid, int bookid, string user)
        {
            return new LibraryBookReadingRecord
            {
                HomeID = hid,
                BookId = bookid,
                User = user,
                FromDate = new DateTime(2026, 8, 1),
                ToDate = new DateTime(2026, 8, 20),
                Comment = "TestCase",
            };
        }

        [Theory]
        [InlineData("")]
        [InlineData(DataSetupUtility.UserA)]
        public async Task TestCase_Read(string strusr)
        {
            var context = fixture.GetCurrentDataContext();

            var control = CreateController(context, strusr);
            if (String.IsNullOrEmpty(strusr))
            {
                Assert.Throws<UnauthorizedAccessException>(() => control.Get());
            }
            else
            {
                var getresult = control.Get();
                Assert.NotNull(getresult);
                var getokresult = Assert.IsType<OkObjectResult>(getresult);
                var getqueryresult = Assert.IsAssignableFrom<IQueryable<LibraryBookReadingRecord>>(getokresult.Value);
                Assert.NotNull(getqueryresult);
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID)]
        [InlineData(DataSetupUtility.UserC, DataSetupUtility.Home4ID)]
        public async Task TestCase_CRUD(string currentUser, int hid)
        {
            var context = fixture.GetCurrentDataContext();

            var control = CreateController(context, currentUser);

            // 1. Insert a new one. The client-sent User must be ignored: the
            // server stamps it from the token (source of truth).
            var rec = BuildRecord(hid, 1, "SPOOFED-USER");
            var postresult = await control.Post(rec);
            var createdResult = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(postresult);
            int nrecid = createdResult.Entity.Id;
            Assert.Equal(hid, createdResult.Entity.HomeID);
            Assert.Equal(1, createdResult.Entity.BookId);
            Assert.Equal(currentUser, createdResult.Entity.User);
            Assert.Equal(currentUser, createdResult.Entity.Createdby);
            // Both dates supplied -> server derives Completed (history backfill).
            Assert.Equal(LibraryBookReadingStatus.Completed, createdResult.Entity.Status);

            // 2. Read it out by key
            var getsingle = control.Get(nrecid);
            Assert.NotNull(getsingle);
            Assert.Equal(nrecid, getsingle.Id);

            // 3. Read it out in the collection
            var getokresult = Assert.IsType<OkObjectResult>(control.Get());
            var listed = Assert.IsAssignableFrom<IQueryable<LibraryBookReadingRecord>>(getokresult.Value)
                .ToList();
            Assert.Contains(listed, (p) => p.Id == nrecid);

            // 4. Delete it
            var deleteresult = await control.Delete(nrecid);
            Assert.NotNull(deleteresult);
            var deletestatus = Assert.IsType<StatusCodeResult>(deleteresult);
            Assert.Equal(204, deletestatus.StatusCode);

            // 5. Gone
            Assert.Null(control.Get(nrecid));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_Read_HomeWideVisibility()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB posts into Home 1 (A and B are both members)...
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var postresult = await ctrlB.Post(BuildRecord(DataSetupUtility.Home1ID, 1, DataSetupUtility.UserB));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(postresult);
            int nrecid = created.Entity.Id;

            try
            {
                // ... UserA sees it in the collection: visibility is home-wide,
                // not user-scoped.
                var ctrlA = CreateController(context, DataSetupUtility.UserA);
                var getokresult = Assert.IsType<OkObjectResult>(ctrlA.Get());
                var listed = Assert.IsAssignableFrom<IQueryable<LibraryBookReadingRecord>>(getokresult.Value)
                    .ToList();
                Assert.Contains(listed, (p) => p.Id == nrecid);
            }
            finally
            {
                await ctrlB.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task GetByKey_CrossHome_NotFound()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB posts into Home 2 (UserA is NOT a member of Home 2)...
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var postresult = await ctrlB.Post(BuildRecord(DataSetupUtility.Home2ID, 1, DataSetupUtility.UserB));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(postresult);
            int nrecid = created.Entity.Id;

            try
            {
                // ... UserA gets null (rendered as 404; existence not leaked).
                var ctrlA = CreateController(context, DataSetupUtility.UserA);
                Assert.Null(ctrlA.Get(nrecid));
            }
            finally
            {
                await ctrlB.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Post_CrossHome_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            // UserA is a member of Homes 1 and 3, not Home 4.
            var control = CreateController(context, DataSetupUtility.UserA);
            var rec = BuildRecord(DataSetupUtility.Home4ID, 1, DataSetupUtility.UserA);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => control.Post(rec));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Post_ClientSentId_Ignored()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            // A client-forged key must not become an explicit-key insert
            // (UNIQUE collision on an existing id -> unhandled 500, or
            // AUTOINCREMENT sequence skew): the server assigns the Id.
            var rec = BuildRecord(DataSetupUtility.Home1ID, 4303, DataSetupUtility.UserA);
            rec.Id = 999999;
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await control.Post(rec));

            try
            {
                Assert.NotEqual(999999, created.Entity.Id);
                // The server-assigned key reads back normally.
                Assert.NotNull(control.Get(created.Entity.Id));
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Post_InvalidObject_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            var control = CreateController(context, DataSetupUtility.UserA);
            var rec = BuildRecord(DataSetupUtility.Home1ID, 0, DataSetupUtility.UserA);
            await Assert.ThrowsAsync<BadRequestException>(() => control.Post(rec));

            await context.DisposeAsync();
        }

        private static LibraryBookReadingRecord BuildRecordRange(int hid, int bookid, string user,
            DateTime from, DateTime to)
        {
            var rec = BuildRecord(hid, bookid, user);
            rec.FromDate = from;
            rec.ToDate = to;
            return rec;
        }

        // Start-reading request: only a FromDate, open-ended until finalized.
        private static LibraryBookReadingRecord BuildOpenRecord(int hid, int bookid, string user,
            DateTime from)
        {
            var rec = BuildRecord(hid, bookid, user);
            rec.FromDate = from;
            rec.ToDate = null;
            return rec;
        }

        // Parameters for the CompleteReading / AbortReading collection actions;
        // a null toDate omits the key entirely (optional for abort).
        private static ODataActionParameters FinalizeParams(int hid, int recid, String toDate)
        {
            var p = new ODataActionParameters
            {
                { "HomeID", hid },
                { "RecordID", recid },
            };
            if (toDate != null)
                p.Add("ToDate", toDate);
            return p;
        }

        // FromDate is always required on POST (rule enforced in
        // LibraryBookReadingRecord.IsValid); a missing ToDate is now legal and
        // starts the Reading lifecycle (see Post_OnlyFromDate_CreatesReading).
        [Fact]
        public async Task Post_MissingFromDate_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            var control = CreateController(context, DataSetupUtility.UserA);

            var noFrom = BuildRecordRange(DataSetupUtility.Home1ID, 4242, DataSetupUtility.UserA,
                new DateTime(2026, 8, 1), new DateTime(2026, 8, 20));
            noFrom.FromDate = null;
            await Assert.ThrowsAsync<BadRequestException>(() => control.Post(noFrom));

            await context.DisposeAsync();
        }

        // The same reader may log the same book several times, but any overlapping
        // period must be refused; the checks below cover every overlap shape.
        [Fact]
        public async Task Post_OverlappingPeriod_Rejected()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var first = BuildRecordRange(DataSetupUtility.Home1ID, 4242, DataSetupUtility.UserA,
                new DateTime(2026, 8, 1), new DateTime(2026, 8, 20));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await control.Post(first));

            try
            {
                var overlapCases = new[]
                {
                    new DateTime(2026, 8, 10), new DateTime(2026, 8, 25), // crosses the start
                    new DateTime(2026, 7, 1),  new DateTime(2026, 8, 5),  // crosses the end
                    new DateTime(2026, 8, 5),  new DateTime(2026, 8, 10), // contained
                    new DateTime(2026, 8, 20), new DateTime(2026, 9, 1),  // shares the boundary day
                    new DateTime(2026, 8, 1),  new DateTime(2026, 8, 20), // identical
                };
                for (int i = 0; i < overlapCases.Length; i += 2)
                {
                    var dup = BuildRecordRange(DataSetupUtility.Home1ID, 4242, DataSetupUtility.UserA,
                        overlapCases[i], overlapCases[i + 1]);
                    await Assert.ThrowsAsync<BadRequestException>(() => control.Post(dup));
                }
            }
            finally
            {
                await control.Delete(created.Entity.Id);
            }

            await context.DisposeAsync();
        }

        // Adjacent periods (no shared day) and the same period logged by a
        // different reader of the same book are both fine.
        [Fact]
        public async Task Post_AdjacentPeriodAndDifferentUser_Allowed()
        {
            var context = fixture.GetCurrentDataContext();
            var ctrlA = CreateController(context, DataSetupUtility.UserA);
            var ctrlB = CreateController(context, DataSetupUtility.UserB);

            var first = BuildRecordRange(DataSetupUtility.Home1ID, 4242, DataSetupUtility.UserA,
                new DateTime(2026, 8, 1), new DateTime(2026, 8, 20));
            var createdFirst = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await ctrlA.Post(first));

            var adjacent = BuildRecordRange(DataSetupUtility.Home1ID, 4242, DataSetupUtility.UserA,
                new DateTime(2026, 8, 21), new DateTime(2026, 8, 31));
            var createdAdjacent = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await ctrlA.Post(adjacent));

            var otherUser = BuildRecordRange(DataSetupUtility.Home1ID, 4242, DataSetupUtility.UserB,
                new DateTime(2026, 8, 1), new DateTime(2026, 8, 20));
            var createdOther = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await ctrlB.Post(otherUser));

            await ctrlA.Delete(createdFirst.Entity.Id);
            await ctrlA.Delete(createdAdjacent.Entity.Id);
            await ctrlB.Delete(createdOther.Entity.Id);

            await context.DisposeAsync();
        }

        // ---------------------------------------------------------------------
        // Lifecycle: start / complete / abort
        // ---------------------------------------------------------------------

        [Fact]
        public async Task Post_OnlyFromDate_CreatesReading()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var rec = BuildOpenRecord(DataSetupUtility.Home1ID, 4301, DataSetupUtility.UserA,
                new DateTime(2026, 8, 1));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await control.Post(rec));
            Assert.Equal(LibraryBookReadingStatus.Reading, created.Entity.Status);
            Assert.Null(created.Entity.ToDate);

            await control.Delete(created.Entity.Id);
            await context.DisposeAsync();
        }

        // The server derives the status from the supplied dates; any
        // client-sent Status value is overwritten.
        [Fact]
        public async Task Post_StatusFromClient_Ignored()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var rec = BuildRecord(DataSetupUtility.Home1ID, 4302, DataSetupUtility.UserA);
            rec.Status = LibraryBookReadingStatus.Aborted;
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await control.Post(rec));
            Assert.Equal(LibraryBookReadingStatus.Completed, created.Entity.Status);

            await control.Delete(created.Entity.Id);
            await context.DisposeAsync();
        }

        [Fact]
        public async Task CompleteReading_HappyPath()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4310, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                var rst = await control.CompleteReading(
                    FinalizeParams(DataSetupUtility.Home1ID, nrecid, "2026-08-20"));
                var ok = Assert.IsType<OkObjectResult>(rst);
                var ent = Assert.IsType<LibraryBookReadingRecord>(ok.Value);
                Assert.Equal(nrecid, ent.Id);
                Assert.Equal(LibraryBookReadingStatus.Completed, ent.Status);
                Assert.Equal(new DateTime(2026, 8, 20), ent.ToDate);
                Assert.Equal(DataSetupUtility.UserA, ent.Updatedby);
                Assert.NotNull(ent.UpdatedAt);

                // Persisted, not just echoed:
                var reread = control.Get(nrecid);
                Assert.Equal(LibraryBookReadingStatus.Completed, reread.Status);
                Assert.Equal(new DateTime(2026, 8, 20), reread.ToDate);
            }
            finally
            {
                await control.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task CompleteReading_MissingKeyParams_Rejected()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            await Assert.ThrowsAsync<BadRequestException>(
                () => control.CompleteReading(new ODataActionParameters()));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task CompleteReading_MissingToDate_Rejected()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4311, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                // Completed demands an end date: the key is omitted, not empty.
                await Assert.ThrowsAsync<BadRequestException>(
                    () => control.CompleteReading(FinalizeParams(DataSetupUtility.Home1ID, nrecid, null)));
            }
            finally
            {
                await control.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Theory]
        [InlineData("not-a-date")]
        [InlineData("2026-07-31")]    // earlier than FromDate
        public async Task CompleteReading_BadToDate_Rejected(String strToDate)
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4312, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                await Assert.ThrowsAsync<BadRequestException>(
                    () => control.CompleteReading(FinalizeParams(DataSetupUtility.Home1ID, nrecid, strToDate)));

                // Still open after the rejected attempt:
                Assert.Equal(LibraryBookReadingStatus.Reading, control.Get(nrecid).Status);
                Assert.Null(control.Get(nrecid).ToDate);
            }
            finally
            {
                await control.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        // Completed and Aborted are terminal: only Reading can be finalized.
        [Fact]
        public async Task CompleteReading_NotReadingStatus_Rejected()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            // A backfilled record (both dates -> Completed) cannot be completed.
            var done = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildRecord(DataSetupUtility.Home1ID, 4313, DataSetupUtility.UserA)));
            try
            {
                await Assert.ThrowsAsync<BadRequestException>(
                    () => control.CompleteReading(FinalizeParams(DataSetupUtility.Home1ID, done.Entity.Id, "2026-08-25")));

                // A second completion of the same record is refused too.
                var open = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                    await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4314, DataSetupUtility.UserA,
                        new DateTime(2026, 8, 1))));
                try
                {
                    Assert.IsType<OkObjectResult>(await control.CompleteReading(
                        FinalizeParams(DataSetupUtility.Home1ID, open.Entity.Id, "2026-08-20")));
                    await Assert.ThrowsAsync<BadRequestException>(
                        () => control.CompleteReading(FinalizeParams(DataSetupUtility.Home1ID, open.Entity.Id, "2026-08-25")));
                }
                finally
                {
                    await control.Delete(open.Entity.Id);
                }
            }
            finally
            {
                await control.Delete(done.Entity.Id);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task AbortReading_WithoutEndDate_Succeeds()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4320, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                var rst = await control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, nrecid, null));
                var ok = Assert.IsType<OkObjectResult>(rst);
                var ent = Assert.IsType<LibraryBookReadingRecord>(ok.Value);
                Assert.Equal(LibraryBookReadingStatus.Aborted, ent.Status);
                Assert.Null(ent.ToDate);

                Assert.Equal(LibraryBookReadingStatus.Aborted, control.Get(nrecid).Status);
            }
            finally
            {
                await control.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task AbortReading_WithEndDate_Succeeds()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4321, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                var rst = await control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, nrecid, "2026-08-10"));
                var ok = Assert.IsType<OkObjectResult>(rst);
                var ent = Assert.IsType<LibraryBookReadingRecord>(ok.Value);
                Assert.Equal(LibraryBookReadingStatus.Aborted, ent.Status);
                Assert.Equal(new DateTime(2026, 8, 10), ent.ToDate);
            }
            finally
            {
                await control.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, 4322)]
        [InlineData(DataSetupUtility.UserB, 4323)]
        public async Task AbortReading_OnClosedStatus_Rejected(string currentUser, int bookId)
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, currentUser);

            // Completed cannot be aborted (backfilled via POST with both dates)...
            var done = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildRecord(DataSetupUtility.Home1ID, bookId, currentUser)));
            try
            {
                await Assert.ThrowsAsync<BadRequestException>(
                    () => control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, done.Entity.Id, null)));

                // ... and neither can an already-Aborted record (no re-abort).
                var open = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                    await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, bookId + 100, currentUser,
                        new DateTime(2026, 8, 1))));
                try
                {
                    Assert.IsType<OkObjectResult>(await control.AbortReading(
                        FinalizeParams(DataSetupUtility.Home1ID, open.Entity.Id, null)));
                    await Assert.ThrowsAsync<BadRequestException>(
                        () => control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, open.Entity.Id, null)));
                }
                finally
                {
                    await control.Delete(open.Entity.Id);
                }
            }
            finally
            {
                await control.Delete(done.Entity.Id);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task AbortReading_BadToDate_Rejected()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4324, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                await Assert.ThrowsAsync<BadRequestException>(
                    () => control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, nrecid, "2026-07-31")));

                Assert.Equal(LibraryBookReadingStatus.Reading, control.Get(nrecid).Status);
            }
            finally
            {
                await control.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task CompleteReading_UnknownKey_NotFound()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var rst = await control.CompleteReading(
                FinalizeParams(DataSetupUtility.Home1ID, 999999, "2026-08-20"));
            Assert.IsType<NotFoundResult>(rst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task AbortReading_UnknownKey_NotFound()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var rst = await control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, 999999, null));
            Assert.IsType<NotFoundResult>(rst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Finalize_CrossHome_NotFound()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB opens a record in Home 2 (UserA is NOT a member of Home 2)...
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await ctrlB.Post(BuildOpenRecord(DataSetupUtility.Home2ID, 4330, DataSetupUtility.UserB,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                // ... UserA claims its OWN home (Home 1) + B's RecordID:
                // renders as 404 exactly like a missing key (no existence leak).
                var ctrlA = CreateController(context, DataSetupUtility.UserA);
                var rst = await ctrlA.CompleteReading(FinalizeParams(DataSetupUtility.Home1ID, nrecid, "2026-08-20"));
                Assert.IsType<NotFoundResult>(rst);

                // Untouched:
                Assert.Equal(LibraryBookReadingStatus.Reading, ctrlB.Get(nrecid).Status);
            }
            finally
            {
                await ctrlB.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Finalize_NonMemberClaimedHome_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB opens a record in Home 2; UserA claims Home 2 membership it lacks.
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await ctrlB.Post(BuildOpenRecord(DataSetupUtility.Home2ID, 4331, DataSetupUtility.UserB,
                    new DateTime(2026, 8, 1))));
            int nrecid = created.Entity.Id;

            try
            {
                var ctrlA = CreateController(context, DataSetupUtility.UserA);
                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => ctrlA.AbortReading(FinalizeParams(DataSetupUtility.Home2ID, nrecid, null)));
            }
            finally
            {
                await ctrlB.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        // An open-ended record blocks later periods of the same book/reader -
        // including another start-reading attempt - while a period entirely
        // before it stays valid.
        [Fact]
        public async Task OpenRecord_BlocksLaterOverlappingPost()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var open = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4340, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));

            try
            {
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    BuildRecordRange(DataSetupUtility.Home1ID, 4340, DataSetupUtility.UserA,
                        new DateTime(2026, 8, 15), new DateTime(2026, 8, 20))));

                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    BuildOpenRecord(DataSetupUtility.Home1ID, 4340, DataSetupUtility.UserA,
                        new DateTime(2026, 9, 1))));

                var earlier = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await control.Post(
                    BuildRecordRange(DataSetupUtility.Home1ID, 4340, DataSetupUtility.UserA,
                        new DateTime(2026, 7, 1), new DateTime(2026, 7, 31))));
                await control.Delete(earlier.Entity.Id);
            }
            finally
            {
                await control.Delete(open.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Giving up on a book must not prevent logging a future re-read:
        // Aborted records never participate in overlap blocking.
        [Fact]
        public async Task AbortedRecord_DoesNotBlockNewReading()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var first = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4350, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));
            Assert.IsType<OkObjectResult>(await control.AbortReading(
                FinalizeParams(DataSetupUtility.Home1ID, first.Entity.Id, "2026-08-05")));

            try
            {
                var again = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                    await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4350, DataSetupUtility.UserA,
                        new DateTime(2026, 8, 1))));
                await control.Delete(again.Entity.Id);
            }
            finally
            {
                await control.Delete(first.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Completing adjacent to a closed period (no shared day) is fine.
        [Fact]
        public async Task CompleteReading_AdjacentClosedPeriod_Allowed()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var closed = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(await control.Post(
                BuildRecordRange(DataSetupUtility.Home1ID, 4360, DataSetupUtility.UserA,
                    new DateTime(2026, 7, 1), new DateTime(2026, 7, 31))));
            var open = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(
                await control.Post(BuildOpenRecord(DataSetupUtility.Home1ID, 4360, DataSetupUtility.UserA,
                    new DateTime(2026, 8, 1))));

            try
            {
                Assert.IsType<OkObjectResult>(await control.CompleteReading(
                    FinalizeParams(DataSetupUtility.Home1ID, open.Entity.Id, "2026-08-20")));
            }
            finally
            {
                await control.Delete(open.Entity.Id);
                await control.Delete(closed.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // (The completion path re-checks overlap as defence in depth, but a
        // Reading record that survived the POST-time check cannot later be
        // completed onto an overlap: every closed subrange of a non-overlapping
        // open interval is itself non-overlapping. No constructive test here.)

        // Legacy rows from the era before dates were mandatory may carry a NULL
        // FromDate (the seeder backfill leaves NULL-ToDate rows open). They can
        // never be finalized - and must get their own verdict, not the
        // misleading "Only a record in Reading status..." message.
        [Fact]
        public async Task Finalize_LegacyRowWithoutFromDate_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            var legacy = new LibraryBookReadingRecord
            {
                HomeID = DataSetupUtility.Home1ID,
                BookId = 4370,
                User = DataSetupUtility.UserA,
                FromDate = null,
                ToDate = null,
                Comment = "Legacy",
                Status = LibraryBookReadingStatus.Reading,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = DateTime.Now,
            };
            context.BookReadingRecords.Add(legacy);
            await context.SaveChangesAsync();

            try
            {
                var control = CreateController(context, DataSetupUtility.UserA);

                var ex = await Assert.ThrowsAsync<BadRequestException>(
                    () => control.CompleteReading(FinalizeParams(DataSetupUtility.Home1ID, legacy.Id, "2026-08-20")));
                Assert.Contains("start date", ex.Message, StringComparison.InvariantCultureIgnoreCase);

                var ex2 = await Assert.ThrowsAsync<BadRequestException>(
                    () => control.AbortReading(FinalizeParams(DataSetupUtility.Home1ID, legacy.Id, null)));
                Assert.Contains("start date", ex2.Message, StringComparison.InvariantCultureIgnoreCase);

                // Untouched by the rejected attempts:
                Assert.Equal(LibraryBookReadingStatus.Reading, control.Get(legacy.Id).Status);
            }
            finally
            {
                context.BookReadingRecords.Remove(legacy);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Delete_ByNonMember_NotFound()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB posts into Home 3 (A is a member, C is not)...
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var postresult = await ctrlB.Post(BuildRecord(DataSetupUtility.Home3ID, 1, DataSetupUtility.UserB));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(postresult);
            int nrecid = created.Entity.Id;

            try
            {
                // ... UserC (non-member of the record's home) gets 404 exactly
                // like a missing key (no existence leak - the former 401 let
                // any authenticated user enumerate valid IDs across homes).
                var ctrlC = CreateController(context, DataSetupUtility.UserC);
                var deleteresult = await ctrlC.Delete(nrecid);
                Assert.IsType<NotFoundResult>(deleteresult);

                // Untouched:
                Assert.NotNull(ctrlB.Get(nrecid));
            }
            finally
            {
                await ctrlB.Delete(nrecid);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Delete_CoMemberAllowed()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB posts into Home 1; UserA is a co-member and may delete it
            // (deliberate consequence of the whole-home visibility scope).
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var postresult = await ctrlB.Post(BuildRecord(DataSetupUtility.Home1ID, 1, DataSetupUtility.UserB));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(postresult);
            int nrecid = created.Entity.Id;

            var ctrlA = CreateController(context, DataSetupUtility.UserA);
            var deleteresult = await ctrlA.Delete(nrecid);
            var deletestatus = Assert.IsType<StatusCodeResult>(deleteresult);
            Assert.Equal(204, deletestatus.StatusCode);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Delete_UnknownKey_NotFound()
        {
            var context = fixture.GetCurrentDataContext();

            var control = CreateController(context, DataSetupUtility.UserA);
            var deleteresult = await control.Delete(999999);
            Assert.IsType<NotFoundResult>(deleteresult);

            await context.DisposeAsync();
        }
    }
}
