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

namespace hihapi.test.UnitTests
{
    [Collection("Collection#1")]
    public class CurrenciesControllerTest
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

                    DataSetupUtility.InitialTable_Currency(context);
                    await context.SaveChangesAsync();

                    CurrenciesController control = new CurrenciesController(context);
                    
                    // Step 1. Read all
                    var rsts = control.Get();
                    var rstscnt = await rsts.CountAsync();
                    var cnt1 = DataSetupUtility.listCurrency.Count;                    
                    Assert.Equal(cnt1, rstscnt);

                    //var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
                    //Expression<Func<IUrlHelper, string>> urlSetup
                    //    = url => url.Action(It.Is<UrlActionContext>(uac => uac.Action == "Get" && GetId(uac.Values) != cmd.Id));
                    //mockUrlHelper.Setup(urlSetup).Returns("a/mock/url/for/testing").Verifiable();

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

                    //var routes = new RouteCollection();
                    //MvcApplication.RegisterRoutes(routes);

                    //var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
                    //request.SetupGet(x => x.ApplicationPath).Returns("/");
                    //request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/a", UriKind.Absolute));
                    //request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());

                    //var response = new Mock<HttpResponseBase>(MockBehavior.Strict);
                    //response.Setup(x => x.ApplyAppPathModifier("/post1")).Returns("http://localhost/post1");

                    //var context = new Mock<HttpContextBase>(MockBehavior.Strict);
                    //context.SetupGet(x => x.Request).Returns(request.Object);
                    //context.SetupGet(x => x.Response).Returns(response.Object);

                    //var controller = new LinkbackController(dbF.Object);
                    //controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
                    //controller.Url = new UrlHelper(new RequestContext(context.Object, new RouteData()), routes);

                    // // Step 2. Create a new one
                    // var nmod = new Knowledge() {
                    //     Title = "Test 1",
                    //     Category = KnowledgeCategory.Concept,
                    //     Content = "My test 1"
                    // };
                    // var result = control.Post(nmod);
                    // var actionResult = Assert.IsType<Task<IActionResult>>(result);
                    // var actResult = Assert.IsType<CreatedODataResult<Knowledge>>(result.Result);
                    // rstscnt = await context.Knowledges.CountAsync();
                    // Assert.Equal(1, rstscnt);

                    // var nid = actResult.Entity.ID;
                    // var dbrst = await context.Knowledges.SingleOrDefaultAsync(p => p.ID == nid);
                    // Assert.Equal(dbrst.Title, nmod.Title);
                    // Assert.Equal(dbrst.Content, nmod.Content);
                    // Assert.Equal(dbrst.Category, nmod.Category);

                    // // Step 3. Re-read
                    // rsts = control.Get();
                    // rstscnt = await rsts.CountAsync();
                    // Assert.Equal(1, rstscnt);
                    // var firstrst = rsts.ToList()[0];
                    // Assert.Equal(firstrst.Title, nmod.Title);
                    // Assert.Equal(firstrst.Content, nmod.Content);
                    // Assert.Equal(firstrst.Category, nmod.Category);
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
