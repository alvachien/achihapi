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

namespace hihapi.test.UnitTests.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentDownpaymentTest: IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;

        public FinanceDocumentDownpaymentTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceDocument>(provider, "FinanceDocuments");
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA)]
        public async Task TestCase1(int hid, string currency, string user)
        {
            List<FinanceAccount> accountsCreated = new List<FinanceAccount>();
            List<FinanceControlCenter> controlCentersCreated = new List<FinanceControlCenter>();
            List<FinanceOrder> ordersCreated = new List<FinanceOrder>();
            List<FinanceDocument> documentsCreated = new List<FinanceDocument>();
            FinanceAccount accountObject = null;
            FinanceControlCenter controlCenterObject = null;
            FinanceOrder orderObject = null;
            FinanceDocument documentObject = null;
            var context = this.fixture.GetCurrentDataContext();

            // 0. Prepare the context for current home
            if (hid > 0)
            {
                // Account
                accountObject = new FinanceAccount()
                {
                    HomeID = hid,
                    Name = "Account 3.1",
                    CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                    Owner = user
                };
                var ea1 = context.FinanceAccount.Add(accountObject);
                accountsCreated.Add(ea1.Entity);
                // Control center
                controlCenterObject = new FinanceControlCenter()
                {
                    HomeID = hid,
                    Name = "Control Center 3.1",
                    Comment = "Comment 3.1",
                    Owner = user
                };
                var ec1 = context.FinanceControlCenter.Add(controlCenterObject);
                controlCentersCreated.Add(ec1.Entity);
                // Order
                orderObject = new FinanceOrder()
                {
                    HomeID = hid,
                    Name = "Order 3.1",
                    Comment = "Comment 3.1"
                };
                var srule1 = new FinanceOrderSRule()
                {
                    Order = orderObject,
                    RuleID = 1,
                    ControlCenterID = ec1.Entity.ID,
                    Precent = 100
                };
                orderObject.SRule.Add(srule1);
                var eord1 = context.FinanceOrder.Add(orderObject);
                ordersCreated.Add(eord1.Entity);
                await context.SaveChangesAsync();
            }
            else
            {
                Assert.Equal(1, 2); // Quit!
            }

            // 1. Create first DP docs.
            var control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            var firstdocid = 0;
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // 1a. Prepare data
            var dpcontext = new FinanceADPDocumentCreateContext();
            dpcontext.DocumentInfo = new FinanceDocument()
            {
                HomeID = hid,
                DocType = FinanceDocumentType.DocType_Normal,
                TranCurr = currency,
                Desp = "Test 1"
            };
            var item = new FinanceDocumentItem()
            {
                DocumentHeader = dpcontext.DocumentInfo,
                ItemID = 1,
                Desp = "Item 1.1",
                TranType = 2, // Wage
                TranAmount = 10,
                AccountID = accountsCreated.First(p => p.HomeID == hid).ID,
                ControlCenterID = controlCentersCreated.First(p => p.HomeID == hid).ID,
            };
            dpcontext.DocumentInfo.Items.Add(item);
            dpcontext.AccountInfo = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account_8" + ".1",
                CategoryID = FinanceAccountCategoriesController.AccountCategory_AdvancePayment,
                Owner = user
            };
            var startdate = new DateTime();
            var enddate = startdate.AddMonths(6);
            dpcontext.AccountExtraInfo = new FinanceAccountExtraDP()
            {
                StartDate = startdate,
                EndDate = enddate,
                RepeatType = RepeatFrequency.Month,
                Comment = "Test",
            };

            // Last, clear all created objects
            context.FinanceDocument.RemoveRange(documentsCreated);
            context.FinanceAccount.RemoveRange(accountsCreated);
            context.FinanceControlCenter.RemoveRange(controlCentersCreated);
            context.FinanceOrder.RemoveRange(ordersCreated);
            await context.SaveChangesAsync();

            if (firstdocid > 0)
            {
                Assert.Equal(0, context.FinanceDocumentItem.Where(p => p.DocID == firstdocid).Count());
            }

            await context.DisposeAsync();
        }
    }
}
