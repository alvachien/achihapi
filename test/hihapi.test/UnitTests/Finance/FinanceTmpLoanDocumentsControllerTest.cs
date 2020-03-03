using hihapi.Controllers;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceTmpLoanDocumentsControllerTest : IDisposable
    {
        SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> accountsCreated = new List<Int32>();
        private List<Int32> documentsCreated = new List<Int32>();

        public FinanceTmpLoanDocumentsControllerTest(SqliteDatabaseFixture fixture)
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_BorrowFrom)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_LendTo)]
        public async Task TestCase1_InterestFree(int hid, string currency, string user, short doctype)
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
                Owner = user,
                Status = FinanceAccountStatus.Normal,
            };
            var startdate = new DateTime(2020, 1, 10);
            var enddate = new DateTime(2021, 1, 10);
            dpcontext.AccountInfo.ExtraLoan = new FinanceAccountExtraLoan()
            {
                StartDate = startdate,
                EndDate = enddate,
                TotalMonths = 12,
                RepaymentMethod = LoanRepaymentMethod.EqualPrincipal,
                InterestFree = true,
            };
            var rsts = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(new RepeatDatesWithAmountAndInterestCalInput
            {
                RepaymentMethod = dpcontext.AccountInfo.ExtraLoan.RepaymentMethod.Value,
                InterestFreeLoan = dpcontext.AccountInfo.ExtraLoan.InterestFree.Value,
                StartDate = dpcontext.AccountInfo.ExtraLoan.StartDate,
                TotalAmount = 1200,
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
            documentsCreated.Add(doc.ID);
            var loanacntid = -1;
            foreach (var docitem in doc.Items)
            {
                if (docitem.AccountID != account.ID)
                {
                    loanacntid = docitem.AccountID;
                    accountsCreated.Add(loanacntid);
                }
            }
            Assert.True(doc.Items.Count == 2);

            // 2. Switch to second controller
            var tmpcontrol = new FinanceTmpLoanDocumentsController(context);
            tmpcontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var tmpdocs = tmpcontrol.Get();
            Assert.NotEmpty(tmpdocs);
            Assert.Equal(rsts.Count, tmpdocs.Count());
            var dpdocs = tmpdocs.Cast<FinanceTmpLoanDocument>();

            // 3. Create repay document
            foreach (var dpdoc in dpdocs)
            {
                var contxt = new FinanceLoanRepayDocumentCreateContext();
                contxt.HomeID = hid;
                contxt.LoanTemplateDocumentID = dpdoc.DocumentID;
                contxt.DocumentInfo = new FinanceDocument
                {
                    DocType = FinanceDocumentType.DocType_Repay,
                    HomeID = hid,
                    Desp = dpdoc.Description,
                    TranCurr = currency,
                };
                contxt.DocumentInfo.Items.Add(new FinanceDocumentItem
                {
                    ItemID = 1,
                    AccountID = dpdoc.AccountID,
                    TranAmount = dpdoc.TransactionAmount,
                    ControlCenterID = dpdoc.ControlCenterID,
                    OrderID = dpdoc.OrderID,
                    TranType = doctype == FinanceDocumentType.DocType_BorrowFrom
                        ? FinanceTransactionType.TranType_RepaymentIn
                        : FinanceTransactionType.TranType_RepaymentOut,
                });
                contxt.DocumentInfo.Items.Add(new FinanceDocumentItem
                {
                    ItemID = 2,
                    AccountID = account.ID,
                    TranAmount = dpdoc.TransactionAmount,
                    ControlCenterID = dpdoc.ControlCenterID,
                    OrderID = dpdoc.OrderID,
                    TranType = doctype == FinanceDocumentType.DocType_BorrowFrom
                        ? FinanceTransactionType.TranType_RepaymentOut
                        : FinanceTransactionType.TranType_RepaymentIn,
                });
                var repaydocresp = await tmpcontrol.PostRepayDocument(contxt);
                var repaydoc = Assert.IsType<CreatedODataResult<FinanceDocument>>(repaydocresp);
                Assert.True(repaydoc.Entity.ID > 0);
                documentsCreated.Add(repaydoc.Entity.ID);

                // Check in the database
                var dpdocInDB = context.FinanceTmpLoanDocument.Where(p => p.DocumentID == dpdoc.DocumentID).SingleOrDefault();
                Assert.NotNull(dpdocInDB);
                Assert.NotNull(dpdocInDB.ReferenceDocumentID);
            }

            // 4. Now the account shall be closed automatically
            var account2 = context.FinanceAccount.Where(p => p.HomeID == hid && p.ID == loanacntid).FirstOrDefault();
            Assert.True(account2.Status == FinanceAccountStatus.Closed);

            // Last, clear all created objects
            CleanupCreatedEntries();

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_BorrowFrom)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, FinanceDocumentType.DocType_LendTo)]
        public async Task TestCase1_WithInterest(int hid, string currency, string user, short doctype)
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
                Name = "Account_8A" + ".1",
                CategoryID = doctype == FinanceDocumentType.DocType_BorrowFrom
                    ? FinanceAccountCategoriesController.AccountCategory_BorrowFrom
                    : FinanceAccountCategoriesController.AccountCategory_LendTo,
                Owner = user,
                Status = FinanceAccountStatus.Normal,
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
                AnnualRate = 0.05M,
            };
            var rsts = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(new RepeatDatesWithAmountAndInterestCalInput
            {
                RepaymentMethod = dpcontext.AccountInfo.ExtraLoan.RepaymentMethod.Value,
                InterestFreeLoan = dpcontext.AccountInfo.ExtraLoan.InterestFree.Value,
                InterestRate = dpcontext.AccountInfo.ExtraLoan.AnnualRate.Value,
                StartDate = dpcontext.AccountInfo.ExtraLoan.StartDate,
                TotalAmount = 1200,
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
            documentsCreated.Add(doc.ID);
            var loanacntid = -1;
            foreach (var docitem in doc.Items)
            {
                if (docitem.AccountID != account.ID)
                {
                    loanacntid = docitem.AccountID;
                    accountsCreated.Add(loanacntid);
                }
            }
            Assert.True(doc.Items.Count == 2);

            // 2. Switch to second controller
            var tmpcontrol = new FinanceTmpLoanDocumentsController(context);
            tmpcontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var tmpdocs = tmpcontrol.Get();
            Assert.NotEmpty(tmpdocs);
            Assert.Equal(rsts.Count, tmpdocs.Count());
            var dpdocs = tmpdocs.Cast<FinanceTmpLoanDocument>();

            // 3. Create repay document
            foreach (var dpdoc in dpdocs)
            {
                var contxt = new FinanceLoanRepayDocumentCreateContext();
                contxt.HomeID = hid;
                contxt.LoanTemplateDocumentID = dpdoc.DocumentID;
                contxt.DocumentInfo = new FinanceDocument
                {
                    DocType = FinanceDocumentType.DocType_Repay,
                    HomeID = hid,
                    Desp = dpdoc.Description,
                    TranCurr = currency,
                };
                contxt.DocumentInfo.Items.Add(new FinanceDocumentItem
                {
                    ItemID = 1,
                    AccountID = dpdoc.AccountID,
                    TranAmount = dpdoc.TransactionAmount,
                    ControlCenterID = dpdoc.ControlCenterID,
                    OrderID = dpdoc.OrderID,
                    TranType = doctype == FinanceDocumentType.DocType_BorrowFrom
                        ? FinanceTransactionType.TranType_RepaymentIn
                        : FinanceTransactionType.TranType_RepaymentOut,
                });
                contxt.DocumentInfo.Items.Add(new FinanceDocumentItem
                {
                    ItemID = 2,
                    AccountID = account.ID,
                    TranAmount = dpdoc.TransactionAmount,
                    ControlCenterID = dpdoc.ControlCenterID,
                    OrderID = dpdoc.OrderID,
                    TranType = doctype == FinanceDocumentType.DocType_BorrowFrom
                        ? FinanceTransactionType.TranType_RepaymentOut
                        : FinanceTransactionType.TranType_RepaymentIn,
                });
                var repaydocresp = await tmpcontrol.PostRepayDocument(contxt);
                var repaydoc = Assert.IsType<CreatedODataResult<FinanceDocument>>(repaydocresp);
                Assert.True(repaydoc.Entity.ID > 0);
                documentsCreated.Add(repaydoc.Entity.ID);

                // Check in the database
                var dpdocInDB = context.FinanceTmpLoanDocument.Where(p => p.DocumentID == dpdoc.DocumentID).SingleOrDefault();
                Assert.NotNull(dpdocInDB);
                Assert.NotNull(dpdocInDB.ReferenceDocumentID);
            }

            // 4. Now the account shall be closed automatically
            var account2 = context.FinanceAccount.Where(p => p.HomeID == hid && p.ID == loanacntid).FirstOrDefault();
            Assert.True(account2.Status == FinanceAccountStatus.Closed);

            // Last, clear all created objects
            CleanupCreatedEntries();

            await context.DisposeAsync();
        }

        private void CleanupCreatedEntries()
        {
            var context = this.fixture.GetCurrentDataContext();
            foreach (var doccrt in documentsCreated)
                fixture.DeleteDocument(context, doccrt);
            foreach (var acntcrt in accountsCreated)
                fixture.DeleteAccount(context, acntcrt);

            documentsCreated.Clear();
            accountsCreated.Clear();

            context.SaveChanges();
        }
    }
}
