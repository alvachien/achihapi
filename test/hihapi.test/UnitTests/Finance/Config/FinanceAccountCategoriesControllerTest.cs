using System;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers;
using hihapi.Models;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Xunit;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountCategoriesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceAccountCategoriesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData("")]
        [InlineData(DataSetupUtility.UserA)]
        public async Task TestCase_Read(string strusr)
        {
            var context = fixture.GetCurrentDataContext();

            // 1. Read it without User assignment
            var control = new FinanceAccountCategoriesController(context);
            if (String.IsNullOrEmpty(strusr))
            {
                var userclaim = DataSetupUtility.GetClaimForUser(strusr);
                control.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = userclaim }
                };
            }
            var getresult = control.Get();
            Assert.NotNull(getresult);
            var getokresult = Assert.IsType<OkObjectResult>(getresult);
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<FinanceAccountCategory>>(getokresult.Value);
            Assert.NotNull(getqueryresult);
            if (String.IsNullOrEmpty(strusr))
            {
                var dbcategories = (from acntctgy in context.FinAccountCategories
                                    where acntctgy.HomeID == null
                                    select acntctgy).ToList<FinanceAccountCategory>();
                Assert.Equal(dbcategories.Count, getqueryresult.Count());
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, "Test 1")]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID, "Test 2")]
        public async Task TestCase_CRUD(string currentUser, int hid, string name)
        {
            var context = fixture.GetCurrentDataContext();

            // 1. Read it out before insert.
            var control = new FinanceAccountCategoriesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(currentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var getresult = control.Get();
            Assert.NotNull(getresult);
            var getokresult = Assert.IsType<OkObjectResult>(getresult);
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<FinanceAccountCategory>>(getokresult.Value);
            Assert.NotNull(getqueryresult);

            // 2. Insert a new one.
            FinanceAccountCategory ctgy = new FinanceAccountCategory();
            ctgy.HomeID = hid;
            ctgy.Name = name;
            ctgy.AssetFlag = true;
            ctgy.Comment = name;
            var postresult = await control.Post(ctgy);
            var createdResult = Assert.IsType<CreatedODataResult<FinanceAccountCategory>>(postresult);
            Assert.NotNull(createdResult);
            int nctgyid = createdResult.Entity.ID;
            Assert.Equal(hid, createdResult.Entity.HomeID);
            Assert.Equal(ctgy.Name, createdResult.Entity.Name);
            Assert.Equal(ctgy.AssetFlag, createdResult.Entity.AssetFlag);
            Assert.Equal(ctgy.Comment, createdResult.Entity.Comment);

            // 3. Read it out
            var getsingleresult = control.Get(nctgyid);
            Assert.NotNull(getsingleresult);
            var getctgy = Assert.IsType<FinanceAccountCategory>(getsingleresult);
            Assert.Equal(hid, getctgy.HomeID);
            Assert.Equal(ctgy.Name, getctgy.Name);
            Assert.Equal(ctgy.AssetFlag, getctgy.AssetFlag);
            Assert.Equal(ctgy.Comment, getctgy.Comment);

            // 4. Change it
            getctgy.Comment += "Changed";
            var putresult = control.Put(nctgyid, getctgy);
            Assert.NotNull(putresult);

            // 5. Delete it
            var deleteresult = control.Delete(nctgyid);
            Assert.NotNull(deleteresult);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Put_RejectsHomeIDChange()
        {
            var context = fixture.GetCurrentDataContext();
            var control = new FinanceAccountCategoriesController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA) }
            };

            // Create a Home 1 category as UserA (a member of Home 1)
            var ctgy = new FinanceAccountCategory
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "HomeIDChangeTest",
                AssetFlag = true,
                Comment = "HomeIDChangeTest",
            };
            var createdId = Assert.IsType<CreatedODataResult<FinanceAccountCategory>>(await control.Post(ctgy)).Entity.ID;

            try
            {
                // Attempt to move it to Home 2 via PUT (must be rejected)
                var update = new FinanceAccountCategory
                {
                    ID = createdId,
                    HomeID = DataSetupUtility.Home2ID,
                    Name = "HomeIDChangeTest",
                    AssetFlag = true,
                    Comment = "Changed",
                };
                var rst = await control.Put(createdId, update);
                Assert.IsType<BadRequestODataResult>(rst);
            }
            finally
            {
                control.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA) }
                };
                await control.Delete(createdId);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Put_RejectsCrossTenantWriteByNonMember()
        {
            var context = fixture.GetCurrentDataContext();

            // UserB (sole member of Home 2) creates a category in Home 2
            var control = new FinanceAccountCategoriesController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserB) }
            };
            var ctgy = new FinanceAccountCategory
            {
                HomeID = DataSetupUtility.Home2ID,
                Name = "CrossTenantTarget",
                AssetFlag = true,
                Comment = "CrossTenantTarget",
            };
            var createdId = Assert.IsType<CreatedODataResult<FinanceAccountCategory>>(await control.Post(ctgy)).Entity.ID;

            try
            {
                // UserA (NOT a member of Home 2) attempts to overwrite it, claiming Home 1 membership
                control.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA) }
                };
                var attack = new FinanceAccountCategory
                {
                    ID = createdId,
                    HomeID = DataSetupUtility.Home1ID,
                    Name = "Stolen",
                    AssetFlag = true,
                    Comment = "Stolen",
                };
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => control.Put(createdId, attack));
            }
            finally
            {
                control.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserB) }
                };
                await control.Delete(createdId);
            }

            await context.DisposeAsync();
        }
    }
}
