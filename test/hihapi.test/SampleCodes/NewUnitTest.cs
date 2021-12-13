using System;
using System.Collections.Generic;
using System.Text;
//using Microsoft.AspNet.OData;
//using Microsoft.AspNet.OData.Builder;
//using Microsoft.AspNet.OData.Extensions;
//using Microsoft.AspNet.OData.Query;
//using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace hihapi.test.UnitTests
{
    public class NewUnitTest
    {
        //// The following example code copied from
        //// https://github.com/OData/WebApi/issues/1352
        //// However, it focus on the validation testing
        ////

        //private HttpContext _httpContext;
        //private ServiceProvider _provider;
        //private ODataValidationSettings _settings;

        //public NewUnitTest()
        //{
        //    _settings = new ODataValidationSettings
        //    {
        //        AllowedQueryOptions = AllowedQueryOptions.Top
        //    };

        //    var collection = new ServiceCollection();

        //    collection.AddOData();
        //    collection.AddODataQueryFilter();
        //    collection.AddTransient<ODataUriResolver>();
        //    collection.AddTransient<ODataQueryValidator>();
        //    collection.AddTransient<TopQueryValidator>();

        //    _provider = collection.BuildServiceProvider();

        //    var routeBuilder = new RouteBuilder(Mock.Of<IApplicationBuilder>(x => x.ApplicationServices == _provider));
        //    routeBuilder.EnableDependencyInjection();
        //}

        ////[Fact]
        //public void Some_Unit_Test()
        //{
        //    // Arrange
        //    var context = GetActionContextFor("$top=1");
        //    // var target = new OurCustomActionFilterAttribute();

        //    // Act
        //    // target.OnActionExecuting(context);

        //    // Assert
        //    // Assert some stuff about it.
        //}

        //private ActionExecutingContext GetActionContextFor(string url)
        //{
        //    var uri = new Uri($"http://localhost/api/TestType?{url}");

        //    _httpContext = new DefaultHttpContext
        //    {
        //        RequestServices = _provider
        //    };

        //    // ReSharper disable once UnusedVariable
        //    HttpRequest request = new DefaultHttpRequest(_httpContext)
        //    {
        //        Method = "GET",
        //        Host = new HostString(uri.Host, uri.Port),
        //        Path = uri.LocalPath,
        //        QueryString = new QueryString(uri.Query)
        //    };

        //    // ReSharper disable once UnusedVariable
        //    HttpResponse response = new DefaultHttpResponse(_httpContext)
        //    {
        //        StatusCode = StatusCodes.Status200OK
        //    };

        //    var modelBuilder = new ODataConventionModelBuilder(_provider);
        //    var entitySet = modelBuilder.EntitySet<TestType>("TestType");
        //    entitySet.EntityType.HasKey(entity => entity.SomeProperty);
        //    var model = modelBuilder.GetEdmModel();

        //    var actionArguments = new Dictionary<string, object>();
        //    var actionContext = new ActionExecutingContext(new ActionContext(_httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary()), new List<IFilterMetadata>(), actionArguments, null);
        //    var context = new ODataQueryContext(model, typeof(TestType), new Microsoft.AspNet.OData.Routing.ODataPath());
        //    var options = new ODataQueryOptions<TestType>(context, actionContext.HttpContext.Request);
        //    actionArguments.Add("options", options);
        //    return actionContext;
        //}

        //internal class TestType
        //{
        //    public string SomeProperty { get; set; }
        //}
    }
}
