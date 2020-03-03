using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Http;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LearnCategoriesControllerTest : IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> ctgiesCreated = new List<Int32>();

        public LearnCategoriesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<LearnCategory>(provider, "LearnCategories");
        }

        public void Dispose()
        {
            CleanupCreatedEntries();

            if (this.provider != null)
            {
                this.provider.Dispose();
                this.provider = null;
            }
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        public async Task TestCase1(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            var control = new LearnCategoriesController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 0. Initialize data
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }
            else if (hid == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
            }
            var existamt = context.LearnCategories.Where(p => p.HomeID == hid || p.HomeID == null).Count();

            // 1. Read all categories
            var items = control.Get();
            var itemcnt = items.Count();
            Assert.Equal(existamt, itemcnt);

            // 2. Insert new category
            var ctgy = new LearnCategory()
            {
                HomeID = hid,
                Name = "Test 1_UT_" + hid.ToString(),
                Comment = "Test 1"
            };
            var rst1 = await control.Post(ctgy);
            Assert.NotNull(rst1);
            var rst2 = Assert.IsType<CreatedODataResult<LearnCategory>>(rst1);
            Assert.Equal(ctgy.Name, rst2.Entity.Name);
            var firstctg = rst2.Entity.ID;
            Assert.True(firstctg > 0);
            ctgiesCreated.Add(firstctg);
            Assert.Equal(ctgy.Comment, rst2.Entity.Comment);

            // 3. Read all categories
            items = control.Get();
            itemcnt = items.Count();
            Assert.Equal(existamt + 1, itemcnt);

            // 4. Change the category's name
            ctgy.Name = "Test 2";
            rst1 = await control.Put(firstctg, ctgy);
            var rst3 = Assert.IsType<UpdatedODataResult<LearnCategory>>(rst1);
            Assert.Equal(ctgy.Name, rst3.Entity.Name);

            // 5. Delete it
            var rst4 = await control.Delete(firstctg);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst4);
            Assert.Equal(204, rst6.StatusCode);
            ctgiesCreated.Clear();

            // 6. Read all categories again
            items = control.Get();
            itemcnt = items.Count();
            Assert.Equal(existamt, itemcnt);

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            var context = this.fixture.GetCurrentDataContext();
            foreach (var ctgyid in ctgiesCreated)
                fixture.DeleteLearnCategory(context, ctgyid);

            ctgiesCreated.Clear();

            context.SaveChanges();
        }
    }
}
