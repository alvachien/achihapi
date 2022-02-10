using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Results;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAssetCategoriesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceAssetCategoriesControllerTest(SqliteDatabaseFixture fixture)
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
            var control = new FinanceAssetCategoriesController(context);
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
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<FinanceAssetCategory>>(getokresult.Value);
            Assert.NotNull(getqueryresult);
            if (String.IsNullOrEmpty(strusr))
            {
                var dbcategories = (from tt in context.FinAssetCategories
                                    where tt.HomeID == null
                                    select tt).ToList<FinanceAssetCategory>();
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
            var control = new FinanceAssetCategoriesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(currentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var getresult = control.Get();
            Assert.NotNull(getresult);
            var getokresult = Assert.IsType<OkObjectResult>(getresult);
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<FinanceAssetCategory>>(getokresult.Value);
            Assert.NotNull(getqueryresult);

            // 2. Insert a new one.
            FinanceAssetCategory ctgy = new FinanceAssetCategory();
            ctgy.HomeID = hid;
            ctgy.Name = name;
            ctgy.Desp = name;            
            var postresult = await control.Post(ctgy);
            var createdResult = Assert.IsType<CreatedODataResult<FinanceAssetCategory>>(postresult);
            Assert.NotNull(createdResult);
            int nctgyid = createdResult.Entity.ID;
            Assert.Equal(hid, createdResult.Entity.HomeID);
            Assert.Equal(ctgy.Name, createdResult.Entity.Name);
            Assert.Equal(ctgy.Desp, createdResult.Entity.Desp);

            // 3. Read it out
            var getsingleresult = control.Get(nctgyid);
            Assert.NotNull(getsingleresult);
            var getctgy = Assert.IsType<FinanceAssetCategory>(getsingleresult);
            Assert.Equal(hid, getctgy.HomeID);
            Assert.Equal(ctgy.Name, getctgy.Name);
            Assert.Equal(ctgy.Desp, getctgy.Desp);

            // 4. Change it
            getctgy.Desp += "Changed";
            var putresult = control.Put(nctgyid, getctgy);
            Assert.NotNull(putresult);

            // 5. Delete it
            var deleteresult = control.Delete(nctgyid);
            Assert.NotNull(deleteresult);

            await context.DisposeAsync();
        }
    }
}
