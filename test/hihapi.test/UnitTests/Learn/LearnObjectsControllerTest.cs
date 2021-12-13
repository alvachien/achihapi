using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNet.OData.Results;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Http;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LearnObjectsControllerTest : IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> objectsCreated = new List<Int32>();

        public LearnObjectsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<LearnObject>(provider, "LearnObjects");
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
        [InlineData(DataSetupUtility.Home1ID)]
        [InlineData(DataSetupUtility.Home2ID)]
        public void TestModel(int hid)
        {
            var obj = new LearnObject();
            Assert.False(obj.IsValid(null));

            obj.HomeID = hid;
            obj.CategoryID = 12;
            obj.Name = "test";
            obj.Content = "test";
            Assert.True(obj.IsValid(null));

            obj.HomeID = 0;
            Assert.False(obj.IsValid(null));

            obj.HomeID = hid;
            obj.CategoryID = 0;
            Assert.False(obj.IsValid(null));

            obj.CategoryID = 12;
            obj.Name = "";
            Assert.False(obj.IsValid(null));

            obj.Name = "test";
            obj.Content = "";
            Assert.False(obj.IsValid(null));
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        public async Task TestController(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            var control = new LearnObjectsController(context);
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
            var existamt = context.LearnObjects.Where(p => p.HomeID == hid).Count();
            var ctgid = context.LearnCategories.Where(p => p.HomeID == hid || p.HomeID == null).FirstOrDefault();
            var objamt = (from homemem in context.HomeMembers
                           join lobjs in context.LearnObjects
                           on new { homemem.HomeID, homemem.User } equals new { lobjs.HomeID, User = user }
                           select lobjs.ID).ToList().Count();

            // 1. Insert new Object
            var obj = new LearnObject()
            {
                HomeID = hid,
                Name = "Test_LOBJ_1_UT_" + hid.ToString(),
                CategoryID = ctgid.ID,
                Content = "Content 1"
            };
            var rst1 = await control.Post(obj);
            Assert.NotNull(rst1);
            var rst2 = Assert.IsType<CreatedODataResult<LearnObject>>(rst1);
            Assert.Equal(obj.Name, rst2.Entity.Name);
            var firstordid = rst2.Entity.ID;
            Assert.True(firstordid > 0);
            objectsCreated.Add(firstordid);

            // 2. Now read the whole accounts (no home ID applied)
            var queryUrl = "http://localhost/api/LearnObjects";
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<LearnObject>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<LearnObject>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(objamt + 1, rst3.Cast<LearnObject>().Count());

            // 2a. Read the whole accounts (with home ID applied)
            queryUrl = "http://localhost/api/LearnObjects?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceAccount>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<LearnObject>(odatacontext, req);
            rst3 = control.Get(options);
            existamt = context.LearnObjects.Where(p => p.HomeID == hid).Count();
            Assert.NotNull(rst3);
            Assert.Equal(existamt, rst3.Cast<LearnObject>().Count());

            // 3. Change the object's name
            obj.Name = "Test 2";
            rst1 = await control.Put(firstordid, obj);
            var rst3a = Assert.IsType<UpdatedODataResult<LearnObject>>(rst1);
            Assert.Equal(obj.Name, rst3a.Entity.Name);

            // 4. Delete it
            var rst4 = await control.Delete(firstordid);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst4);
            Assert.Equal(204, rst6.StatusCode);
            objectsCreated.Clear();

            // 5. Read all object again
            existamt = context.LearnObjects.Where(p => p.HomeID == hid).Count();
            var rst7 = control.Get(options);
            Assert.Equal(existamt, rst3.Cast<LearnObject>().Count());

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            var context = this.fixture.GetCurrentDataContext();
            foreach (var oid in objectsCreated)
                fixture.DeleteLearnObject(context, oid);

            objectsCreated.Clear();

            context.SaveChanges();
        }

    }
}
