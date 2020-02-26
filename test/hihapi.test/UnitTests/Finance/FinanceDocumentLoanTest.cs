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
    public class FinanceDocumentLoanTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;

        public FinanceDocumentLoanTest(SqliteDatabaseFixture fixture)
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_BorrowFrom)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_LendTo)]
        public async Task TestCase1(int hid, string currency, string user, short doctype)
        {
            var accountCreated = 0;
            var documentCreated = 0;
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

            // 1. Create first Loan docs.
            var control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // 1a. Prepare data
            var dpcontext = new FinanceLoanDocumentCreateContext();
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
                TranType = doctype == FinanceDocumentType.DocType_BorrowFrom 
                        ? FinanceTransactionType.TranType_BorrowFrom 
                        : FinanceTransactionType.TranType_LendTo,
                TranAmount = 1200,
                AccountID = account.ID,
                ControlCenterID = cc.ID,
            };
            dpcontext.DocumentInfo.Items.Add(item);
            dpcontext.AccountInfo = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account_8" + ".1",
                CategoryID = doctype == FinanceDocumentType.DocType_BorrowFrom 
                    ? FinanceAccountCategoriesController.AccountCategory_BorrowFrom
                    : FinanceAccountCategoriesController.AccountCategory_LendTo,
                Owner = user
            };
            var startdate = new DateTime(2020, 1, 10);
            var enddate = new DateTime(2021, 1, 10);
            dpcontext.AccountInfo.ExtraLoan = new FinanceAccountExtraLoan()
            {
                StartDate = startdate,
                EndDate = enddate,
                TotalMonths = 12,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
                InterestFree = false,
            };
            var rsts = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(new RepeatDatesWithAmountAndInterestCalInput
            {
                RepaymentMethod = dpcontext.AccountInfo.ExtraLoan.RepaymentMethod.Value,
                InterestFreeLoan = dpcontext.AccountInfo.ExtraLoan.InterestFree.Value,
                StartDate = dpcontext.AccountInfo.ExtraLoan.StartDate,
                TotalAmount = 12000,
                EndDate = dpcontext.AccountInfo.ExtraLoan.EndDate,
                TotalMonths = dpcontext.AccountInfo.ExtraLoan.TotalMonths.Value,
                FirstRepayDate = new DateTime(2020, 2, 15)
            });
            var tmpdocid = 1;
            foreach (var rst in rsts)
            {
                var tmpdoc = new FinanceTmpLoanDocument
                {
                    DocumentID = tmpdocid++,
                    TransactionAmount = rst.TranAmount,
                    InterestAmount = rst.InterestAmount,
                    TransactionDate = rst.TranDate,
                    HomeID = hid,
                    ControlCenterID = item.ControlCenterID,
                    OrderID = item.OrderID,
                    Description = item.Desp,                    
                };

                dpcontext.AccountInfo.ExtraLoan.LoanTmpDocs.Add(tmpdoc);
            }
            var resp = await control.PostLoanDocument(dpcontext);
            var doc = Assert.IsType<CreatedODataResult<FinanceDocument>>(resp).Entity;
            documentCreated = doc.ID;
            Assert.True(doc.Items.Count == 2);

            // Now check in the databse
            foreach (var docitem in doc.Items)
            {
                if (docitem.AccountID != account.ID)
                {
                    accountCreated = docitem.AccountID;

                    var acnt = context.FinanceAccount.Find(docitem.AccountID);
                    Assert.NotNull(acnt);
                    if (doctype == FinanceDocumentType.DocType_BorrowFrom)
                        Assert.True(acnt.CategoryID == FinanceAccountCategoriesController.AccountCategory_BorrowFrom);
                    else if (doctype == FinanceDocumentType.DocType_LendTo)
                        Assert.True(acnt.CategoryID == FinanceAccountCategoriesController.AccountCategory_LendTo);
                    var acntExtraLoan = context.FinanceAccountExtraLoan.Find(docitem.AccountID);
                    Assert.NotNull(acntExtraLoan);
                    Assert.True(acntExtraLoan.RefDocID == doc.ID);

                    var tmpdocs = context.FinanceTmpLoanDocument.Where(p => p.AccountID == docitem.AccountID).OrderBy(p => p.TransactionDate).ToList();
                    Assert.True(rsts.Count == tmpdocs.Count);

                    foreach (var rst in rsts)
                    {
                        DateTime dat = rst.TranDate;
                        var tdoc = tmpdocs.Find(p => p.TransactionDate.Date == dat);
                        Assert.NotNull(tdoc);
                        Assert.True(tdoc.AccountID == acntExtraLoan.AccountID);
                    }
                }
            }

            // Last, clear all created objects
            if (documentCreated > 0)
                this.fixture.DeleteDocument(context, documentCreated);
            if (accountCreated > 0)
                this.fixture.DeleteAccount(context, accountCreated);
            await context.SaveChangesAsync();

            await context.DisposeAsync();
        }
    }
}
