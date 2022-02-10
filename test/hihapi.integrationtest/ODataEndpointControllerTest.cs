using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace hihapi.integrationtest
{
    [Collection("HIHAPI_IntegrationTests#1")]
    public class ODataEndpointControllerTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public ODataEndpointControllerTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetODataPage()
        {
            // Arrange
            var defaultPage = await _client.GetAsync("/$odata");
            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);

            ////Act
            //var response = await _client.SendAsync(
            //    (IHtmlFormElement)content.QuerySelector("form[id='messages']"),
            //    (IHtmlButtonElement)content.QuerySelector("button[id='deleteAllBtn']"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            //Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            //Assert.Equal("/", response.Headers.Location.OriginalString);
        }
    }
}
