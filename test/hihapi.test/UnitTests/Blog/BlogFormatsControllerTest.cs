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

namespace hihapi.test.UnitTests.Blog
{
    [Collection("HIHAPI_UnitTests#1")]
    public class BlogFormatsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public BlogFormatsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase_Read()
        {
            var context = fixture.GetCurrentDataContext();
            var control = new BlogFormatsController(context);

            var getall = control.Get();
            Assert.NotNull(getall);
            var getallok = Assert.IsType<OkObjectResult>(getall);
            var getallrst = Assert.IsAssignableFrom<IQueryable<BlogFormat>>(getallok.Value);
            Assert.NotNull(getallrst);

            await getallrst.ForEachAsync(item =>
            {
                var readrst = control.Get(item.ID);
                Assert.Equal(item.Comment, readrst.Comment);
                Assert.Equal(item.Name, readrst.Name);
            });

            await context.DisposeAsync();
        }
    }
}
