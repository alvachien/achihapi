using System;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers.Library;
using hihapi.Exceptions;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task Post_InvalidObject_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            var control = CreateController(context, DataSetupUtility.UserA);
            var rec = BuildRecord(DataSetupUtility.Home1ID, 0, DataSetupUtility.UserA);
            await Assert.ThrowsAsync<BadRequestException>(() => control.Post(rec));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Delete_ByNonMember_Rejected()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB posts into Home 3 (A is a member, C is not)...
            var ctrlB = CreateController(context, DataSetupUtility.UserB);
            var postresult = await ctrlB.Post(BuildRecord(DataSetupUtility.Home3ID, 1, DataSetupUtility.UserB));
            var created = Assert.IsType<CreatedODataResult<LibraryBookReadingRecord>>(postresult);
            int nrecid = created.Entity.Id;

            try
            {
                // ... UserC (non-member of the record's home) may not delete it.
                var ctrlC = CreateController(context, DataSetupUtility.UserC);
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => ctrlC.Delete(nrecid));
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
