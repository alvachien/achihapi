using System;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNet.OData.Results;
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

            var existedcoll = (from coll in context.BlogCollections where coll.Owner == user select coll).ToList();
            var existedamt = (from coll in context.BlogPosts where coll.Owner == user select coll).ToList().Count();

            // Step 1. Read all
            var rsts = control.Get();
            var rstscnt = await rsts.CountAsync();
            Assert.Equal(existedamt, rstscnt);

            // Step 2. Create one new post
            var newpost = new BlogPost()
            {
                Owner = user,
                Title = "TestCase1_Add_" + user,
                Brief = "TestCase1_Add_" + user,
                Content = "TestCase1_Add_" + user,
            };
            var rst = await control.Post(newpost);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<BlogPost>>(rst);
            objectsCreated.Add(rst2.Entity.ID);
            newpost.ID = rst2.Entity.ID;
            Assert.Equal(rst2.Entity.Title, newpost.Title);
            Assert.Equal(rst2.Entity.Brief, newpost.Brief);
            Assert.Equal(rst2.Entity.Content, newpost.Content);
            Assert.Equal(rst2.Entity.Owner, user);

            // Step 3. Read all 
            rsts = control.Get();
            rstscnt = await rsts.CountAsync();
            Assert.Equal(existedamt + 1, rstscnt);

            // Step 4. Change it - Title and Add new tag
            newpost.Title = "Tobe Delteed";
            newpost.BlogPostTags.Add(new BlogPostTag
            {
                Tag = "Test",
                PostID = newpost.ID,
            });
            var rst3 = await control.Put(newpost.ID, newpost);
            Assert.NotNull(rst3);
            var rst3a = Assert.IsType<OkObjectResult>(rst3);
            var rst3b = rst3a.Value as BlogPost;
            Assert.Equal(newpost.Title, rst3b.Title);
            Assert.True(rst3b.BlogPostTags.Count == 1);
            Assert.Equal("Test", rst3b.BlogPostTags.ElementAt(0).Tag);

            // Step 4a. Change it - Remove tag and add two new tags
            newpost.BlogPostTags.Clear();
            newpost.BlogPostTags.Add(new BlogPostTag
            {
                Tag = "Test2",
                PostID = newpost.ID,
            });
            newpost.BlogPostTags.Add(new BlogPostTag
            {
                Tag = "Test2a",
                PostID = newpost.ID,
            });
            rst3 = await control.Put(newpost.ID, newpost);
            Assert.NotNull(rst3);
            rst3a = Assert.IsType<OkObjectResult>(rst3);
            rst3b = rst3a.Value as BlogPost;
            Assert.Equal(newpost.Title, rst3b.Title);
            Assert.True(rst3b.BlogPostTags.Count == 2);
            Assert.Equal("Test2", rst3b.BlogPostTags.ElementAt(0).Tag);
            Assert.Equal("Test2a", rst3b.BlogPostTags.ElementAt(1).Tag);

            // Step 4b. Change it - add new collection
            if (existedcoll.Count > 0)
            {
                newpost.BlogPostCollections.Add(new BlogPostCollection
                {
                    PostID = newpost.ID,
                    CollectionID = existedcoll[0].ID
                });
                rst3 = await control.Put(newpost.ID, newpost);
                Assert.NotNull(rst3);
                rst3a = Assert.IsType<OkObjectResult>(rst3);
                rst3b = rst3a.Value as BlogPost;
                Assert.Equal(newpost.Title, rst3b.Title);
                Assert.True(rst3b.BlogPostTags.Count == 2);
                Assert.True(rst3b.BlogPostCollections.Count == 1);
                Assert.Equal(existedcoll[0].ID, rst3b.BlogPostCollections.ElementAt(0).CollectionID);

                if (existedcoll.Count > 1)
                {
                    newpost.BlogPostCollections.Clear();
                    newpost.BlogPostCollections.Add(new BlogPostCollection
                    {
                        PostID = newpost.ID,
                        CollectionID = existedcoll[1].ID
                    });
                    rst3 = await control.Put(newpost.ID, newpost);
                    Assert.NotNull(rst3);
                    rst3a = Assert.IsType<OkObjectResult>(rst3);
                    rst3b = rst3a.Value as BlogPost;
                    Assert.Equal(newpost.Title, rst3b.Title);
                    Assert.True(rst3b.BlogPostTags.Count == 2);
                    Assert.True(rst3b.BlogPostCollections.Count == 1);
                    Assert.Equal(existedcoll[1].ID, rst3b.BlogPostCollections.ElementAt(0).CollectionID);
                }
            }

            // Step 5. Delete it
            var rst5 = await control.Delete(newpost.ID);
            Assert.NotNull(rst5);
            objectsCreated.Remove(newpost.ID);
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
