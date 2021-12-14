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
//using Microsoft.AspNet.OData.Results;
using Moq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class CurrenciesControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public CurrenciesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1()
        {
            var context = this.fixture.GetCurrentDataContext();
            CurrenciesController control = new CurrenciesController(context);

            // Step 1. Read all
            var rsts = control.Get();
            var rstscnt = await rsts.CountAsync();
            var cnt1 = DataSetupUtility.Currencies.Count;
            Assert.Equal(cnt1, rstscnt);

            // Step 2. Read currency with select
            var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
            httpContext.Request.Path = "/api/Currencies";
            httpContext.Request.QueryString = new QueryString("?$select=Name");
            httpContext.Request.Method = "GET";
            var routeData = new RouteData();
            routeData.Values.Add("odataPath", "Currencies");
            routeData.Values.Add("action", "GET");

            //Controller needs a controller context 
            var controllerContext = new ControllerContext()
            {
                RouteData = routeData,
                HttpContext = httpContext,
            };
            // Assign context to controller
            control = new CurrenciesController(context)
            {
                ControllerContext = controllerContext,
            };
            rsts = control.Get();
            Assert.NotNull(rsts);

            await context.DisposeAsync();
        }
    }
}
