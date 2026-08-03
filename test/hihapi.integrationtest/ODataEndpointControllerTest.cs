using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace hihapi.integrationtest
{
    [Collection("HIHAPI_IntegrationTests#1")]
    public class ODataEndpointControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ODataEndpointControllerTest(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetODataPage()
        {
            var response = await _client.GetAsync("/$odata");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("OData Endpoint Mapping", body, System.StringComparison.Ordinal);
        }
    }
}
