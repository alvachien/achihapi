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
    public class FinanceDocumentsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;

        public FinanceDocumentsControllerTest(SqliteDatabaseFixture fixture)
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserD)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserB)]
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

            // 0a. Prepare the context for other homes to test the filters
            var secondhid = hid;
            var secondcurrency = "";
            if (hid == DataSetupUtility.Home1ID)
            {
                if (user == DataSetupUtility.UserA || user == DataSetupUtility.UserB)
                {
                    secondhid = DataSetupUtility.Home3ID;
                    secondcurrency = DataSetupUtility.Home3BaseCurrency;
                }
                else if (user == DataSetupUtility.UserC)
                {
                    secondhid = DataSetupUtility.Home4ID;
                    secondcurrency = DataSetupUtility.Home4BaseCurrency;
                }
                else if (user == DataSetupUtility.UserD)
                {
                    secondhid = DataSetupUtility.Home5ID;
                    secondcurrency = DataSetupUtility.Home5BaseCurrency;
                }
            }
            else if (hid == DataSetupUtility.Home2ID)
            {
                secondhid = DataSetupUtility.Home3ID;
                secondcurrency = DataSetupUtility.Home3BaseCurrency;
            }
            if (secondhid != hid)
            {
                // Account
                accountObject = new FinanceAccount()
                {
                    HomeID = secondhid,
                    Name = "Account 3.1",
                    CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                    Owner = user
                };
                var ea1 = this.fixture.CurrentDataContext.FinanceAccount.Add(accountObject);
                accountsCreated.Add(ea1.Entity);
                // Control center
                controlCenterObject = new FinanceControlCenter()
                {
                    HomeID = secondhid,
                    Name = "Control Center 3.1",
                    Comment = "Comment 3.1",
                    Owner = user
                };
                var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(controlCenterObject);
                controlCentersCreated.Add(ec1.Entity);
                // Order
                orderObject = new FinanceOrder()
                {
                    HomeID = secondhid,
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
                var eord1 = this.fixture.CurrentDataContext.FinanceOrder.Add(orderObject);
                ordersCreated.Add(eord1.Entity);
                // Document
                documentObject = new FinanceDocument()
                {
                    HomeID = secondhid,
                    DocType = FinanceDocumentType.DocType_Normal,
                    TranCurr = secondcurrency,
                    Desp = "Test 1"
                };
                var item1 = new FinanceDocumentItem()
                {
                    DocumentHeader = documentObject,
                    ItemID = 1,
                    Desp = "Item 1.1",
                    TranType = 2, // Wage
                    TranAmount = 10,
                    AccountID = ea1.Entity.ID,
                    ControlCenterID = ec1.Entity.ID,
                };
                documentObject.Items.Add(item1);
                var edoc1 = this.fixture.CurrentDataContext.FinanceDocument.Add(documentObject);
                documentsCreated.Add(edoc1.Entity);
                this.fixture.CurrentDataContext.SaveChanges();
            }

            // 0b. Prepare the context for current home
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
                var ea1 = this.fixture.CurrentDataContext.FinanceAccount.Add(accountObject);
                accountsCreated.Add(ea1.Entity);
                // Control center
                controlCenterObject = new FinanceControlCenter()
                {
                    HomeID = hid,
                    Name = "Control Center 3.1",
                    Comment = "Comment 3.1",
                    Owner = user
                };
                var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(controlCenterObject);
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
                var eord1 = this.fixture.CurrentDataContext.FinanceOrder.Add(orderObject);
                ordersCreated.Add(eord1.Entity);
                this.fixture.CurrentDataContext.SaveChanges();
            }

            // 1. Create first docs.
            var control = new FinanceDocumentsController(this.fixture.CurrentDataContext);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var context = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var doc = new FinanceDocument()
            {
                HomeID = hid,
                DocType = FinanceDocumentType.DocType_Normal,
                TranCurr = currency,
                Desp = "Test 1"
            };
            var item = new FinanceDocumentItem()
            {
                DocumentHeader = doc,
                ItemID = 1,
                Desp = "Item 1.1",
                TranType = 2, // Wage
                TranAmount = 10,
                AccountID = accountsCreated.First(p => p.HomeID == hid).ID,
                ControlCenterID = controlCentersCreated.First(p => p.HomeID == hid).ID,
            };
            doc.Items.Add(item);
            var rst = await control.Post(doc);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst2.Entity.TranCurr, doc.TranCurr);
            Assert.Equal(rst2.Entity.Desp, doc.Desp);
            var firstdocid = rst2.Entity.ID;
            documentsCreated.Add(rst2.Entity);
            Assert.True(firstdocid > 0);

            // 2a. Now read the whole orders (without home id)
            var queryUrl = "http://localhost/api/FinanceDocuments";
            var req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocument>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceDocument>(odatacontext, req);
            var expectedamt = documentsCreated.Where(p => p.HomeID == hid).Count() + 1;
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(expectedamt, rst3.Cast<FinanceDocument>().Count());

            // 2b. Now read the whole orders (with home id)
            queryUrl = "http://localhost/api/FinanceDocuments?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            // odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocument>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceDocument>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Cast<FinanceDocument>().Count());

            // 3. Now create another one!
            doc = new FinanceDocument()
            {
                HomeID = hid,
                DocType = FinanceDocumentType.DocType_Normal,
                TranCurr = currency,
                Desp = "Test 2"
            };
            item = new FinanceDocumentItem()
            {
                DocumentHeader = doc,
                ItemID = 1,
                Desp = "Item 2.1",
                TranType = 2, // Wage
                TranAmount = 10,
                AccountID = accountsCreated.First(p => p.HomeID == hid).ID,
                OrderID = ordersCreated.FirstOrDefault(p => p.HomeID == hid)?.ID,
            };
            doc.Items.Add(item);
            rst = await control.Post(doc);
            Assert.NotNull(rst);
            rst2 = Assert.IsType<CreatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst2.Entity.TranCurr, doc.TranCurr);
            Assert.Equal(rst2.Entity.Desp, doc.Desp);
            // documentsCreated.Add(rst2.Entity);
            var seconddocid = rst2.Entity.ID;
            Assert.True(seconddocid > 0);

            // 4. Change one document
            doc.Desp = "Change Test";
            rst = await control.Put(seconddocid, doc);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst4.Entity.Desp, doc.Desp);

            // 5. Delete the second document
            var rst5 = await control.Delete(seconddocid);
            Assert.NotNull(rst5);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);
            Assert.Equal(0, this.fixture.CurrentDataContext.FinanceDocumentItem.Where(p => p.DocID == seconddocid).Count());

            // 6. Now read the whole documents
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Cast<FinanceDocument>().Count());

            // Last, clear all created objects
            this.fixture.CurrentDataContext.FinanceDocument.RemoveRange(documentsCreated);
            this.fixture.CurrentDataContext.FinanceAccount.RemoveRange(accountsCreated);
            this.fixture.CurrentDataContext.FinanceControlCenter.RemoveRange(controlCentersCreated);
            this.fixture.CurrentDataContext.FinanceOrder.RemoveRange(ordersCreated);
            this.fixture.CurrentDataContext.SaveChanges();

            Assert.Equal(0, this.fixture.CurrentDataContext.FinanceDocumentItem.Where(p => p.DocID == firstdocid).Count());
        }
    }
}
