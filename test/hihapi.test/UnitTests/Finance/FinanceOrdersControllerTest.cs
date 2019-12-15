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

        public FinanceOrdersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceOrder>(provider, "FinanceOrders");
        }

        public void Dispose()
        {
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
            // 0. Prepare entries for other homes
            List<FinanceControlCenter> ccInOtherHomes = new List<FinanceControlCenter>();
            List<FinanceOrder> ordInOtherHomes = new List<FinanceOrder>();
            var secondhid = hid;
            if (hid == DataSetupUtility.Home1ID)
            {
                if (user == DataSetupUtility.UserA || user == DataSetupUtility.UserB)
                {
                    secondhid = DataSetupUtility.Home3ID;
                    var cc1 = new FinanceControlCenter()
                    {
                        HomeID = secondhid,
                        Name = "Control Center 2.1",
                        Comment = "Comment 2.1",
                        Owner = user
                    };
                    var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                    ccInOtherHomes.Add(ec1.Entity);

                    var ord1 = new FinanceOrder()
                    {
                        HomeID = secondhid,
                        Name = "Order 2.1",
                        Comment = "Comment 2.1"
                    };
                    var srule1 = new FinanceOrderSRule()
                    {
                        Order = ord1,
                        RuleID = 1,
                        ControlCenterID = ec1.Entity.ID,
                        Precent = 100
                    };
                    ord1.SRule.Add(srule1);
                    var eord1 = this.fixture.CurrentDataContext.FinanceOrder.Add(ord1);
                    ordInOtherHomes.Add(eord1.Entity);
                    this.fixture.CurrentDataContext.SaveChanges();
                }
                else if (user == DataSetupUtility.UserC)
                {
                    secondhid = DataSetupUtility.Home4ID;
                    var cc1 = new FinanceControlCenter()
                    {
                        HomeID = secondhid,
                        Name = "Control Center 4.1",
                        Comment = "Comment 4.1",
                        Owner = user
                    };
                    var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                    ccInOtherHomes.Add(ec1.Entity);

                    var ord1 = new FinanceOrder()
                    {
                        HomeID = secondhid,
                        Name = "Order 4.1",
                        Comment = "Comment 4.1"
                    };
                    var srule1 = new FinanceOrderSRule()
                    {
                        Order = ord1,
                        RuleID = 1,
                        ControlCenterID = ec1.Entity.ID,
                        Precent = 100
                    };
                    ord1.SRule.Add(srule1);
                    var eord1 = this.fixture.CurrentDataContext.FinanceOrder.Add(ord1);
                    ordInOtherHomes.Add(eord1.Entity);
                    this.fixture.CurrentDataContext.SaveChanges();
                }
                else if (user == DataSetupUtility.UserD)
                {
                    secondhid = DataSetupUtility.Home5ID;
                    var cc1 = new FinanceControlCenter()
                    {
                        HomeID = secondhid,
                        Name = "Control Center 5.1",
                        Comment = "Comment 5.1",
                        Owner = user
                    };
                    var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                    ccInOtherHomes.Add(ec1.Entity);

                    var ord1 = new FinanceOrder()
                    {
                        HomeID = secondhid,
                        Name = "Order 5.1",
                        Comment = "Comment 5.1"
                    };
                    var srule1 = new FinanceOrderSRule()
                    {
                        Order = ord1,
                        RuleID = 1,
                        ControlCenterID = ec1.Entity.ID,
                        Precent = 100
                    };
                    ord1.SRule.Add(srule1);
                    var eord1 = this.fixture.CurrentDataContext.FinanceOrder.Add(ord1);
                    ordInOtherHomes.Add(eord1.Entity);
                    this.fixture.CurrentDataContext.SaveChanges();
                }
            }
            else if(hid == DataSetupUtility.Home2ID)
            {
                secondhid = DataSetupUtility.Home3ID;
                var cc1 = new FinanceControlCenter()
                {
                    HomeID = secondhid,
                    Name = "Control Center 3.1",
                    Comment = "Comment 3.1",
                    Owner = user
                };
                var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                ccInOtherHomes.Add(ec1.Entity);

                var ord1 = new FinanceOrder()
                {
                    HomeID = secondhid,
                    Name = "Order 3.1",
                    Comment = "Comment 3.1"
                };
                var srule1 = new FinanceOrderSRule()
                {
                    Order = ord1,
                    RuleID = 1,
                    ControlCenterID = ec1.Entity.ID,
                    Precent = 100
                };
                ord1.SRule.Add(srule1);
                var eord1 = this.fixture.CurrentDataContext.FinanceOrder.Add(ord1);
                ordInOtherHomes.Add(eord1.Entity);
                this.fixture.CurrentDataContext.SaveChanges();
            }

            // 1. Setup control centers
            var cccontrol = new FinanceControlCentersController(this.fixture.CurrentDataContext);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var context = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            cccontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };
            await cccontrol.Post(new FinanceControlCenter()
            {
                HomeID = hid,
                Name = "Control Center 1",
                Comment = "Comment 1",
                Owner = user
            });
            await cccontrol.Post(new FinanceControlCenter()
            {
                HomeID = hid,
                Name = "Control Center 2",
                Comment = "Comment 2",
                Owner = user
            });
            var listCCs = this.fixture.CurrentDataContext.FinanceControlCenter.Where(p => p.HomeID == hid).ToList<FinanceControlCenter>();

            // 2. Create order
            var control = new FinanceOrdersController(this.fixture.CurrentDataContext);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = context
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

            // 3. Read the order out (without Home ID)
            var queryUrl = "http://localhost/api/FinanceOrders";
            var req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceOrder>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceOrder>(odatacontext, req);
            var rst3 = control.Get(options);
            var expacntamt = ordInOtherHomes.Count() + 1;
            Assert.NotNull(rst3);
            Assert.Equal(expacntamt, rst3.Cast<FinanceOrder>().Count());

            // 3a. Read the order out (with Home ID)
            queryUrl = "http://localhost/api/FinanceOrders?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceOrder>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceOrder>(odatacontext, req);
            rst3 = control.Get(options);
            expacntamt = 1;
            Assert.NotNull(rst3);
            Assert.Equal(expacntamt, rst3.Cast<FinanceOrder>().Count());

            // 4. Change one order
            var norder = rst2.Entity;
            norder.Name = "New Order";
            rst = await control.Put(norder.ID, norder);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceOrder>>(rst);
            Assert.Equal(norder.Name, rst4.Entity.Name);

            // 5. Delete an order
            var rst5 = await control.Delete(oid);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);
            Assert.Equal(0, this.fixture.CurrentDataContext.FinanceOrderSRule.Where(p => p.OrderID == oid).Count());

            // 6. Read the order again
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Cast<FinanceOrder>().Count());

            // Last, delete all pre-created objects.
            this.fixture.CurrentDataContext.FinanceControlCenter.RemoveRange(listCCs);
            this.fixture.CurrentDataContext.FinanceOrder.RemoveRange(ordInOtherHomes);
            this.fixture.CurrentDataContext.FinanceControlCenter.RemoveRange(ccInOtherHomes);
            this.fixture.CurrentDataContext.SaveChanges();
        }
    }
}
