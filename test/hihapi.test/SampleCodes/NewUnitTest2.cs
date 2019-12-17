using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;
using Microsoft.AspNet.OData.Builder;
using Moq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Query;
using hihapi.Models;
using System.Web;
//using System.Web.OData;
//using System.Web.OData.Extensions;
using Microsoft.AspNet.OData.Extensions;
using hihapi.Controllers;
using Microsoft.AspNetCore.Http;

namespace hihapi.test.SampleCodes
{
    public class NewUnitTest2
    {
        private const string _serviceRoot = "http://any/";

        public void TestMethod1()
        {
            //var serviceMock = new Mock<IVendorService>();
            //serviceMock.SetReturnsDefault<IEnumerable<Vendor>>(new List<Vendor>()
            //{
            //   new Vendor() { id = "1" }
            //});

            //HttpRequest request = new HttpRequest(HttpMethod.Get, "http://localhost/api/FinanceAccountCategories");
            
            //ODataModelBuilder builder = new ODataConventionModelBuilder();
            //builder.EntitySet<FinanceAccountCategory>("FinanceAccountCategories");
            //var model = builder.GetEdmModel();
            ////ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            //ODataPath odataPath = new DefaultODataPathHandler().Parse(_serviceRoot, "FinanceAccountCategories?$top=1");
            //ODataQueryContext context = new ODataQueryContext(model, typeof(FinanceAccountCategory), odataPath);

            ////HttpRouteCollection routes = new HttpRouteCollection();
            ////HttpConfiguration config = new HttpConfiguration(routes) { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            //////HttpConfiguration config = new HttpConfiguration();

            ////////1        
            //////config.EnableDependencyInjection();

            ////////2 
            //////config.EnsureInitialized();
            /////
            ////ODataQueryContext context = new ODataQueryContext(model.Instance, elementType);
            ////ODataQueryOptions options = new ODataQueryOptions(context, request);

            //HttpConfiguration configuration = request.GetConfiguration(); 
            //if (configuration == null) 
            //{ 
            //    configuration = new HttpConfiguration(); 
            //    request.SetConfiguration(configuration); 
            //}
            //configuration.EnableDependencyInjection((Action<IContainerBuilder>)null);

            //// attempting to register at least some non-OData HTTP route doesn't seem to help
            //routes.MapHttpRoute("Default", "{controller}/{action}/{id}",
            //        new
            //        {
            //            controller = "Home",
            //            action = "Index",
            //            id = UrlParameter.Optional
            //        }
            //        );
            //config.MapODataServiceRoute("odata", "odata", model);
            //config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            //config.EnsureInitialized();

            //request.SetConfiguration(config);
            //ODataQueryContext context = new ODataQueryContext(
            //    model,
            //    typeof(Vendor),
            //    new ODataPath(
            //        new Microsoft.OData.UriParser.EntitySetSegment(
            //            model.EntityContainer.FindEntitySet("Vendor"))
            //    )
            //);


            //var controller = new FinanceAccountCategoriesController(context);

            //// InvalidOperationException in System.Web.OData on next line:
            //// No non-OData HTTP route registered
            //var options = new ODataQueryOptions<FinanceAccountCategory>(context, request);

            //var response = controller.Get() as ViewResult;
        }
    }
}
