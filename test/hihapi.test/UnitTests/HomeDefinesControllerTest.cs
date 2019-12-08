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
    [Collection("HIHAPI_UnitTests#1")]
    public class HomeDefinesControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public HomeDefinesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1()
        {
            HomeDefinesController control = new HomeDefinesController(this.fixture.CurrentDataContext);
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            // badRequestResult.Value

            // For user A, 2 home defines
            //var mockContext = new Mock<HttpContext>(MockBehavior.Strict);
            //mockContext.SetupGet(hc => hc.User.Identity.Name).Returns("USERA");
            var user = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);

            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var result = control.Get();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IQueryable<HomeDefine>>(okResult.Value);
            var cnt = returnValue.Count();
            Assert.Equal(2, cnt);

            // For user B, 3 home defines
            user = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserB);
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
}

