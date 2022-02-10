using System;
using Xunit;
using hihapi.Controllers;
using System.Threading.Tasks;
using hihapi.test.common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using hihapi.Models;
using System.Linq;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.unittest.Blog
{
    [Collection("HIHAPI_UnitTests#1")]
    public class BlogPostsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public BlogPostsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            var context = fixture.GetCurrentDataContext();
            fixture.InitBlogTestData(context);
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.UserC)]
        public async Task TestCase_Read(String user)
        {
            var context = fixture.GetCurrentDataContext();
            var control = new BlogPostsController(context);

            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var getall = control.Get();
            Assert.NotNull(getall);
            var getallok = Assert.IsType<OkObjectResult>(getall);
            var getallrst = Assert.IsAssignableFrom<IQueryable<BlogPost>>(getallok.Value);
            Assert.NotNull(getallrst);

            //await getallrst.ForEachAsync(item =>
            //{
            //    var readrst = control.Get(item.ID);
            //    Assert.Equal(item.Comment, readrst.Comment);
            //    Assert.Equal(item.Name, readrst.Name);
            //});

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, "Test 1", "Test 1", "Test 1")]
        [InlineData(DataSetupUtility.UserC, "Test 1", "Test 1", "Test 1")]
        public async Task TestCase_BasicOperation(String user, String title, String brief, String content)
        {
            var context = fixture.GetCurrentDataContext();
            var control = new BlogPostsController(context);

            var npost = new BlogPost();
            npost.Owner = user;
            npost.Title = title;
            npost.Brief = brief;
            npost.Content = content;

            try
            {
                await control.Post(npost);
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var isexist = (from usrsetting in context.BlogUserSettings
                           where usrsetting.Owner == user
                           select usrsetting).Count();
            if (isexist == 1)
            {
                // Ensure the create is success
                var postrst = await control.Post(npost);
                var createdpostrst = Assert.IsType<CreatedODataResult<BlogPost>>(postrst);
                Assert.Equal(npost.Title, createdpostrst.Entity.Title);
                Assert.Equal(npost.Brief, createdpostrst.Entity.Brief);
                Assert.Equal(npost.Content, createdpostrst.Entity.Content);

                // Then, read it out
                var getsinglerst = control.Get(createdpostrst.Entity.ID);
                Assert.NotNull(getsinglerst);
                Assert.Equal(npost.Title, getsinglerst.Title);
                Assert.Equal(npost.Brief, getsinglerst.Brief);
                Assert.Equal(npost.Content, getsinglerst.Content);

                // Then, Delete it
                var delrst = await control.Delete(createdpostrst.Entity.ID);
                Assert.NotNull(delrst);
                var delcoderst = Assert.IsType<StatusCodeResult>(delrst);
                Assert.Equal(204, delcoderst.StatusCode);
            }
            else
            {
                try
                {
                    await control.Post(npost);
                }
                catch (Exception exp)
                {
                    Assert.IsType<BadRequestException>(exp);
                }
            }

            await context.DisposeAsync();
        }
    }
}
