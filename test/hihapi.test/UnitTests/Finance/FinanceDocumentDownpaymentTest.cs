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
using hihapi.Utilities;

namespace hihapi.test.UnitTests
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_AdvancePayment)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_AdvanceReceive)]
        public async Task TestCase1(int hid, string currency, string user, short doctype)
        {
            List<int> accountsCreated = new List<int>();
            List<int> controlCentersCreated = new List<int>();
            List<int> ordersCreated = new List<int>();
            List<int> documentsCreated = new List<int>();
            var context = this.fixture.GetCurrentDataContext();

            // 0. Prepare the context for current home
            if (hid > 0)
            {
                // Account
                var accountObject = new FinanceAccount()
                {
                    HomeID = hid,
                    Name = "Account 3.1",
                    CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                    Owner = user
                };
                var ea1 = context.FinanceAccount.Add(accountObject);
                // Control center
                var controlCenterObject = new FinanceControlCenter()
                {
                    HomeID = hid,
                    Name = "Control Center 3.1",
                    Comment = "Comment 3.1",
                    Owner = user
                };
                var ec1 = context.FinanceControlCenter.Add(controlCenterObject);
                // Order
                var orderObject = new FinanceOrder()
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
                await context.SaveChangesAsync();

                accountsCreated.Add(ea1.Entity.ID);
                controlCentersCreated.Add(ec1.Entity.ID);
                ordersCreated.Add(eord1.Entity.ID);
            }
            else
            {
                Assert.Equal(1, 2); // Quit!
            }

            // 1. Create first DP docs.
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
                AccountID = accountsCreated[0],
                ControlCenterID = controlCentersCreated[0],
            };
            dpcontext.DocumentInfo.Items.Add(item);
            dpcontext.AccountInfo = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account_8" + ".1",
                CategoryID = doctype == FinanceDocumentType.DocType_AdvancePayment
                        ? FinanceAccountCategoriesController.AccountCategory_AdvancePayment
                        : FinanceAccountCategoriesController.AccountCategory_AdvanceReceive,
                Owner = user
            };
            var startdate = new DateTime();
            var enddate = startdate.AddMonths(6);
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
            foreach(var rst in rsts)
            {
                var tmpdoc = new FinanceTmpDPDocument
                {
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
            var resp = await control.PostDPDocument(hid, dpcontext);
            var doc = Assert.IsType<CreatedODataResult<FinanceDocument>>(resp).Entity;
            documentsCreated.Add(doc.ID);
            Assert.True(doc.Items.Count == 2);

            // Now check in the databse
            foreach (var docitem in doc.Items)
            {
                if (docitem.AccountID != accountsCreated[0])
                {
                    accountsCreated.Add(docitem.AccountID);

                    var acnt = context.FinanceAccount.Find(docitem.AccountID);
                    Assert.NotNull(acnt);
                    if (doctype == FinanceDocumentType.DocType_AdvancePayment)
                        Assert.True(acnt.CategoryID == FinanceAccountCategoriesController.AccountCategory_AdvancePayment);
                    if (doctype == FinanceDocumentType.DocType_AdvanceReceive)
                        Assert.True(acnt.CategoryID == FinanceAccountCategoriesController.AccountCategory_AdvanceReceive);
                    var acntExtraDP = context.FinanceAccountExtraDP.Find(docitem.AccountID);
                    Assert.NotNull(acntExtraDP);
                    Assert.True(acntExtraDP.RefenceDocumentID == doc.ID);

                    var tmpdocs = context.FinanceTmpDPDocument.Where(p => p.AccountID == docitem.AccountID).OrderBy(p => p.TransactionDate).ToList();
                    Assert.True(rsts.Count == tmpdocs.Count);

                    foreach(var rst in rsts)
                    {
                        DateTime dat = rst.TranDate;
                        var tdoc = tmpdocs.Find(p => p.TransactionDate.Date == dat);
                        Assert.NotNull(tdoc);
                        Assert.True(rst.TranAmount == tdoc.TranAmount);
                        Assert.True(tdoc.AccountID == acntExtraDP.AccountID);
                    }
                }
            }

            // Last, clear all created objects
            foreach (var docid in documentsCreated)
                this.fixture.DeleteDocument(context, docid);
            foreach (var aid in accountsCreated)
                this.fixture.DeleteAccount(context, aid);
            foreach (var ccid in controlCentersCreated)
                this.fixture.DeleteControlCenter(context, ccid);
            foreach (var ordid in ordersCreated)
                this.fixture.DeleteOrder(context, ordid);
            await context.SaveChangesAsync();

            await context.DisposeAsync();
        }
    }
}
