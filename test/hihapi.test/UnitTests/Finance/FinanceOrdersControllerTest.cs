using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceOrdersControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> ordersCreated = new List<Int32>();

        public FinanceOrdersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            var modelBuilder = UnitTestUtility.GetModelBuilder(this.provider);
            modelBuilder.EntitySet<FinanceOrder>("FinanceOrders");
            modelBuilder.EntitySet<FinanceOrderSRule>("FinanceOrderSRules");
            this.model = modelBuilder.GetEdmModel();
        }

        public void Dispose()
        {
            CleanupCreatedEntries();

            if (this.provider != null)
            {
                this.provider.Dispose();
                this.provider = null;
            }
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB)]
        public async Task TestCase1(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }
            if (hid == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
            }
            if (hid == DataSetupUtility.Home3ID)
            {
                fixture.InitHome3TestData(context);
            }
            if (hid == DataSetupUtility.Home4ID)
            {
                fixture.InitHome4TestData(context);
            }
            if (hid == DataSetupUtility.Home5ID)
            {
                fixture.InitHome5TestData(context);
            }

            // 1. Prepare dta
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            var listCCs = context.FinanceControlCenter.Where(p => p.HomeID == hid).ToList<FinanceControlCenter>();

            var existamt = (from homemem in context.HomeMembers
                              join finord in context.FinanceOrder
                              on new { homemem.HomeID, homemem.User } equals new { finord.HomeID, User = user }
                              select finord.ID).ToList().Count();
            var existamt_curhome = context.FinanceOrder.Where(p => p.HomeID == hid).Count();

            // 2. Create order
            var control = new FinanceOrdersController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
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
                ControlCenterID = listCCs[0].ID,
                Precent = 100
            };
            ord.SRule.Add(srule);
            var rst = await control.Post(ord);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceOrder>>(rst);
            Assert.Equal(rst2.Entity.Name, ord.Name);
            var oid = rst2.Entity.ID;
            Assert.True(oid > 0);
            ordersCreated.Add(oid);

            // 3. Read the order out (without Home ID)
            var queryUrl = "http://localhost/api/FinanceOrders";
            var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceOrder>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceOrder>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt + 1, rst3.Cast<FinanceOrder>().Count());

            // 3a. Read the order out (with Home ID)
            queryUrl = "http://localhost/api/FinanceOrders?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceOrder>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceOrder>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt_curhome + 1, rst3.Cast<FinanceOrder>().Count());

            // 4a. Change one order - Add new rule
            var norder = rst2.Entity;
            norder.Name = "New Order";
            norder.SRule.ElementAt(0).Precent = 80;
            norder.SRule.Add(new FinanceOrderSRule
            {
                Order = norder,
                RuleID = 2,
                ControlCenterID = listCCs.Count > 1 ? listCCs[1].ID : listCCs[0].ID,
                Precent = 20
            });
            rst = await control.Put(norder.ID, norder);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceOrder>>(rst);
            Assert.Equal(norder.Name, rst4.Entity.Name);
            // Re-read the order
            //queryUrl = "http://localhost/api/FinanceOrders?$filter=HomeID eq " + hid.ToString() + " and ID eq " + norder.ID.ToString() + "&$expand=SRule";
            //req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            //options = UnitTestUtility.GetODataQueryOptions<FinanceOrder>(odatacontext, req);
            //rst3 = control.Get(options);
            //var rst3a = rst3.Cast<FinanceOrder>();
            //Assert.True(rst3a.Count() == 1);
            //var rst3a_elem = rst3a.ElementAt(0);
            //Assert.True(rst3a_elem.SRule.Count == 2);
            ////var rst3a = Assert.IsType<FinanceOrder>(rst3);
            ////Assert.True(rst3a.SRule.Count == 2);

            // 5. Delete an order
            var rst5 = await control.Delete(oid);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);
            Assert.Equal(0, context.FinanceOrderSRule.Where(p => p.OrderID == oid).Count());
            ordersCreated.Clear();

            // 6. Read the order again
            queryUrl = "http://localhost/api/FinanceOrders?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            options = UnitTestUtility.GetODataQueryOptions<FinanceOrder>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt_curhome, rst3.Cast<FinanceOrder>().Count());

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            if (ordersCreated.Count > 0)
            {
                var context = this.fixture.GetCurrentDataContext();
                foreach (var ord in ordersCreated)
                    fixture.DeleteFinanceOrder(context, ord);

                ordersCreated.Clear();
                context.SaveChanges();
            }
        }
    }
}
