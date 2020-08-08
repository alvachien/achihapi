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
        private List<int> documentsCreated = new List<int>();

        public FinanceDocumentsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceDocument>(provider, "FinanceDocuments");
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserD)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserB)]
        public async Task TestCase1(int hid, string currency, string user)
        {
            var context = this.fixture.GetCurrentDataContext();

            // 0a. Prepare the context for other homes to test the filters
            var secondhid = hid;
            if (hid == DataSetupUtility.Home1ID)
            {
                if (user == DataSetupUtility.UserA || user == DataSetupUtility.UserB)
                {
                    secondhid = DataSetupUtility.Home3ID;
                }
                else if (user == DataSetupUtility.UserC)
                {
                    secondhid = DataSetupUtility.Home4ID;
                }
                else if (user == DataSetupUtility.UserD)
                {
                    secondhid = DataSetupUtility.Home5ID;
                }
            }
            else if (hid == DataSetupUtility.Home2ID)
            {
                secondhid = DataSetupUtility.Home3ID;
            }

            if (hid == DataSetupUtility.Home1ID || secondhid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }
            else if (hid == DataSetupUtility.Home2ID || secondhid == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
            }
            else if (hid == DataSetupUtility.Home3ID || secondhid == DataSetupUtility.Home3ID)
            {
                fixture.InitHome3TestData(context);
            }
            else if (hid == DataSetupUtility.Home4ID || secondhid == DataSetupUtility.Home4ID)
            {
                fixture.InitHome4TestData(context);
            }
            else if (hid == DataSetupUtility.Home5ID || secondhid == DataSetupUtility.Home5ID)
            {
                fixture.InitHome5TestData(context);
            }
            var account = context.FinanceAccount.Where(p => p.HomeID == hid && p.Status != (Byte)FinanceAccountStatus.Closed).FirstOrDefault();
            var cc = context.FinanceControlCenter.Where(p => p.HomeID == hid).FirstOrDefault();
            var existamt = (from homemem in context.HomeMembers
                            join findoc in context.FinanceDocument
                            on new { homemem.HomeID, homemem.User } equals new { findoc.HomeID, User = user }
                            select findoc.ID).ToList().Count();
            var existamt_curhome = context.FinanceDocument.Where(p => p.HomeID == hid).Count();

            // 1. Create first docs.
            var control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
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
                AccountID = account.ID,
                ControlCenterID = cc.ID,
            };
            doc.Items.Add(item);
            var rst = await control.Post(doc);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(rst2.Entity.TranCurr, doc.TranCurr);
            Assert.Equal(rst2.Entity.Desp, doc.Desp);
            var firstdocid = rst2.Entity.ID;
            documentsCreated.Add(firstdocid);
            Assert.True(firstdocid > 0);

            // 2a. Now read the whole orders (without home id)
            var queryUrl = "http://localhost/api/FinanceDocuments";
            var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocument>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceDocument>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt + 1, rst3.Cast<FinanceDocument>().Count());

            // 2b. Now read the whole orders (with home id)
            queryUrl = "http://localhost/api/FinanceDocuments?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            // odatacontext = UnitTestUtility.GetODataQueryContext<FinanceDocument>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceDocument>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt_curhome + 1, rst3.Cast<FinanceDocument>().Count());

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
                AccountID = account.ID,
                ControlCenterID = cc.ID
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

            // 4a. Change one document - add new document item
            doc.Desp = "Change Test";
            doc.Items.Add(new FinanceDocumentItem
            {
                DocumentHeader = doc,
                ItemID = 2,
                Desp = "Item 2.2",
                TranType = 2, // Wage
                TranAmount = 20,
                AccountID = account.ID,
                ControlCenterID = cc.ID
            });
            rst = await control.Put(seconddocid, doc);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(doc.Desp, rst4.Entity.Desp);
            Assert.Equal(2, rst4.Entity.Items.Count);
            Assert.Equal("Item 2.1", rst4.Entity.Items.First(p => p.ItemID == 1).Desp);
            Assert.Equal("Item 2.2", rst4.Entity.Items.First(p => p.ItemID == 2).Desp);

            // 4b. Change one document - remove document item
            var itemidx = doc.Items.First(p => p.ItemID == 2);
            doc.Items.Remove(item);
            rst = await control.Put(seconddocid, doc);
            Assert.NotNull(rst);
            rst4 = Assert.IsType<UpdatedODataResult<FinanceDocument>>(rst);
            Assert.Equal(1, rst4.Entity.Items.Count);
            // Assert.Equal("Item 2.1", rst4.Entity.Items.First(p => p.ItemID == 1).Desp);

            // 5. Delete the second document
            var rst5 = await control.Delete(seconddocid);
            Assert.NotNull(rst5);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);
            Assert.Equal(0, context.FinanceDocumentItem.Where(p => p.DocID == seconddocid).Count());

            // 6. Now read the whole documents
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt_curhome + 1, rst3.Cast<FinanceDocument>().Count());

            // Last, clear all created objects
            CleanupCreatedEntries();
            await context.SaveChangesAsync();

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            if (documentsCreated.Count > 0)
            {
                var context = this.fixture.GetCurrentDataContext();
                foreach (var acntcrt in documentsCreated)
                    fixture.DeleteFinanceDocument(context, acntcrt);

                documentsCreated.Clear();
                context.SaveChanges();
            }
        }
    }
}
