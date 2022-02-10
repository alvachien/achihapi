using System;
using Xunit;
using hihapi.Controllers;
using System.Threading.Tasks;
using hihapi.test.common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using hihapi.Models;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.unittest.Blog
{
    [Collection("HIHAPI_UnitTests#1")]
    public class BlogCollectionsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public BlogCollectionsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            var context = fixture.GetCurrentDataContext();
            fixture.InitBlogTestData(context);
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA)]
        public async Task TestCase_Read(String user)
        {
            var context = fixture.GetCurrentDataContext();
            var control = new BlogCollectionsController(context);

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
            var getallrst = Assert.IsAssignableFrom<IQueryable<BlogCollection>>(getallok.Value);
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
        [InlineData(DataSetupUtility.UserA, "Test 1", "Test 1")]
        [InlineData(DataSetupUtility.UserC, "Test 1", "Test 1")]
        public async Task TestCase_BasicOperation(String user, String name, String comment)
        {
            var context = fixture.GetCurrentDataContext();
            var control = new BlogCollectionsController(context);

            var ncoll = new BlogCollection();
            ncoll.Name = name;
            ncoll.Comment = comment;
            ncoll.Owner = user;

            try
            {
                await control.Post(ncoll);
            }
            catch(Exception exp)
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
                var postrst = await control.Post(ncoll);
                var createdcollrst = Assert.IsType<CreatedODataResult<BlogCollection>>(postrst);
                Assert.Equal(ncoll.Name, createdcollrst.Entity.Name);
                Assert.Equal(ncoll.Comment, createdcollrst.Entity.Comment);
                Assert.Equal(ncoll.Owner, createdcollrst.Entity.Owner);

                // Then, read it out
                var getsinglerst = control.Get(createdcollrst.Entity.ID);
                Assert.NotNull(getsinglerst);
                Assert.Equal(ncoll.Name, getsinglerst.Name);
                Assert.Equal(ncoll.Comment, getsinglerst.Comment);
                Assert.Equal(ncoll.Owner, getsinglerst.Owner);

                // Then, Delete it
                var delrst = await control.Delete(getsinglerst.ID);
                Assert.NotNull(delrst);
                var delcoderst = Assert.IsType<StatusCodeResult>(delrst);
                Assert.Equal(204, delcoderst.StatusCode);
            }
            else
            {
                try
                {
                    await control.Post(ncoll);
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
