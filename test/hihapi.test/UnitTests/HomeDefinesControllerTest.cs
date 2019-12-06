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
using Moq;
using System.Security.Claims;
using System.Collections.Generic;

namespace hihapi.test.UnitTests
{
    [Collection("Collection#1")]
    public class HomeDefinesControllerTest
    {
        [Fact]
        public async Task TestCase1()
        {
            hihDataContext.TestingMode = true;

            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<hihDataContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new hihDataContext(options))
                {
                    context.Database.EnsureCreated();

                    // Insert some home define data
                    DataSetupUtility.SetupTable_HomeDefineAndMember();
                    context.HomeDefines.AddRange(DataSetupUtility.listHomeDefine);
                    context.HomeMembers.AddRange(DataSetupUtility.listHomeMember);
                    await context.SaveChangesAsync();

                    HomeDefinesController control = new HomeDefinesController(context);
                    var result = control.Get();
                    Assert.IsType<BadRequestResult>(result);
                    // badRequestResult.Value

                    // For user A, 2 home defines
                    //var mockContext = new Mock<HttpContext>(MockBehavior.Strict);
                    //mockContext.SetupGet(hc => hc.User.Identity.Name).Returns("USERA");
                    var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserA),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserA),
                    }, "mock"));

                    control.ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = user }
                    };
                    result = control.Get();
                    var okResult = Assert.IsType<OkObjectResult>(result);
                    var returnValue = Assert.IsAssignableFrom<IQueryable<HomeDefine>>(okResult.Value);
                    var cnt = returnValue.Count();
                    Assert.Equal(2, cnt);

                    // For user B, 3 home defines
                    user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserB),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserB),
                    }, "mock"));

                    control.ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = user }
                    };
                    result = control.Get();
                    okResult = Assert.IsType<OkObjectResult>(result);
                    returnValue = Assert.IsAssignableFrom<IQueryable<HomeDefine>>(okResult.Value);
                    cnt = returnValue.Count();
                    Assert.Equal(3, cnt);
                }
            }
            finally
            {
                connection.Close();
            }

            //hihDataContext.TestingMode = false;
        }
    }
}

