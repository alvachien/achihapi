using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using hihapi.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.EntityFrameworkCore;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{

    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        List<int> listCreatedAccount = new List<int>();
        List<int> listCreatedCC = new List<int>();
        List<int> listCreatedDocs = new List<int>();

        public static TheoryData<FinanceDocumentsControllerTestData_NormalDoc> NormalDocs =>
            new TheoryData<FinanceDocumentsControllerTestData_NormalDoc>
            {
                // Home 1, Cash account 1
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    DataSetupUtility.Home1BaseCurrency, null, null, null, null, null, "H1_NORMAL_DOC_1",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1, 
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 100, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        }
                    }),
                // Home 1, Cash account 1a - foreign currency
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    "EUR", null, 700, null, null, null, "H1_NORMAL_DOC_1a",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 100, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        }
                    }),
                // Home 1, Cash account 1, multiple lines
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    DataSetupUtility.Home1BaseCurrency, null, null, null, null, null, "H1_NORMAL_DOC_1b",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 100, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        },
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 2,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter4ID, Amount = 200, OrderID = null,
                            TranType = DataSetupUtility.TranType_Expense1
                        }
                    }),
                // Home 1, Cash account 2
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    DataSetupUtility.Home1BaseCurrency, null, null, null, null, null, "H1_NORMAL_DOC_2",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount2ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 200, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        }
                    }),
                // Home 1, Cash account 2a
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    DataSetupUtility.Home1BaseCurrency, null, null, null, null, null, "H1_NORMAL_DOC_2a",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 100, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        },
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 2,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter4ID, Amount = 200, OrderID = null,
                            TranType = DataSetupUtility.TranType_Expense1
                        },
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 3,
                            AccountID = DataSetupUtility.Home1CashAccount2ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 300, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income2
                        },
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 4,
                            AccountID = DataSetupUtility.Home1CashAccount2ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter2ID, Amount = 400, OrderID = null,
                            TranType = DataSetupUtility.TranType_Expense2
                        }
                    }),
                // Home 1, Despoit account 5
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    DataSetupUtility.Home1BaseCurrency, null, null, null, null, null, "H1_NORMAL_DOC_3",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home1DepositAccount5ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 200, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        }
                    }),
                // Home 1, Virtual account 17
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserA, DataSetupUtility.Home1ID,
                    DataSetupUtility.Home1BaseCurrency, null, null, null, null, null, "H1_NORMAL_DOC_4",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home1VirtualAccount17ID,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter3ID, Amount = 100, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        }
                    }),
                // Home 2, Cash Account 1
                new FinanceDocumentsControllerTestData_NormalDoc(
                    DataSetupUtility.UserB, DataSetupUtility.Home2ID,
                    DataSetupUtility.Home2BaseCurrency, null, null, null, null, null, "H2_NORMAL_DOC_1",
                    new List<FinanceDocumentsControllerTestData_DocItem> {
                        new FinanceDocumentsControllerTestData_DocItem { ItemID = 1,
                            AccountID = DataSetupUtility.Home2CashAccount1ID,
                            ControlCenterID = DataSetupUtility.Home2CashAccount1ID, Amount = 200, OrderID = null,
                            TranType = DataSetupUtility.TranType_Income1
                        }
                    }),
            };

        public FinanceDocumentsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
            if (this.listCreatedAccount.Count > 0)
            {
                this.listCreatedAccount.ForEach(x => this.fixture.DeleteFinanceAccount(this.fixture.GetCurrentDataContext(), x));

                this.listCreatedAccount.Clear();
            }
            if (this.listCreatedCC.Count > 0)
            {
                this.listCreatedCC.ForEach(x => this.fixture.DeleteFinanceControlCenter(this.fixture.GetCurrentDataContext(), x));

                this.listCreatedCC.Clear();
            }    
            if (this.listCreatedDocs.Count > 0)
            {
                this.listCreatedDocs.ForEach(x => this.fixture.DeleteFinanceDocument(this.fixture.GetCurrentDataContext(), x));

                this.listCreatedDocs.Clear();
            }
            this.fixture.GetCurrentDataContext().SaveChanges();
        }

        [Theory]
        [MemberData(nameof(NormalDocs))]
        public async Task TestCase_CreateNormalDoc(FinanceDocumentsControllerTestData_NormalDoc normalDoc)
        {
            var context = this.fixture.GetCurrentDataContext();
            int ndocid = -1;
            //int itemcnt = normalDoc.DocItems.Count;
            //int itemmaxid = normalDoc.DocItems.Max(p => p.ItemID);
            //int itemminid = normalDoc.DocItems.Min(p => p.ItemID);
            // Pre. setup
            this.fixture.InitHomeTestData(normalDoc.HomeID, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(normalDoc.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 2. Create normal docs
            FinanceDocument doc = new FinanceDocument();
            doc.HomeID = normalDoc.HomeID;
            doc.DocType = FinanceDocumentType.DocType_Normal;
            doc.Desp = normalDoc.Desp;
            doc.TranCurr = normalDoc.Currency;
            doc.ExgRate = normalDoc.ExchangeRate;
            doc.ExgRate2 = normalDoc.SecondExchangeRate;
            FinanceDocumentItem item = null;
            normalDoc.DocItems.ForEach(di =>
            {
                item = new FinanceDocumentItem();
                item.ItemID = di.ItemID;
                item.AccountID = di.AccountID;
                item.TranAmount = di.Amount;
                item.TranType = di.TranType;
                item.Desp = di.Desp;
                item.ControlCenterID = di.ControlCenterID;
                item.OrderID = di.OrderID;
                item.UseCurr2 = di.UseCurr2;
                doc.Items.Add(item);
            });
            var postresult = await control.Post(doc);
            Assert.NotNull(postresult);
            var createDocResult = Assert.IsType<CreatedODataResult<FinanceDocument>>(postresult);
            // Verify the attributes
            ValidateDocument(doc, createDocResult.Entity, true);
            // Store the Doc ID for deletion
            ndocid = createDocResult.Entity.ID;
            listCreatedDocs.Add(ndocid);
            doc.ID = ndocid;

            // 3. Read it out.
            var getresult = control.Get(ndocid);
            Assert.NotNull(getresult);
            // Verify the attributes - GET won't return the items
            ValidateDocument(doc, getresult, false, true);
            // 3a. Read document item
            var docitemcontrol = new FinanceDocumentItemsController(context);
            try
            {
                docitemcontrol.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            docitemcontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };
            var docitemrst = docitemcontrol.Get();
            Assert.NotNull(docitemrst);
            // 3b. Read document item view
            var docitemviewcontrol = new FinanceDocumentItemViewsController(context);
            try
            {
                docitemviewcontrol.Get();
            }
            catch(Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            docitemviewcontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };
            var docitemviewrst = docitemviewcontrol.Get();
            Assert.NotNull(docitemviewrst);

            // 4. Do the changes to normal docs.
            var changableResult = control.IsChangable(ndocid);
            Assert.NotNull(changableResult);

            // 4.1 Change the desp
            doc.Desp += "Changed";
            var changeResult = await control.Put(ndocid, doc);
            var changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            var changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            // Validate the return value
            ValidateDocument(doc, changedDoc, false);
            // Validate the DB
            ValidateDocumentInDB(doc, context);

            // 4.2 Add the item
            int itemmaxid = normalDoc.DocItems.Max(p => p.ItemID);
            itemmaxid++;
            doc.Items.Add(new FinanceDocumentItem
            {
                ItemID = itemmaxid,
                AccountID = normalDoc.DocItems[0].AccountID,
                TranAmount = 2 * normalDoc.DocItems[0].Amount,
                TranType = normalDoc.DocItems[0].TranType,
                Desp = normalDoc.DocItems[0].Desp,
                ControlCenterID = normalDoc.DocItems[0].ControlCenterID,
                OrderID = normalDoc.DocItems[0].OrderID,
                UseCurr2 = normalDoc.DocItems[0].UseCurr2
            });

            changeResult = await control.Put(ndocid, doc);
            // 4.2.1 Verify the returned data
            changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            ValidateDocument(doc, changedDoc, false, false);
            // 4.2.2 Verify the data in DB.
            ValidateDocumentInDB(doc, context);

            // 4.3 Update the item with min. id
            int itemminid = doc.Items.Min(p => p.ItemID);
            foreach(var docitem in doc.Items)
            {
                if (docitem.ItemID == itemminid)
                {
                    docitem.TranAmount += 100;
                    if (docitem.AccountID != DataSetupUtility.Home1CashAccount3ID)
                        docitem.AccountID = DataSetupUtility.Home1CashAccount3ID;
                    else
                        docitem.AccountID = DataSetupUtility.Home1CashAccount2ID;
                }
            }
            changeResult = await control.Put(ndocid, doc);
            changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            // 4.3.1 Verify the returned data
            changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            ValidateDocument(doc, changedDoc, false);
            // 4.3.2 Verify the data in DB.
            ValidateDocumentInDB(doc, context);

            // 4.4 Delete an item
            // Delete the item with min. DI
            var tbditem = doc.Items.FirstOrDefault(p => p.ItemID == itemminid);
            if (tbditem != null)
            {
                doc.Items.Remove(tbditem);

                changeResult = await control.Put(ndocid, doc);
                changeokresult = Assert.IsType<OkObjectResult>(changeResult);
                changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);

                // 4.4.1 Verify the returned data
                ValidateDocument(doc, changedDoc, false);
                // 4.4.2 Verify the data in DB.
                ValidateDocumentInDB(doc, context);
            }

            // 5. Delete doc.
            var deleteResult = await control.Delete(ndocid);
            Assert.NotNull(deleteResult);
            var deletStatusCodeResult = Assert.IsType<StatusCodeResult>(deleteResult);
            Assert.NotNull(deletStatusCodeResult);
            Assert.Equal(204, deletStatusCodeResult.StatusCode);
            this.listCreatedDocs.Remove(ndocid);

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, FinanceDocumentType.DocType_Normal, 
            DataSetupUtility.Home1CashAccount1ID, DataSetupUtility.TranType_Expense1,
            DataSetupUtility.Home1ControlCenter1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, FinanceDocumentType.DocType_Normal,
            DataSetupUtility.Home1CashAccount2ID, DataSetupUtility.TranType_Expense1,
            DataSetupUtility.Home1ControlCenter1ID)]
        [InlineData(DataSetupUtility.UserC, DataSetupUtility.Home1ID, FinanceDocumentType.DocType_Normal,
            DataSetupUtility.Home1CashAccount2ID, DataSetupUtility.TranType_Expense1,
            DataSetupUtility.Home1ControlCenter3ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID, FinanceDocumentType.DocType_Normal,
            DataSetupUtility.Home2CashAccount1ID, DataSetupUtility.TranType_Expense2,
            DataSetupUtility.Home2ControlCenter1ID)]
        public async Task TestCase_PatchDocument(string user, int hid, Int16 doctype, int accountid, int trantypeid, int ccid)
        {
            var context = this.fixture.GetCurrentDataContext();
            int ndocid = -1;
            this.fixture.InitHomeTestData(hid, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 2. Create normal docs
            FinanceDocument doc = new FinanceDocument();
            doc.HomeID = hid;
            doc.DocType = doctype;
            doc.Desp = $"{ hid }_{accountid}_{ccid}";
            doc.TranCurr = "CNY";
            FinanceDocumentItem item = null;
            item = new FinanceDocumentItem();
            item.ItemID = 1;
            item.AccountID = accountid;
            item.TranAmount = 100;
            item.TranType = trantypeid;
            item.Desp = doc.Desp;
            item.ControlCenterID = ccid;
            doc.Items.Add(item);
            var postresult = await control.Post(doc);
            Assert.NotNull(postresult);
            var createDocResult = Assert.IsType<CreatedODataResult<FinanceDocument>>(postresult);
            ndocid = createDocResult.Entity.ID;
            doc.ID = ndocid;
            this.listCreatedDocs.Add(ndocid);

            if (doctype == FinanceDocumentType.DocType_Normal)
            {
                // 3. Now patch it - normal case
                Delta<FinanceDocument> delta = new Delta<FinanceDocument>();
                delta.UpdatableProperties.Clear();
                delta.UpdatableProperties.Add("Desp");
                delta.TrySetPropertyValue("Desp", "New Desp");
                var patchresult = await control.Patch(ndocid, delta);
                Assert.NotNull(patchresult);
                var patcheddoc = Assert.IsType<UpdatedODataResult<FinanceDocument>>(patchresult);
                Assert.NotNull(patcheddoc);
                Assert.Equal("New Desp", patcheddoc.Entity.Desp);

                // 4. Now patch it - exception case
                delta = new Delta<FinanceDocument>();
                delta.UpdatableProperties.Clear();
                delta.UpdatableProperties.Add("HomeID");
                delta.TrySetPropertyValue("HomeID", 9999);
                patchresult = await control.Patch(ndocid, delta);
                Assert.NotNull(patchresult);
                Assert.IsType<BadRequestObjectResult>(patchresult);
            }
            else
            {
                // 5. Exception case.
            }

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceDocumentsControllerTestData_DPDoc> AdvancePaymentDocs =>
            new TheoryData<FinanceDocumentsControllerTestData_DPDoc>
            {
                new FinanceDocumentsControllerTestData_DPDoc()
                {
                    CurrentUser = DataSetupUtility.UserA,
                    HomeID = DataSetupUtility.Home1ID,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 1200,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    StartDate = new DateTime(2022, 2, 1),
                    EndDate = new DateTime(2023, 2, 1),
                    Frequency = RepeatFrequency.Month,
                    Comment = "Test Adv. payment 1",
                    DPControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    DPTranType = DataSetupUtility.TranType_Expense1,
                },
                new FinanceDocumentsControllerTestData_DPDoc()
                {
                    CurrentUser = DataSetupUtility.UserB,
                    HomeID = DataSetupUtility.Home2ID,
                    Currency = DataSetupUtility.Home2BaseCurrency,
                    TranDate = new DateTime(2021, 12, 30),
                    Amount = 960,
                    AccountID = DataSetupUtility.Home2CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home2ControlCenter1ID,
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2023, 1, 1),
                    Frequency = RepeatFrequency.Month,
                    Comment = "Test Adv. payment 2",
                    DPControlCenterID = DataSetupUtility.Home2ControlCenter1ID,
                    DPTranType = DataSetupUtility.TranType_Expense1,
                },
            };

        [Theory]
        [MemberData(nameof(AdvancePaymentDocs))]
        public async Task TestCase_AdvancePayment(FinanceDocumentsControllerTestData_DPDoc testdata)
        {
            var context = fixture.GetCurrentDataContext();

            this.fixture.InitHomeTestData(testdata.HomeID, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 2. Normal case
            FinanceADPDocumentCreateContext createContext = new FinanceADPDocumentCreateContext();
            var acntDP = new FinanceAccountExtraDP()
            {
                StartDate = testdata.StartDate,
                EndDate = testdata.EndDate,
                RepeatType = testdata.Frequency,
                Comment = testdata.Comment,
            };
            var dpdates = CommonUtility.WorkoutRepeatedDatesWithAmount(new RepeatDatesWithAmountCalculationInput()
            {
                StartDate = acntDP.StartDate,
                EndDate = acntDP.EndDate,
                RepeatType = acntDP.RepeatType,
                TotalAmount = testdata.Amount,
                Desp = testdata.Comment,
            });
            int tdocid = 1;
            dpdates.ForEach(d =>
            {
                acntDP.DPTmpDocs.Add(new FinanceTmpDPDocument()
                {
                    DocumentID = tdocid++,
                    HomeID = testdata.HomeID,
                    TranAmount = d.TranAmount,
                    TransactionDate = d.TranDate,
                    ControlCenterID = testdata.DPControlCenterID,
                    OrderID = testdata.DPOrderID,
                    Description = d.Desp,
                });
            });
            createContext.AccountInfo = new FinanceAccount()
            {
                HomeID = testdata.HomeID,
                Name = testdata.Comment,
                CategoryID = FinanceAccountCategory.AccountCategory_AdvancePayment,
                Status = (byte)FinanceAccountStatus.Normal,
                Owner = testdata.CurrentUser,
                ExtraDP = acntDP,
            };
            createContext.DocumentInfo = new FinanceDocument()
            {
                HomeID = testdata.HomeID,
                TranCurr = testdata.Currency,
                TranDate = testdata.TranDate,
                Desp = testdata.Comment,
                DocType = FinanceDocumentType.DocType_AdvancePayment,
                Items = new List<FinanceDocumentItem> 
                { 
                    new FinanceDocumentItem()
                    {
                        ItemID = 1,
                        AccountID = testdata.AccountID,
                        ControlCenterID = testdata.ControlCenterID,
                        OrderID = testdata.OrderID,
                        TranType = FinanceTransactionType.TranType_AdvancePaymentOut,
                        TranAmount = testdata.Amount,
                        Desp = testdata.Comment,
                    }
                },
            };
            var postresult = await control.PostDPDocument(createContext);
            var postokresult = Assert.IsType<OkObjectResult>(postresult);
            var createdoc = Assert.IsAssignableFrom<FinanceDocument>(postokresult.Value as FinanceDocument);

            listCreatedDocs.Add(createdoc.ID);
            ValidateDocument(createContext.DocumentInfo, createdoc, true, true);
            var createdAcntID = -1;
            foreach(var item in createdoc.Items)
            {
                if (item.AccountID != testdata.AccountID)
                {
                    createdAcntID = item.AccountID;
                    listCreatedAccount.Add(createdAcntID);
                }
            }
            Assert.True(createdAcntID != -1);

            // Verify the template docs.
            FinanceTmpDPDocumentsController tmpcontroller = new FinanceTmpDPDocumentsController(context);
            tmpcontroller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // Perform the get template docs.
            var getresult = tmpcontroller.Get();
            Assert.NotNull(getresult);
            var gettmpokresult = Assert.IsType<OkObjectResult>(getresult);
            var tmpdocs = Assert.IsAssignableFrom<IQueryable<FinanceTmpDPDocument>>(gettmpokresult.Value);
            Assert.Equal(dpdates.Count, tmpdocs.Count());

            //acntDP.DPTmpDocs.OrderBy(p => p.TransactionDate);

            // Perform the post
            foreach (var tmpdoc in tmpdocs)
            {
                var tmppostcontex = new FinanceTmpDPDocumentPostContext();
                tmppostcontex.HomeID = testdata.HomeID;
                tmppostcontex.AccountID = createdAcntID;
                tmppostcontex.DocumentID = tmpdoc.DocumentID;
                var tmppostresult = await tmpcontroller.PostDocument(tmppostcontex);
                Assert.NotNull(tmppostresult);
                var tmppostokrst = Assert.IsType<OkObjectResult>(tmppostresult);
                var tmppostdoc = Assert.IsType<FinanceDocument>(tmppostokrst.Value);
                listCreatedDocs.Add(tmppostdoc.ID);
            }

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceDocumentsControllerTestData_LoanDoc> BorrowFromDocs =>
            new TheoryData<FinanceDocumentsControllerTestData_LoanDoc>
            {
                new FinanceDocumentsControllerTestData_LoanDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,

                    AccountName = "Test Loan 1",
                    StartDate = new DateTime(2022, 2, 1),
                    EndDate = new DateTime(2023, 2, 1),
                    InterestFree = true,
                    RepaymentMethod = LoanRepaymentMethod.DueRepayment,


                    Frequency = RepeatFrequency.Month,
                    Comment = "Test Loan 1",
                    TmpDocControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    TmpDocTranType = DataSetupUtility.TranType_Expense1,

                    PostLoanTmpDocs = false,
                },
                new FinanceDocumentsControllerTestData_LoanDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,

                    AccountName = "Test Loan 2",
                    StartDate = new DateTime(2022, 2, 1),
                    EndDate = new DateTime(2023, 2, 1),
                    InterestFree = true,
                    RepaymentMethod = LoanRepaymentMethod.DueRepayment,


                    Frequency = RepeatFrequency.Month,
                    Comment = "Test Loan 2",
                    TmpDocControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    TmpDocTranType = DataSetupUtility.TranType_Expense1,

                    PostLoanTmpDocs = true,
                    RepayAccountID = DataSetupUtility.Home1CashAccount1ID,
                },
                new FinanceDocumentsControllerTestData_LoanDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,

                    AccountName = "Test Loan 3",
                    StartDate = new DateTime(2022, 2, 1),
                    EndDate = new DateTime(2023, 2, 1),
                    InterestFree = false,
                    AnnualRate = 10,
                    RepaymentMethod = LoanRepaymentMethod.DueRepayment,

                    Frequency = RepeatFrequency.Month,
                    Comment = "Test Loan 3",
                    TmpDocControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    TmpDocTranType = DataSetupUtility.TranType_Expense1,

                    PostLoanTmpDocs = false,
                },
                new FinanceDocumentsControllerTestData_LoanDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,

                    AccountName = "Test Loan 4",
                    StartDate = new DateTime(2022, 2, 1),
                    EndDate = new DateTime(2023, 2, 1),
                    InterestFree = false,
                    AnnualRate = 10,
                    RepaymentMethod = LoanRepaymentMethod.DueRepayment,

                    Frequency = RepeatFrequency.Month,
                    Comment = "Test Loan 4",
                    TmpDocControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    TmpDocTranType = DataSetupUtility.TranType_Expense1,

                    PostLoanTmpDocs = true,
                    RepayAccountID = DataSetupUtility.Home1CashAccount2ID,
                },
            };

        [Theory]
        [MemberData(nameof(BorrowFromDocs))]
        public async Task TestCase_BorrowFrom(FinanceDocumentsControllerTestData_LoanDoc testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(testdata.HomeID, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var loanContext = new FinanceLoanDocumentCreateContext();
            var acntLoan = new FinanceAccountExtraLoan()
            {
                StartDate = testdata.StartDate,
                EndDate = testdata.EndDate,
                InterestFree = testdata.InterestFree,
                AnnualRate = testdata.AnnualRate,
                RepaymentMethod = testdata.RepaymentMethod,
                TotalMonths = testdata.TotalMonths,
                Others = testdata.Others,
                PayingAccount = testdata.PayingAccount,
                Partner = testdata.Partner,
            };
            var dpdates = CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(new RepeatDatesWithAmountAndInterestCalInput()
            {
                StartDate = acntLoan.StartDate,
                EndDate = acntLoan.EndDate,
                RepaymentMethod = acntLoan.RepaymentMethod.GetValueOrDefault(),
                TotalAmount = testdata.Amount,
                InterestFreeLoan = acntLoan.InterestFree.GetValueOrDefault(),
                InterestRate = acntLoan.AnnualRate.GetValueOrDefault(),
            });
            int tdocid = 1;
            dpdates.ForEach(d =>
            {
                acntLoan.LoanTmpDocs.Add(new FinanceTmpLoanDocument()
                {
                    DocumentID = tdocid++,
                    HomeID = testdata.HomeID,
                    TransactionAmount = d.TranAmount,
                    InterestAmount = d.InterestAmount,
                    TransactionDate = d.TranDate,
                    ControlCenterID = testdata.TmpDocControlCenterID,
                    OrderID = testdata.TmpDocOrderID,
                    Description = testdata.Comment,
                });
            });

            loanContext.DocumentInfo = new FinanceDocument
            {
                HomeID = testdata.HomeID,
                TranCurr = testdata.Currency,
                TranDate = testdata.TranDate,
                Desp = testdata.Comment,
                DocType = FinanceDocumentType.DocType_BorrowFrom,
                Items = new List<FinanceDocumentItem> 
                {
                    new FinanceDocumentItem()
                    {
                        ItemID = 1,
                        AccountID = testdata.AccountID,
                        ControlCenterID = testdata.ControlCenterID,
                        OrderID = testdata.OrderID,
                        TranType = FinanceTransactionType.TranType_BorrowFrom,
                        TranAmount = testdata.Amount,
                        Desp = testdata.Comment,
                    }
                },
            };
            loanContext.AccountInfo = new FinanceAccount
            {
                HomeID = testdata.HomeID,
                CategoryID = FinanceAccountCategory.AccountCategory_BorrowFrom,
                Name = testdata.AccountName,
                Status = (byte)FinanceAccountStatus.Normal,
                ExtraLoan = acntLoan,
            };
            var postloanrst = await control.PostLoanDocument(loanContext);
            Assert.NotNull(postloanrst);
            var postokrst = Assert.IsType<OkObjectResult>(postloanrst);
            var createdoc = Assert.IsType<FinanceDocument>(postokrst.Value);
            listCreatedDocs.Add(createdoc.ID);
            ValidateDocument(loanContext.DocumentInfo, createdoc, true, true);
            var createdAcntID = -1;
            foreach (var item in createdoc.Items)
            {
                if (item.AccountID != testdata.AccountID)
                {
                    createdAcntID = item.AccountID;
                    listCreatedAccount.Add(createdAcntID);
                }
            }
            Assert.True(createdAcntID != -1);
            listCreatedAccount.Add(createdAcntID);

            // Verify the template docs.
            FinanceTmpLoanDocumentsController tmpcontroller = new FinanceTmpLoanDocumentsController(context);
            tmpcontroller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // Perform the get template docs.
            var getresult = tmpcontroller.Get();
            Assert.NotNull(getresult);
            var gettmpokresult = Assert.IsType<OkObjectResult>(getresult);
            var tmpdocs = Assert.IsAssignableFrom<IQueryable<FinanceTmpLoanDocument>>(gettmpokresult.Value);
            Assert.Equal(dpdates.Count, tmpdocs.Count());

            // Post the loan document.
            if (testdata.PostLoanTmpDocs)
            {
                foreach (var tmpdoc in tmpdocs)
                {
                    var loanrepayctx = new FinanceLoanRepayDocumentCreateContext();
                    loanrepayctx.HomeID = testdata.HomeID;
                    loanrepayctx.LoanTemplateDocumentID = tmpdoc.DocumentID;
                    loanrepayctx.DocumentInfo = new FinanceDocument()
                    {
                        DocType = FinanceDocumentType.DocType_Repay,
                        HomeID = testdata.HomeID,
                        TranDate = tmpdoc.TransactionDate,
                        TranCurr = testdata.Currency,
                        Desp = tmpdoc.Description,
                        Items = new List<FinanceDocumentItem> { 
                            new FinanceDocumentItem
                            {
                                ItemID = 1,
                                AccountID = testdata.RepayAccountID,
                                TranAmount = tmpdoc.TransactionAmount,
                                TranType = FinanceTransactionType.TranType_RepaymentOut,
                                ControlCenterID = testdata.TmpDocControlCenterID,
                                OrderID = testdata.TmpDocOrderID,
                                Desp = tmpdoc.Description,
                            },
                            new FinanceDocumentItem
                            {
                                ItemID = 2,
                                AccountID = createdAcntID,
                                TranAmount = tmpdoc.TransactionAmount,
                                TranType = FinanceTransactionType.TranType_RepaymentIn,
                                ControlCenterID = testdata.TmpDocControlCenterID,
                                OrderID = testdata.TmpDocOrderID,
                                Desp = tmpdoc.Description,
                            },
                        },
                    };
                    if (tmpdoc.InterestAmount > 0)
                    {
                        loanrepayctx.DocumentInfo.Items.Add(new FinanceDocumentItem
                        {
                            ItemID = 3,
                            AccountID = testdata.RepayAccountID,
                            TranType = FinanceTransactionType.TranType_InterestOut,
                            TranAmount = tmpdoc.InterestAmount.GetValueOrDefault(),
                            ControlCenterID = testdata.TmpDocControlCenterID,
                            OrderID = testdata.TmpDocOrderID,
                            Desp = tmpdoc.Description,
                        });
                    }
                    var repayrst = await tmpcontroller.PostRepayDocument(loanrepayctx);
                    Assert.NotNull(repayrst);
                    var repayokrst = Assert.IsType<OkObjectResult>(repayrst);
                    var repaydoc = Assert.IsType<FinanceDocument>(repayokrst.Value);
                    listCreatedDocs.Add(repaydoc.ID);
                }

                // After all tmp. docs posted, the account status shall be updated.
            }

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceDocumentsControllerTestData_AssetBuyDoc> AssetBuyDocs =>
            new TheoryData<FinanceDocumentsControllerTestData_AssetBuyDoc>
            {
                new FinanceDocumentsControllerTestData_AssetBuyDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    Desp = "Test 1",

                    AccountExtra = new FinanceAccountExtraAS()
                    {
                        CategoryID = DataSetupUtility.Home1AssetCategory1ID,
                        Name = "Test1"
                    },
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Expense3,
                            TranAmount = 100000,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                            Desp = "Test 1",
                        }
                    }
                },
                new FinanceDocumentsControllerTestData_AssetBuyDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    Desp = "Test 1",

                    AccountExtra = new FinanceAccountExtraAS()
                    {
                        CategoryID = DataSetupUtility.Home1AssetCategory1ID,
                        Name = "Test1"
                    },
                    IsLegacy = true,
                },
            };

        [Theory]
        [MemberData(nameof(AssetBuyDocs))]
        public async Task TestCase_BuyAsset(FinanceDocumentsControllerTestData_AssetBuyDoc testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(testdata.HomeID, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var assetbuycontext = new FinanceAssetBuyDocumentCreateContext();
            assetbuycontext.HID = testdata.HomeID;
            assetbuycontext.AccountOwner = testdata.CurrentUser;
            assetbuycontext.ControlCenterID = testdata.ControlCenterID;
            assetbuycontext.Desp = testdata.Desp;
            assetbuycontext.TranAmount = testdata.Amount;
            assetbuycontext.TranDate = testdata.TranDate;
            assetbuycontext.IsLegacy = testdata.IsLegacy;
            assetbuycontext.ExtraAsset = testdata.AccountExtra;
            assetbuycontext.TranCurr = testdata.Currency;
            assetbuycontext.Items = testdata.Items;

            var buydocrst = await control.PostAssetBuyDocument(assetbuycontext);
            Assert.NotNull(buydocrst);
            var buydocokrst = Assert.IsType<OkObjectResult>(buydocrst);
            var buydoc = Assert.IsType<FinanceDocument>(buydocokrst.Value);
            listCreatedDocs.Add(buydoc.ID);

            var nacntid = -1;
            foreach (var item in buydoc.Items)
            {
                if (item.TranType == FinanceTransactionType.TranType_OpeningAsset)
                    nacntid = item.AccountID;
            }
            Assert.True(nacntid != -1);
            listCreatedAccount.Add(nacntid);

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceDocumentsControllerTestData_AssetChangeDoc> AssetChangeDocs =>
            new TheoryData<FinanceDocumentsControllerTestData_AssetChangeDoc>
            {
                new FinanceDocumentsControllerTestData_AssetChangeDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    Desp = "Test 1",
                    NewAmount = 1000,

                    AccountExtra = new FinanceAccountExtraAS()
                    {
                        CategoryID = DataSetupUtility.Home1AssetCategory1ID,
                        Name = "Test1"
                    },
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Expense3,
                            TranAmount = 100000,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                            Desp = "Test 1",
                        }
                    }
                },
                new FinanceDocumentsControllerTestData_AssetChangeDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    Desp = "Test 1",
                    NewAmount = 1000,

                    AccountExtra = new FinanceAccountExtraAS()
                    {
                        CategoryID = DataSetupUtility.Home1AssetCategory1ID,
                        Name = "Test1"
                    },
                    IsLegacy = true,
                },
            };

        [Theory]
        [MemberData(nameof(AssetChangeDocs))]
        public async Task TestCase_AssetChange(FinanceDocumentsControllerTestData_AssetChangeDoc testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(testdata.HomeID, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 1. Buy an asset
            var assetbuycontext = new FinanceAssetBuyDocumentCreateContext();
            assetbuycontext.HID = testdata.HomeID;
            assetbuycontext.AccountOwner = testdata.CurrentUser;
            assetbuycontext.ControlCenterID = testdata.ControlCenterID;
            assetbuycontext.Desp = testdata.Desp;
            assetbuycontext.TranAmount = testdata.Amount;
            assetbuycontext.TranDate = testdata.TranDate;
            assetbuycontext.IsLegacy = testdata.IsLegacy;
            assetbuycontext.ExtraAsset = testdata.AccountExtra;
            assetbuycontext.TranCurr = testdata.Currency;
            assetbuycontext.Items = testdata.Items;

            var buydocrst = await control.PostAssetBuyDocument(assetbuycontext);
            Assert.NotNull(buydocrst);
            var buydocokrst = Assert.IsType<OkObjectResult>(buydocrst);
            var buydoc = Assert.IsType<FinanceDocument>(buydocokrst.Value);
            listCreatedDocs.Add(buydoc.ID);

            var nacntid = -1;
            foreach (var item in buydoc.Items)
            {
                if (item.TranType == FinanceTransactionType.TranType_OpeningAsset)
                    nacntid = item.AccountID;
            }
            Assert.True(nacntid != -1);
            listCreatedAccount.Add(nacntid);

            // 2. Asset value change
            var assetchangecontext = new FinanceAssetRevaluationDocumentCreateContext();
            assetchangecontext.HID = testdata.HomeID;
            assetchangecontext.ControlCenterID = testdata.ControlCenterID;
            assetchangecontext.OrderID = testdata.OrderID;
            assetchangecontext.Desp = testdata.Desp;
            assetchangecontext.TranDate = new DateTime(testdata.TranDate.Ticks).AddDays(1);
            assetchangecontext.TranCurr = testdata.Currency;
            assetchangecontext.AssetAccountID = nacntid;
            var amtdiff = testdata.NewAmount - testdata.Amount;
            assetchangecontext.Items = new List<FinanceDocumentItem> 
            {
                new FinanceDocumentItem
                {
                    ItemID = 1,
                    AccountID = nacntid,
                    TranType = amtdiff > 0? FinanceTransactionType.TranType_AssetValueIncrease : FinanceTransactionType.TranType_AssetValueDecrease,
                    TranAmount = Math.Abs(amtdiff),
                    Desp = testdata.Desp,
                    ControlCenterID = testdata.ControlCenterID,
                    OrderID = testdata.OrderID,
                },
            };
            var changerst = await control.PostAssetValueChangeDocument(assetchangecontext);
            Assert.NotNull(changerst);

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceDocumentsControllerTestData_AssetSellDoc> AssetSellDocs =>
            new TheoryData<FinanceDocumentsControllerTestData_AssetSellDoc>
            {
                new FinanceDocumentsControllerTestData_AssetSellDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    Desp = "Test 1",

                    NewAmount = 500,
                    CashAccountID = DataSetupUtility.Home1CashAccount2ID,

                    AccountExtra = new FinanceAccountExtraAS()
                    {
                        CategoryID = DataSetupUtility.Home1AssetCategory1ID,
                        Name = "Test1"
                    },
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Expense3,
                            TranAmount = 100000,
                            ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                            Desp = "Test 1",
                        }
                    }
                },
                new FinanceDocumentsControllerTestData_AssetSellDoc()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CurrentUser = DataSetupUtility.UserA,
                    Currency = DataSetupUtility.Home1BaseCurrency,
                    TranDate = new DateTime(2022, 1, 30),
                    Amount = 100000,
                    AccountID = DataSetupUtility.Home1CashAccount1ID,
                    ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                    Desp = "Test 1",

                    NewAmount = 500,
                    CashAccountID = DataSetupUtility.Home1CashAccount2ID,

                    AccountExtra = new FinanceAccountExtraAS()
                    {
                        CategoryID = DataSetupUtility.Home1AssetCategory1ID,
                        Name = "Test1"
                    },
                    IsLegacy = true,
                },
            };
        
        [Theory]
        [MemberData(nameof(AssetSellDocs))]
        public async Task TestCase_AssetSellCreate(FinanceDocumentsControllerTestData_AssetSellDoc testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(testdata.HomeID, context);

            FinanceDocumentsController control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            // 1. Create asset
            var assetbuycontext = new FinanceAssetBuyDocumentCreateContext();
            assetbuycontext.HID = testdata.HomeID;
            assetbuycontext.AccountOwner = testdata.CurrentUser;
            assetbuycontext.ControlCenterID = testdata.ControlCenterID;
            assetbuycontext.Desp = testdata.Desp;
            assetbuycontext.TranAmount = testdata.Amount;
            assetbuycontext.TranDate = testdata.TranDate;
            assetbuycontext.IsLegacy = testdata.IsLegacy;
            assetbuycontext.ExtraAsset = testdata.AccountExtra;
            assetbuycontext.TranCurr = testdata.Currency;
            assetbuycontext.Items = testdata.Items;

            var buydocrst = await control.PostAssetBuyDocument(assetbuycontext);
            Assert.NotNull(buydocrst);
            var buydocokrst = Assert.IsType<OkObjectResult>(buydocrst);
            var buydoc = Assert.IsType<FinanceDocument>(buydocokrst.Value);
            listCreatedDocs.Add(buydoc.ID);

            var nacntid = -1;
            foreach (var item in buydoc.Items)
            {
                if (item.TranType == FinanceTransactionType.TranType_OpeningAsset)
                    nacntid = item.AccountID;
            }
            Assert.True(nacntid != -1);
            listCreatedAccount.Add(nacntid);

            // 2. Delete account
            var sellContext = new FinanceAssetSellDocumentCreateContext();
            sellContext.TranDate = new DateTime(testdata.TranDate.Ticks).AddDays(1);
            sellContext.HID = testdata.HomeID;
            sellContext.TranAmount = testdata.NewAmount;
            sellContext.AssetAccountID = nacntid;
            sellContext.ControlCenterID = testdata.ControlCenterID;
            sellContext.OrderID = testdata.OrderID;
            sellContext.Desp = testdata.Desp;
            sellContext.TranCurr = testdata.Currency;
            sellContext.Items = new List<FinanceDocumentItem>
            {
                new FinanceDocumentItem
                {
                    ItemID = 1,
                    AccountID = testdata.CashAccountID,
                    Desp = testdata.Desp,
                    TranAmount = testdata.NewAmount,
                    TranType = FinanceTransactionType.TranType_AssetSoldoutIncome,
                    ControlCenterID = testdata.ControlCenterID,
                    OrderID = testdata.OrderID,
                }
            };
            var sellrst = await control.PostAssetSellDocument(sellContext);
            Assert.NotNull(sellrst);

            // 3. Read all documents
            var getrst = control.Get();
            Assert.NotNull(getrst);

            await context.DisposeAsync();
        }

        private void ValidateDocumentInDB(FinanceDocument expdoc, hihDataContext context)
        {
            var docinDB = context.FinanceDocument
                .Include(prop => prop.Items)
                .Single(p => p.ID == expdoc.ID);
            ValidateDocument(expdoc, docinDB, false);
        }
        private void ValidateDocument(FinanceDocument expdoc, FinanceDocument actdoc, bool excludeID = true, bool excludeItem = false)
        {
            if (!excludeID)
                Assert.Equal(expdoc.ID, actdoc.ID);
            Assert.Equal(expdoc.Desp, actdoc.Desp);
            Assert.Equal(expdoc.DocType, actdoc.DocType);
            Assert.Equal(expdoc.HomeID, actdoc.HomeID);
            Assert.Equal(expdoc.TranCurr, actdoc.TranCurr);
            Assert.Equal(expdoc.ExgRate, actdoc.ExgRate);
            Assert.Equal(expdoc.ExgRate_Plan, actdoc.ExgRate_Plan);
            Assert.Equal(expdoc.ExgRate2, actdoc.ExgRate2);
            Assert.Equal(expdoc.ExgRate_Plan2, actdoc.ExgRate_Plan2);
            if (!excludeItem)
            {
                Assert.True(expdoc.Items.Count == actdoc.Items.Count);
                foreach (var expitem in expdoc.Items)
                {
                    var actitem = actdoc.Items.FirstOrDefault(p => p.ItemID == expitem.ItemID);
                    Assert.NotNull(actitem);
                    Assert.Equal(expitem.AccountID, actitem.AccountID);
                    Assert.Equal(expitem.ControlCenterID, actitem.ControlCenterID);
                    Assert.Equal(expitem.OrderID, actitem.OrderID);
                    Assert.Equal(expitem.TranAmount, actitem.TranAmount);
                    Assert.Equal(expitem.Desp, actitem.Desp);
                    Assert.Equal(expitem.TranType, actitem.TranType);
                }
            }
        }
    }
}
