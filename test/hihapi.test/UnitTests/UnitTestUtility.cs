using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.UriParser;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.OData.Edm;
using Moq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace hihapi.test.UnitTests
{
    public class UnitTestUtility
    {
        public static ServiceProvider GetServiceProvider()
        {
            var collection = new ServiceCollection();

            collection.AddOData();
            collection.AddODataQueryFilter();
            collection.AddTransient<ODataUriResolver>();
            collection.AddTransient<ODataQueryValidator>();
            collection.AddTransient<TopQueryValidator>();
            collection.AddTransient<FilterQueryValidator>();
            collection.AddTransient<SkipQueryValidator>();
            collection.AddTransient<OrderByQueryValidator>();

            var provider = collection.BuildServiceProvider();
            var routeBuilder = new RouteBuilder(Mock.Of<IApplicationBuilder>(x => x.ApplicationServices == provider));
            routeBuilder.EnableDependencyInjection();

            return provider;
        }

        public static ODataConventionModelBuilder GetModelBuilder(ServiceProvider provider)
        {
            return new ODataConventionModelBuilder(provider);
        }

        public static IEdmModel GetEdmModel<T>(ServiceProvider provider, String entitySetName) where T : class
        {
            var modelBuilder = new ODataConventionModelBuilder(provider);
            modelBuilder.EntitySet<T>(entitySetName);
            return modelBuilder.GetEdmModel();
        }

        public static IEdmModel GetEdmModel<T>(ODataConventionModelBuilder modelBuilder, String entitySetName) where T : class
        {            
            modelBuilder.EntitySet<T>(entitySetName);
            return modelBuilder.GetEdmModel();
        }

        public static DefaultHttpContext GetDefaultHttpContext(ServiceProvider provider, ClaimsPrincipal user)
        {
            return new DefaultHttpContext() {
                RequestServices = provider,
                User = user 
            };
        }

        public static HttpRequest GetHttpRequest(DefaultHttpContext context, String method, String url)
        {
            var uri = new Uri(url);
            var req = context.Request;
            req.Method = method;
            req.Host = new HostString(uri.Host, uri.Port);
            req.Path = uri.LocalPath;
            req.QueryString = new QueryString(uri.Query);

            return req;
        }

        public static ODataQueryContext GetODataQueryContext<T>(IEdmModel model) where T: class
        {
            return new ODataQueryContext(model, typeof(T), new Microsoft.AspNet.OData.Routing.ODataPath());
        }

        public static ODataQueryOptions<T> GetODataQueryOptions<T>(ODataQueryContext odatacontext, HttpRequest req) where T: class
        {
            return new ODataQueryOptions<T>(odatacontext, req);
        }
    }
}
