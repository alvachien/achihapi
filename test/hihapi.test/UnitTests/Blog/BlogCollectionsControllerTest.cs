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
using Moq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace hihapi.test.UnitTests.Blog
{
    [Collection("HIHAPI_UnitTests#1")]
    public class BlogCollectionsControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public BlogCollectionsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA)]
        public async Task TestCase1(string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            fixture.InitBlogTestData(context);

            //var control = new FinanceAccountsController(context);
            //var userclaim = DataSetupUtility.GetClaimForUser(user);
            //var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            //control.ControllerContext = new ControllerContext()
            //{
            //    HttpContext = httpctx
            //};
            //var acntamt = (from homemem in context.HomeMembers
            //               join finacnt in context.FinanceAccount
            //               on new { homemem.HomeID, homemem.User } equals new { finacnt.HomeID, User = user }
            //               select finacnt.ID).ToList().Count();

            //// Step 1. Read all
            //var rsts = control.Get();
            //var rstscnt = await rsts.CountAsync();
            //var cnt1 = DataSetupUtility.Currencies.Count;
            //Assert.Equal(cnt1, rstscnt);

            //// Step 2. Read currency with select
            //var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            //httpContext.Request.Path = "/api/Currencies";
            //httpContext.Request.QueryString = new QueryString("?$select=Name");
            //httpContext.Request.Method = "GET";
            //var routeData = new RouteData();
            //routeData.Values.Add("odataPath", "Currencies");
            //routeData.Values.Add("action", "GET");

            ////Controller needs a controller context 
            //var controllerContext = new ControllerContext()
            //{
            //    RouteData = routeData,
            //    HttpContext = httpContext,
            //};
            //// Assign context to controller
            //control = new CurrenciesController(context)
            //{
            //    ControllerContext = controllerContext,
            //};
            //rsts = control.Get();
            //Assert.NotNull(rsts);

            //await context.DisposeAsync();
        }
    }
}
