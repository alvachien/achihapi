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
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using hihapi.Utilities;
using System.Collections.Generic;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceTmpDPDocumentsControllerTest : IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> accountsCreated = new List<Int32>();
        private List<Int32> documentsCreated = new List<Int32>();

        public FinanceTmpDPDocumentsControllerTest(SqliteDatabaseFixture fixture)
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_AdvancePayment)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_AdvanceReceive)]
        public async Task TestCase1(int hid, string currency, string user, short doctype)
        {
            var context = this.fixture.GetCurrentDataContext();

            // 0. Prepare the context for current home
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }
            else if (hid == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
            }
            else if (hid == DataSetupUtility.Home3ID)
            {
                fixture.InitHome3TestData(context);
            }
            else if (hid == DataSetupUtility.Home4ID)
            {
                fixture.InitHome4TestData(context);
            }
            else if (hid == DataSetupUtility.Home5ID)
            {
                fixture.InitHome5TestData(context);
            }
            var account = context.FinanceAccount.Where(p => p.HomeID == hid && p.Status != FinanceAccountStatus.Closed).FirstOrDefault();
            var cc = context.FinanceControlCenter.Where(p => p.HomeID == hid).FirstOrDefault();

            // 1. Create DP docs.
            var control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // 1a. Prepare data
            var dpcontext = new FinanceADPDocumentCreateContext();
            dpcontext.DocumentInfo = new FinanceDocument()
            {
                HomeID = hid,
                DocType = doctype,
                TranCurr = currency,
                Desp = "Test 1"
            };
            var item = new FinanceDocumentItem()
            {
                DocumentHeader = dpcontext.DocumentInfo,
                ItemID = 1,
                Desp = "Item 1.1",
                TranType = doctype == FinanceDocumentType.DocType_AdvancePayment
                    ? FinanceTransactionType.TranType_AdvancePaymentOut
                    : FinanceTransactionType.TranType_AdvanceReceiveIn,
                TranAmount = 1200,
                AccountID = account.ID,
                ControlCenterID = cc.ID,
            };
            dpcontext.DocumentInfo.Items.Add(item);
            dpcontext.AccountInfo = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account_8" + ".1",
                CategoryID = doctype == FinanceDocumentType.DocType_AdvancePayment
                        ? FinanceAccountCategoriesController.AccountCategory_AdvancePayment
                        : FinanceAccountCategoriesController.AccountCategory_AdvanceReceive,
                Owner = user,
                Status = FinanceAccountStatus.Normal,
            };
            var startdate = DateTime.Today;
            var enddate = DateTime.Today.AddMonths(6);
            dpcontext.AccountInfo.ExtraDP = new FinanceAccountExtraDP()
            {
                StartDate = startdate,
                EndDate = enddate,
                RepeatType = RepeatFrequency.Month,
                Comment = "Test",
            };
            var rsts = CommonUtility.WorkoutRepeatedDatesWithAmount(new RepeatDatesWithAmountCalculationInput
            {
                StartDate = startdate,
                EndDate = enddate,
                TotalAmount = item.TranAmount,
                RepeatType = RepeatFrequency.Month,
                Desp = item.Desp,
            });
            var tmpdocid = 1;
            foreach (var rst in rsts)
            {
                var tmpdoc = new FinanceTmpDPDocument
                {
                    DocumentID = tmpdocid++,
                    TranAmount = rst.TranAmount,
                    TransactionDate = rst.TranDate,
                    HomeID = hid,
                    TransactionType = 5,
                    ControlCenterID = item.ControlCenterID,
                    OrderID = item.OrderID,
                    Description = item.Desp
                };

                dpcontext.AccountInfo.ExtraDP.DPTmpDocs.Add(tmpdoc);
            }
            var resp = await control.PostDPDocument(dpcontext);
            var doc = Assert.IsType<CreatedODataResult<FinanceDocument>>(resp).Entity;
            documentsCreated.Add(doc.ID);
            Assert.True(doc.Items.Count == 2);
            var dpacntid = -1;
            foreach(var did in doc.Items)
            {
                if (did.AccountID != account.ID)
                {
                    dpacntid = did.AccountID;
                    accountsCreated.Add(dpacntid);
                }
            }

            // 2. Switch to second controller
            var tmpcontrol = new FinanceTmpDPDocumentsController(context);
            tmpcontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var tmpdocs = tmpcontrol.Get();
            Assert.NotEmpty(tmpdocs);
            Assert.Equal(rsts.Count, tmpdocs.Count());
            var dpdocs = tmpdocs.Cast<FinanceTmpDPDocument>();

            // 3. Create repay document
            foreach(var dpdoc in dpdocs)
            {
                var repaydocresp = await tmpcontrol.PostDocument(dpdoc.DocumentID, dpdoc.AccountID, dpdoc.HomeID);
                var repaydoc = Assert.IsType<CreatedODataResult<FinanceDocument>>(repaydocresp);
                Assert.True(repaydoc.Entity.ID > 0);
                documentsCreated.Add(repaydoc.Entity.ID);

                // Check in the database
                var dpdocInDB = context.FinanceTmpDPDocument.Where(p => p.DocumentID == dpdoc.DocumentID).SingleOrDefault();
                Assert.NotNull(dpdocInDB);
                Assert.NotNull(dpdocInDB.ReferenceDocumentID);
            }

            // Check the account status
            var account2 = context.FinanceAccount.Where(p => p.HomeID == hid && p.ID == dpacntid).FirstOrDefault();
            Assert.True(account2.Status == FinanceAccountStatus.Closed);

            CleanupCreatedEntries();

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            var context = this.fixture.GetCurrentDataContext();
            foreach (var doccrt in documentsCreated)
                fixture.DeleteFinanceDocument(context, doccrt);
            foreach (var acntcrt in accountsCreated)
                fixture.DeleteFinanceAccount(context, acntcrt);

            documentsCreated.Clear();
            accountsCreated.Clear();

            context.SaveChanges();
        }
    }
}
