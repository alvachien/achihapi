using System;
using Xunit;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.test.UnitTests.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceTransactionTypesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceTransactionTypesControllerTest(SqliteDatabaseFixture fixture)
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
            var control = new FinanceTransactionTypesController(context);
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
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<FinanceTransactionType>>(getokresult.Value);
            Assert.NotNull(getqueryresult);
            if (String.IsNullOrEmpty(strusr))
            {
                var dbcategories = (from tt in context.FinTransactionType
                                    where tt.HomeID == null
                                    select tt).ToList<FinanceTransactionType>();
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
            var control = new FinanceTransactionTypesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(currentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var getresult = control.Get();
            Assert.NotNull(getresult);
            var getokresult = Assert.IsType<OkObjectResult>(getresult);
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<FinanceTransactionType>>(getokresult.Value);
            Assert.NotNull(getqueryresult);

            // 2. Insert a new one.
            FinanceTransactionType ctgy = new FinanceTransactionType();
            ctgy.HomeID = hid;
            ctgy.Name = name;
            ctgy.Comment = name;
            var postresult = await control.Post(ctgy);
            var createdResult = Assert.IsType<CreatedODataResult<FinanceTransactionType>>(postresult);
            Assert.NotNull(createdResult);
            int nctgyid = createdResult.Entity.ID;
            Assert.Equal(hid, createdResult.Entity.HomeID);
            Assert.Equal(ctgy.Name, createdResult.Entity.Name);
            Assert.Equal(ctgy.Comment, createdResult.Entity.Comment);

            // 3. Read it out
            var getsingleresult = control.Get(nctgyid);
            Assert.NotNull(getsingleresult);
            var getctgy = Assert.IsType<FinanceTransactionType>(getsingleresult);
            Assert.Equal(hid, getctgy.HomeID);
            Assert.Equal(ctgy.Name, getctgy.Name);
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
    }
}
