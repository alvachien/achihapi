using System;
using Xunit;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentItemViewsControllerTest : IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;

        public FinanceDocumentItemViewsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceDocumentItemView>(provider, "FinanceDocumentItemViews");
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
        public async Task TestCase_Account(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }

            var control = new FinanceDocumentItemViewsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // Accounts
            var acntids = (from acnts in context.FinanceAccount
                           where acnts.HomeID == hid
                          select acnts.ID).ToList();

            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocumentItemView>(this.model);
            foreach (var accid in acntids)
            {
                // Perform the selection
                var queryUrl = "http://localhost/api/FinanceDocumentItemViews?$filter=HomeID eq " + hid.ToString() + " and AccountID eq " + accid.ToString();
                var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
                var options = UnitTestUtility.GetODataQueryOptions<FinanceDocumentItemView>(odatacontext, req);
                var rst = control.Get(options);
                var expacntamt = (from docitem in context.FinanceDocumentItem
                                  join docheader in context.FinanceDocument
                                  on new { docitem.DocID, HomeID = hid } equals new { DocID = docheader.ID, docheader.HomeID }
                                  where docitem.AccountID == accid
                                  select docitem.ItemID
                                  ).ToList().Count();
                if (expacntamt > 0)
                {
                    Assert.NotNull(rst);
                    Assert.Equal(expacntamt, rst.Cast<FinanceDocumentItemView>().Count());
                }
                else
                {
                    Assert.Empty(rst);
                }
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD)]
        public async Task TestCase_ControlCenter(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }

            var control = new FinanceDocumentItemViewsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // Accounts
            var ccids = (from cc in context.FinanceControlCenter
                         where cc.HomeID == hid
                           select cc.ID).ToList();

            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocumentItemView>(this.model);
            foreach (var cid in ccids)
            {
                // Perform the selection
                var queryUrl = "http://localhost/api/FinanceDocumentItemViews?$filter=HomeID eq " + hid.ToString() + " and ControlCenterID eq " + cid.ToString();
                var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
                var options = UnitTestUtility.GetODataQueryOptions<FinanceDocumentItemView>(odatacontext, req);
                var rst = control.Get(options);
                var expacntamt = (from docitem in context.FinanceDocumentItem
                                  join docheader in context.FinanceDocument
                                  on new { docitem.DocID, HomeID = hid } equals new { DocID = docheader.ID, docheader.HomeID }
                                  where docitem.ControlCenterID == cid
                                  select docitem.ItemID
                                  ).ToList().Count();
                if (expacntamt > 0)
                {
                    Assert.NotNull(rst);
                    Assert.Equal(expacntamt, rst.Cast<FinanceDocumentItemView>().Count());
                }
                else
                {
                    Assert.Empty(rst);
                }
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD)]
        public async Task TestCase_Order(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }

            var control = new FinanceDocumentItemViewsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // Accounts
            var ordids = (from ord in context.FinanceOrder
                         where ord.HomeID == hid
                         select ord.ID).ToList();

            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocumentItemView>(this.model);
            foreach (var oid in ordids)
            {
                // Perform the selection
                var queryUrl = "http://localhost/api/FinanceDocumentItemViews?$filter=HomeID eq " + hid.ToString() + " and OrderID eq " + oid.ToString();
                var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
                var options = UnitTestUtility.GetODataQueryOptions<FinanceDocumentItemView>(odatacontext, req);
                var rst = control.Get(options);
                var expacntamt = (from docitem in context.FinanceDocumentItem
                                  join docheader in context.FinanceDocument
                                  on new { docitem.DocID, HomeID = hid } equals new { DocID = docheader.ID, docheader.HomeID }
                                  where docitem.OrderID == oid
                                  select docitem.ItemID
                                  ).ToList().Count();
                if (expacntamt > 0)
                {
                    Assert.NotNull(rst);
                    Assert.Equal(expacntamt, rst.Cast<FinanceDocumentItemView>().Count());
                }
                else
                {
                    Assert.Empty(rst);
                }
            }

            await context.DisposeAsync();
        }
    }
}
