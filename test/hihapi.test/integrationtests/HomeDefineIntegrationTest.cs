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
            var clientWithAuth = _factory.CreateClient();
            clientWithAuth.SetBearerToken(token);

            // Step 1. Metadata request
            var metadata = await this._client.GetAsync("/api/$metadata");
            Assert.Equal(HttpStatusCode.OK, metadata.StatusCode);
            var content = await metadata.Content.ReadAsStringAsync();
            if (content.Length > 0)
            {
                // How to verify metadata?
                // TBD.
            }

            // Step 2. Read Home Defines - Non authority case
            var req1 = await this._client.GetAsync("/api/HomeDefines");
            Assert.Equal(HttpStatusCode.Unauthorized, req1.StatusCode);

            // Step 3. Read Home Defines - Authority case
            var req2 = await clientWithAuth.GetAsync("/api/HomeDefines");
            Assert.True(req2.IsSuccessStatusCode);
            
        }
    }
}

