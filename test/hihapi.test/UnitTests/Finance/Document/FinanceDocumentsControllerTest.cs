using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Deltas;

namespace hihapi.test.UnitTests.Finance
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
            Assert.Equal(FinanceDocumentType.DocType_Normal, createDocResult.Entity.DocType);
            Assert.Equal(normalDoc.HomeID, createDocResult.Entity.HomeID);
            Assert.Equal(normalDoc.Desp, createDocResult.Entity.Desp);
            Assert.Equal(normalDoc.Currency, createDocResult.Entity.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, createDocResult.Entity.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, createDocResult.Entity.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, createDocResult.Entity.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, createDocResult.Entity.ExgRate_Plan2);
            Assert.True(createDocResult.Entity.Items.Count == normalDoc.DocItems.Count);
            // Verify the attributes of all items
            var itemEnum = createDocResult.Entity.Items.GetEnumerator();
            while(itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                var orgidx = normalDoc.DocItems.FindIndex(p => p.ItemID == docitem.ItemID);
                Assert.NotEqual(-1, orgidx);
                Assert.Equal(normalDoc.DocItems[orgidx].AccountID, docitem.AccountID);
                Assert.Equal(normalDoc.DocItems[orgidx].ControlCenterID, docitem.ControlCenterID);
                Assert.Equal(normalDoc.DocItems[orgidx].OrderID, docitem.OrderID);
                Assert.Equal(normalDoc.DocItems[orgidx].Amount, docitem.TranAmount);
                Assert.Equal(normalDoc.DocItems[orgidx].Desp, docitem.Desp);
                Assert.Equal(normalDoc.DocItems[orgidx].TranType, docitem.TranType);
            }
            // Store the Doc ID for deletion
            ndocid = createDocResult.Entity.ID;
            listCreatedDocs.Add(ndocid);
            doc.ID = ndocid;

            // 3. Read it out.
            var getresult = control.Get(ndocid);
            Assert.NotNull(getresult);
            // Verify the attributes
            Assert.Equal(FinanceDocumentType.DocType_Normal, getresult.DocType);
            Assert.Equal(normalDoc.HomeID, getresult.HomeID);
            Assert.Equal(normalDoc.Desp, getresult.Desp);
            Assert.Equal(normalDoc.Currency, getresult.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, getresult.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, getresult.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, getresult.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, getresult.ExgRate_Plan2);

            // 4. Do the changes to normal docs.
            // 4.1 Change the desp
            doc.Desp += "Changed";
            var changeResult = await control.Put(ndocid, doc);
            var changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            var changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            Assert.Equal(doc.Desp, changedDoc.Desp);
            Assert.Equal(FinanceDocumentType.DocType_Normal, changedDoc.DocType);
            Assert.Equal(normalDoc.HomeID, changedDoc.HomeID);
            Assert.Equal(normalDoc.Currency, changedDoc.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, changedDoc.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, changedDoc.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, changedDoc.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, changedDoc.ExgRate_Plan2);
            Assert.True(changedDoc.Items.Count == normalDoc.DocItems.Count);
            itemEnum = changedDoc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                var orgidx = normalDoc.DocItems.FindIndex(p => p.ItemID == docitem.ItemID);
                Assert.NotEqual(-1, orgidx);
                Assert.Equal(normalDoc.DocItems[orgidx].AccountID, docitem.AccountID);
                Assert.Equal(normalDoc.DocItems[orgidx].ControlCenterID, docitem.ControlCenterID);
                Assert.Equal(normalDoc.DocItems[orgidx].OrderID, docitem.OrderID);
                Assert.Equal(normalDoc.DocItems[orgidx].Amount, docitem.TranAmount);
                Assert.Equal(normalDoc.DocItems[orgidx].Desp, docitem.Desp);
                Assert.Equal(normalDoc.DocItems[orgidx].TranType, docitem.TranType);
            }

            // 4.2 Add the item
            int itemmaxid = normalDoc.DocItems.Max(p => p.ItemID);
            itemmaxid++;
            var newitemdata = new FinanceDocumentsControllerTestData_DocItem();
            newitemdata.ItemID = itemmaxid;
            newitemdata.AccountID = normalDoc.DocItems[0].AccountID;
            newitemdata.Amount = 2 * normalDoc.DocItems[0].Amount;
            newitemdata.TranType = normalDoc.DocItems[0].TranType;
            newitemdata.Desp = normalDoc.DocItems[0].Desp;
            newitemdata.ControlCenterID = normalDoc.DocItems[0].ControlCenterID;
            newitemdata.OrderID = normalDoc.DocItems[0].OrderID;
            newitemdata.UseCurr2 = normalDoc.DocItems[0].UseCurr2;
            normalDoc.DocItems.Add(newitemdata);
            doc.Items.Add(new FinanceDocumentItem
            {
                ItemID = newitemdata.ItemID,
                AccountID = newitemdata.AccountID,
                TranAmount = newitemdata.Amount,
                TranType = newitemdata.TranType,
                Desp = newitemdata.Desp,
                ControlCenterID = newitemdata.ControlCenterID,
                OrderID = newitemdata.OrderID,
                UseCurr2 = newitemdata.UseCurr2
            });

            changeResult = await control.Put(ndocid, doc);
            changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            Assert.Equal(getresult.Desp, changedDoc.Desp);
            Assert.Equal(FinanceDocumentType.DocType_Normal, changedDoc.DocType);
            Assert.Equal(normalDoc.HomeID, changedDoc.HomeID);
            Assert.Equal(normalDoc.Currency, changedDoc.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, changedDoc.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, changedDoc.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, changedDoc.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, changedDoc.ExgRate_Plan2);
            Assert.True(changedDoc.Items.Count == normalDoc.DocItems.Count);
            itemEnum = changedDoc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                var orgidx = normalDoc.DocItems.FindIndex(p => p.ItemID == docitem.ItemID);
                Assert.NotEqual(-1, orgidx);
                Assert.Equal(normalDoc.DocItems[orgidx].AccountID, docitem.AccountID);
                Assert.Equal(normalDoc.DocItems[orgidx].ControlCenterID, docitem.ControlCenterID);
                Assert.Equal(normalDoc.DocItems[orgidx].OrderID, docitem.OrderID);
                Assert.Equal(normalDoc.DocItems[orgidx].Amount, docitem.TranAmount);
                Assert.Equal(normalDoc.DocItems[orgidx].Desp, docitem.Desp);
                Assert.Equal(normalDoc.DocItems[orgidx].TranType, docitem.TranType);
            }

            // 4.3 Update the item with min. id
            int itemminid = normalDoc.DocItems.Min(p => p.ItemID);
            normalDoc.DocItems.ForEach(nitem =>
            {
                if (nitem.ItemID == itemminid)
                {
                    nitem.Amount += 100;
                }
            });
            itemEnum = doc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                if (docitem.ItemID == itemminid)
                {
                    docitem.TranAmount = 100 + docitem.TranAmount;
                }
            }
            changeResult = await control.Put(ndocid, doc);
            changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            Assert.Equal(getresult.Desp, changedDoc.Desp);
            Assert.Equal(FinanceDocumentType.DocType_Normal, changedDoc.DocType);
            Assert.Equal(normalDoc.HomeID, changedDoc.HomeID);
            Assert.Equal(normalDoc.Currency, changedDoc.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, changedDoc.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, changedDoc.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, changedDoc.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, changedDoc.ExgRate_Plan2);
            Assert.True(changedDoc.Items.Count == normalDoc.DocItems.Count);
            itemEnum = changedDoc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                var orgidx = normalDoc.DocItems.FindIndex(p => p.ItemID == docitem.ItemID);
                Assert.NotEqual(-1, orgidx);
                Assert.Equal(normalDoc.DocItems[orgidx].AccountID, docitem.AccountID);
                Assert.Equal(normalDoc.DocItems[orgidx].ControlCenterID, docitem.ControlCenterID);
                Assert.Equal(normalDoc.DocItems[orgidx].OrderID, docitem.OrderID);
                Assert.Equal(normalDoc.DocItems[orgidx].Amount, docitem.TranAmount);
                Assert.Equal(normalDoc.DocItems[orgidx].Desp, docitem.Desp);
                Assert.Equal(normalDoc.DocItems[orgidx].TranType, docitem.TranType);
            }

            // 4.4 Delete an item
            // Delete the item with min. DI
            FinanceDocumentItem itemtbd = null;
            itemEnum = doc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                if (docitem.ItemID == itemminid)
                {
                    itemtbd = docitem;
                    break;
                }
            }
            Assert.NotNull(itemtbd);
            doc.Items.Remove(itemtbd); // <== delete the item with min. ID, keep the max. id
            normalDoc.DocItems.RemoveAt(normalDoc.DocItems.FindIndex(p => p.ItemID == itemminid));

            changeResult = await control.Put(ndocid, doc);
            changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            Assert.Equal(getresult.Desp, changedDoc.Desp);
            Assert.Equal(FinanceDocumentType.DocType_Normal, changedDoc.DocType);
            Assert.Equal(normalDoc.HomeID, changedDoc.HomeID);
            Assert.Equal(normalDoc.Currency, changedDoc.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, changedDoc.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, changedDoc.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, changedDoc.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, changedDoc.ExgRate_Plan2);
            Assert.True(changedDoc.Items.Count == normalDoc.DocItems.Count);
            itemEnum = changedDoc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                var orgidx = normalDoc.DocItems.FindIndex(p => p.ItemID == docitem.ItemID);
                Assert.NotEqual(-1, orgidx);
                Assert.Equal(normalDoc.DocItems[orgidx].AccountID, docitem.AccountID);
                Assert.Equal(normalDoc.DocItems[orgidx].ControlCenterID, docitem.ControlCenterID);
                Assert.Equal(normalDoc.DocItems[orgidx].OrderID, docitem.OrderID);
                Assert.Equal(normalDoc.DocItems[orgidx].Amount, docitem.TranAmount);
                Assert.Equal(normalDoc.DocItems[orgidx].Desp, docitem.Desp);
                Assert.Equal(normalDoc.DocItems[orgidx].TranType, docitem.TranType);
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
    }
}
