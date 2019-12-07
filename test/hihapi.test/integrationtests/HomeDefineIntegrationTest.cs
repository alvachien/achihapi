using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using hihapi;
using hihapi.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace hihapi.test.integrationtests
{
    public class HomeDefineIntegrationTest : BasicIntegrationTest
    {
        public HomeDefineIntegrationTest(CustomWebApplicationFactory<hihapi.Startup> factory)
            : base(factory)
        { }

        [Fact]
        public async Task TestCase1_UserA()
        {
            string token = await IdentityServerSetup.Instance.GetAccessTokenForUser(DataSetupUtility.UserA, DataSetupUtility.IntegrationTestPassword);
            var client = _factory.CreateClient();
            client.SetBearerToken(token);

            // Step 1. Metadata request
            var metadata = await client.GetAsync("/api/$metadata");
            Assert.Equal(HttpStatusCode.OK, metadata.StatusCode);
            var content = await metadata.Content.ReadAsStringAsync();
            if (content.Length > 0)
            {
                // How to verify metadata?
                // TBD.
            }
        }
    }
}

