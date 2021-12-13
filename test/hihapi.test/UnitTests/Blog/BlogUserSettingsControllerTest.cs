using System;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNet.OData.Results;
//using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class BlogUserSettingsControllerTest: IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        //private List<Int32> objectsCreated = new List<Int32>();

        public BlogUserSettingsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.provider = UnitTestUtility.GetServiceProvider();

            this.model = UnitTestUtility.GetEdmModel<BlogUserSetting>(provider, "BlogUserSetting");
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
        [InlineData(DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.UserC)]
        public async Task TestCase1(string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            fixture.InitBlogTestData(context);

            var control = new BlogUserSettingsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            var existedamt = (from coll in context.BlogUserSettings where coll.Owner == user select coll).ToList().Count();

            // Step 1. Read all
            var rsts = control.Get();
            var rstscnt = await rsts.CountAsync();
            Assert.Equal(existedamt, rstscnt);

            // Step 2. Create one new collection
            var nset = new BlogUserSetting()
            {
                Owner = user,
                Name = "Test Setting",
                Comment = "Test Comment",
                DeployFolder = "Test"
            };
            var rst = await control.Post(nset);
            Assert.NotNull(rst);
            Assert.IsType<ForbidResult>(rst);

            // Step 3. Update one
            if (rstscnt > 0)
            {
                var existsett = control.Get(user);
                Assert.NotNull(existsett);
                var existsett2 = Assert.IsType<SingleResult<BlogUserSetting>>(existsett);
                Assert.NotNull(existsett2);
                var existsett2rst = existsett2.Queryable.FirstOrDefault();
                existsett2rst.Comment = "Tobe Delteed";
                existsett2rst.Author = "Author";
                existsett2rst.AuthorDesp = "Author Desp";
                existsett2rst.AuthorImage = "Author Image";
                var rst3 = await control.Put(existsett2rst.Owner, existsett2rst);
                Assert.NotNull(rst3);
                var rst3a = Assert.IsType<OkObjectResult>(rst3);
                var rst3b = rst3a.Value as BlogUserSetting;
                Assert.Equal(existsett2rst.Comment, rst3b.Comment);
                Assert.Equal(existsett2rst.Author, rst3b.Author);
                Assert.Equal(existsett2rst.AuthorDesp, rst3b.AuthorDesp);
                Assert.Equal(existsett2rst.AuthorImage, rst3b.AuthorImage);
            }

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            //if (objectsCreated.Count > 0)
            //{
            //    var context = this.fixture.GetCurrentDataContext();
            //    foreach (var acntcrt in objectsCreated)
            //        fixture.DeleteBlogCollection(context, acntcrt);

            //    objectsCreated.Clear();
            //    context.SaveChanges();
            //}
        }
    }
}
