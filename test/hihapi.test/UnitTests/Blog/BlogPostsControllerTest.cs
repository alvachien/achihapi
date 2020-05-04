using System;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Results;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class BlogPostsControllerTest : IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> objectsCreated = new List<Int32>();

        public BlogPostsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.provider = UnitTestUtility.GetServiceProvider();

            this.model = UnitTestUtility.GetEdmModel<BlogPost>(provider, "BlogPosts");
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
        public async Task TestCase1(string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            fixture.InitBlogTestData(context);

            var control = new BlogPostsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            var existedamt = (from coll in context.BlogPosts where coll.Owner == user select coll).ToList().Count();

            // Step 1. Read all
            var rsts = control.Get();
            var rstscnt = await rsts.CountAsync();
            Assert.Equal(existedamt, rstscnt);

            // Step 2. Create one new collection
            var newcoll = new BlogPost()
            {
                Owner = user,
                Title = "TestCase1_Add_" + user,
                Brief = "TestCase1_Add_" + user,
                Content = "TestCase1_Add_" + user,
            };
            var rst = await control.Post(newcoll);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<BlogPost>>(rst);
            objectsCreated.Add(rst2.Entity.ID);
            newcoll.ID = rst2.Entity.ID;
            Assert.Equal(rst2.Entity.Title, newcoll.Title);
            Assert.Equal(rst2.Entity.Brief, newcoll.Brief);
            Assert.Equal(rst2.Entity.Content, newcoll.Content);
            Assert.Equal(rst2.Entity.Owner, user);

            // Step 3. Read all 
            rsts = control.Get();
            rstscnt = await rsts.CountAsync();
            Assert.Equal(existedamt + 1, rstscnt);

            // Step 4. Change it
            newcoll.Title = "Tobe Delteed";
            var rst3 = await control.Put(newcoll.ID, newcoll);
            Assert.NotNull(rst3);
            var rst3a = Assert.IsType<UpdatedODataResult<BlogPost>>(rst3);
            Assert.Equal(newcoll.Title, rst3a.Entity.Title);

            // Step 5. Delete it
            var rst5 = await control.Delete(newcoll.ID);
            Assert.NotNull(rst5);
            objectsCreated.Remove(newcoll.ID);
            var rst5a = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst5a.StatusCode);

            // Step 6. Read it again
            rsts = control.Get();
            rstscnt = await rsts.CountAsync();
            Assert.Equal(existedamt, rstscnt);

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            if (objectsCreated.Count > 0)
            {
                var context = this.fixture.GetCurrentDataContext();
                foreach (var acntcrt in objectsCreated)
                    fixture.DeleteBlogPost(context, acntcrt);

                objectsCreated.Clear();
                context.SaveChanges();
            }
        }
    }
}
