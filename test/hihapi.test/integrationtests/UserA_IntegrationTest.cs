using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using hihapi;
using hihapi.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Converters;
using Microsoft.AspNet.OData.Results;

namespace hihapi.test.integrationtests
{
    public class UserA_IntegrationTest : BasicIntegrationTest
    {
        public UserA_IntegrationTest(CustomWebApplicationFactory<hihapi.Startup> factory)
            : base(factory)
        { }

        [Fact]
        public async Task TestCase1()
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
            var resp2 = await clientWithAuth.GetAsync("/api/HomeDefines");
            Assert.True(resp2.IsSuccessStatusCode);
            string result = resp2.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(result))
            {
                JToken outer = JToken.Parse(result);

                // Old way to deserialize the arry
                JArray inner = outer["value"].Value<JArray>();
                var dfs = inner.ToObject<List<HomeDefine>>();
                Assert.Equal(2, dfs.Count);

                // For user A, Home1 Is a must
                var bHome1Exist = false;
                foreach (var df in dfs)
                {
                    Assert.NotNull(df);
                    Assert.True(df.ID > 0);
                    Assert.False(String.IsNullOrEmpty(df.Name));
                    Assert.Null(df.HomeMembers);
                    if (df.ID == DataSetupUtility.Home1ID)
                        bHome1Exist = true;
                }
                Assert.True(bHome1Exist);
            }

            // Step 4. Read home defines - with home members
            resp2 = await clientWithAuth.GetAsync("/api/HomeDefines?$expand=HomeMembers");
            Assert.True(resp2.IsSuccessStatusCode);
            result = resp2.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(result))
            {
                JToken outer = JToken.Parse(result);

                JArray inner = outer["value"].Value<JArray>();
                var dfs = inner.ToObject<List<HomeDefine>>();
                Assert.Equal(2, dfs.Count);

                // For user A, Home1 Is a must
                foreach (var df in dfs)
                {
                    Assert.NotNull(df);
                    Assert.True(df.ID > 0);
                    Assert.False(String.IsNullOrEmpty(df.Name));
                    Assert.NotNull(df.HomeMembers);
                    var exist = df.HomeMembers.Single(p => p.User == DataSetupUtility.UserA);
                    Assert.NotNull(exist);
                }
            }

            var hid = DataSetupUtility.Home1ID;
            var cc1id = 0;
            var ord1id = 0;
            var jsetting = new JsonSerializerSettings();
            jsetting.Converters.Add(new StringEnumConverter());
            jsetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Step 5. Create a control center
            var cc = new FinanceControlCenter()
            {
                HomeID = hid,
                Name = "Control Center 1",
                Comment = "Comment 1",
                Owner = DataSetupUtility.UserA
            };
            var kjson = JsonConvert.SerializeObject(cc, jsetting);
            var inputContent = new StringContent(kjson, Encoding.UTF8, "application/json");
            resp2 = await clientWithAuth.PostAsync("/api/FinanceControlCenters", inputContent);
            Assert.True(resp2.IsSuccessStatusCode);
            result = resp2.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(result))
            {
                var odatarst = JsonConvert.DeserializeObject<FinanceControlCenter>(result);
                Assert.Equal(odatarst.Name, cc.Name);
                Assert.Equal(odatarst.HomeID, cc.HomeID);
                Assert.Equal(odatarst.Owner, cc.Owner);
                cc1id = odatarst.ID;
                Assert.True(cc1id > 0);
            }

            // Step 6. Create an order
            var ord = new FinanceOrder()
            {
                HomeID = hid,
                Name = "Order 1",
                Comment = "Comment 1"
            };
            var srule = new FinanceOrderSRule()
            {
                Order = ord,
                RuleID = 1,
                ControlCenterID = cc1id,
                Precent = 100
            };
            ord.SRule.Add(srule);
            kjson = JsonConvert.SerializeObject(ord, jsetting);
            inputContent = new StringContent(kjson, Encoding.UTF8, "application/json");
            resp2 = await clientWithAuth.PostAsync("/api/FinanceOrders", inputContent);
            Assert.True(resp2.IsSuccessStatusCode);
            result = resp2.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(result))
            {
                var odatarst = JsonConvert.DeserializeObject<FinanceOrder>(result);
                Assert.Equal(odatarst.Name, ord.Name);
                ord1id = odatarst.ID;
                Assert.True(ord1id > 0);
            }
        }
    }
}

