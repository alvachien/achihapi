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
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Http;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountCategoriesControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public FinanceAccountCategoriesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        public async Task TestCase1(int hid, string user)
        {
            var ctgyCount = DataSetupUtility.FinanceAccountCategories.Count();
            var context = this.fixture.GetCurrentDataContext();
            FinanceAccountCategoriesController control = new FinanceAccountCategoriesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 1. Read all categories
            var items = control.Get();
            var itemcnt = items.Count();
            Assert.Equal(ctgyCount, itemcnt);

            // 2. Insert new category
            var ctgy = new FinanceAccountCategory()
            {
                HomeID = hid,
                Name = "Test 1",
                Comment = "Test 1"
            };
            var rst1 = await control.Post(ctgy);
            Assert.NotNull(rst1);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceAccountCategory>>(rst1);
            Assert.Equal(ctgy.Name, rst2.Entity.Name);
            var firstctg = rst2.Entity.ID;
            Assert.True(firstctg > 0);
            Assert.Equal(ctgy.Comment, rst2.Entity.Comment);

            // 3. Read all categories
            items = control.Get();
            itemcnt = items.Count();
            Assert.Equal(ctgyCount + 1, itemcnt);

            // 4. Change the category's name
            ctgy.Name = "Test 2";
            rst1 = await control.Put(firstctg, ctgy);
            var rst3 = Assert.IsType<UpdatedODataResult<FinanceAccountCategory>>(rst1);
            Assert.Equal(ctgy.Name, rst3.Entity.Name);

            // 5. Delete it
            var rst4 = await control.Delete(firstctg);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst4);
            Assert.Equal(204, rst6.StatusCode);

            // 6. Read all categories again
            items = control.Get();
            itemcnt = items.Count();
            Assert.Equal(ctgyCount, itemcnt);

            await context.DisposeAsync();
        }
    }
}
