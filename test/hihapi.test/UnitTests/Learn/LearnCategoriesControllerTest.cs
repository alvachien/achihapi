using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;
using hihapi.test.common;
using Microsoft.AspNetCore.OData.Deltas;
using hihapi.Exceptions;

namespace hihapi.unittest.Learn
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LearnCategoriesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public LearnCategoriesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        public async Task TestCase_CRUD(int hid, string usr)
        {
            var context = this.fixture.GetCurrentDataContext();
            // Pre. setup
            this.fixture.InitHomeTestData(hid, context);

            var control = new LearnCategoriesController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(usr);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 2. Read
            var getrst = control.Get();
            Assert.NotNull(getrst);

            var getallokresult = Assert.IsType<OkObjectResult>(getrst);
            var getallokresultval = Assert.IsAssignableFrom<IQueryable<LearnCategory>>(getallokresult.Value);

            if (getallokresultval.Count() > 0)
            {
                //// Read single
                //var getsinglerst = control.Get(getallokresultval.First().ID);
                //Assert.NotNull(getsinglerst);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task PutWithInvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();

            // Action
            var control = new LearnCategoriesController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.Put(9999, null);
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }
    }
}
