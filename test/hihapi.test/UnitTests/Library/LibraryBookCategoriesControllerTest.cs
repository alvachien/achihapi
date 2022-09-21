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
using hihapi.Controllers.Library;
using hihapi.Models.Library;

namespace hihapi.unittest.UnitTests.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryBookCategoriesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryBookCategoriesControllerTest(SqliteDatabaseFixture fixture)
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
            var control = new LibraryBookCategoriesController(context);
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
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<LibraryBookCategory>>(getokresult.Value);
            Assert.NotNull(getqueryresult);
            if (String.IsNullOrEmpty(strusr))
            {
                var dbcategories = (from acntctgy in context.BookCategories
                                    where acntctgy.HomeID == null
                                    select acntctgy).ToList<LibraryBookCategory>();
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
            var control = new LibraryBookCategoriesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(currentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var getresult = control.Get();
            Assert.NotNull(getresult);
            var getokresult = Assert.IsType<OkObjectResult>(getresult);
            var getqueryresult = Assert.IsAssignableFrom<IQueryable<LibraryBookCategory>>(getokresult.Value);
            Assert.NotNull(getqueryresult);

            // 2. Insert a new one.
            LibraryBookCategory ctgy = new LibraryBookCategory();
            ctgy.HomeID = hid;
            ctgy.Name = name;
            ctgy.Comment = name;
            var postresult = await control.Post(ctgy);
            var createdResult = Assert.IsType<CreatedODataResult<LibraryBookCategory>>(postresult);
            Assert.NotNull(createdResult);
            int nctgyid = createdResult.Entity.Id;
            Assert.Equal(hid, createdResult.Entity.HomeID);
            Assert.Equal(ctgy.Name, createdResult.Entity.Name);
            Assert.Equal(ctgy.Comment, createdResult.Entity.Comment);

            // 3. Read it out
            var getsingleresult = control.Get(nctgyid);
            Assert.NotNull(getsingleresult);
            var getctgy = Assert.IsType<LibraryBookCategory>(getsingleresult);
            Assert.Equal(hid, getctgy.HomeID);
            Assert.Equal(ctgy.Name, getctgy.Name);
            Assert.Equal(ctgy.Comment, getctgy.Comment);

            //// 4. Change it
            //getctgy.Comment += "Changed";
            //var putresult = control.Put(nctgyid, getctgy);
            //Assert.NotNull(putresult);

            // 5. Delete it
            var deleteresult = control.Delete(nctgyid);
            Assert.NotNull(deleteresult);

            await context.DisposeAsync();
        }
    }
}
