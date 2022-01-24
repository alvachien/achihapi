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

namespace hihapi.test.UnitTests.Finance.Document
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
            int itemcnt = 0;
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
            itemcnt = normalDoc.DocItems.Count;
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
            Assert.Single(createDocResult.Entity.Items);

            var createdDocResultItem = createDocResult.Entity.Items.FirstOrDefault();
            Assert.Equal(normalDoc.DocItems[0].AccountID, createdDocResultItem.AccountID);
            Assert.Equal(normalDoc.DocItems[0].ControlCenterID, createdDocResultItem.ControlCenterID);
            Assert.Equal(normalDoc.DocItems[0].OrderID, createdDocResultItem.OrderID);
            Assert.Equal(normalDoc.DocItems[0].Amount, createdDocResultItem.TranAmount);
            Assert.Equal(normalDoc.DocItems[0].Desp, createdDocResultItem.Desp);
            Assert.Equal(normalDoc.DocItems[0].TranType, createdDocResultItem.TranType);

            // Store the Doc ID for deletion
            ndocid = createDocResult.Entity.ID;
            listCreatedDocs.Add(ndocid);

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
            getresult.Desp += "Changed";
            var changeResult = await control.Put(ndocid, getresult);
            var changeokresult = Assert.IsType<OkObjectResult>(changeResult);
            var changedDoc = Assert.IsAssignableFrom<FinanceDocument>(changeokresult.Value as FinanceDocument);
            Assert.Equal(getresult.Desp, changedDoc.Desp);
            Assert.Equal(FinanceDocumentType.DocType_Normal, changedDoc.DocType);
            Assert.Equal(normalDoc.HomeID, changedDoc.HomeID);
            Assert.Equal(normalDoc.Currency, changedDoc.TranCurr);
            Assert.Equal(normalDoc.ExchangeRate, changedDoc.ExgRate);
            Assert.Equal(normalDoc.ExchangeRateIsPlanned, changedDoc.ExgRate_Plan);
            Assert.Equal(normalDoc.SecondExchangeRate, changedDoc.ExgRate2);
            Assert.Equal(normalDoc.SecondExchangeRateIsPlanned, changedDoc.ExgRate_Plan2);
            Assert.Single(changedDoc.Items);

            var changeDocItem = changedDoc.Items.FirstOrDefault();
            Assert.Equal(normalDoc.DocItems[0].AccountID, changeDocItem.AccountID);
            Assert.Equal(normalDoc.DocItems[0].ControlCenterID, changeDocItem.ControlCenterID);
            Assert.Equal(normalDoc.DocItems[0].OrderID, changeDocItem.OrderID);
            Assert.Equal(normalDoc.DocItems[0].Amount, changeDocItem.TranAmount);
            Assert.Equal(normalDoc.DocItems[0].Desp, changeDocItem.Desp);
            Assert.Equal(normalDoc.DocItems[0].TranType, changeDocItem.TranType);

            // 4.2 Add the item
            item = new FinanceDocumentItem();
            item.ItemID = 2;
            item.AccountID = normalDoc.DocItems[0].AccountID;
            item.TranAmount = normalDoc.DocItems[0].Amount;
            item.TranType = normalDoc.DocItems[0].TranType;
            item.Desp = normalDoc.DocItems[0].Desp;
            item.ControlCenterID = normalDoc.DocItems[0].ControlCenterID;
            item.OrderID = normalDoc.DocItems[0].OrderID;
            item.UseCurr2 = normalDoc.DocItems[0].UseCurr2;
            getresult.Items.Add(item);
            itemcnt++;
            Assert.True(getresult.Items.Count == itemcnt);

            changeResult = await control.Put(ndocid, getresult);
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
            Assert.True(changedDoc.Items.Count == itemcnt);

            // 4.3 Update the second item
            var itemEnum = changedDoc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                if (docitem.ItemID == 2)
                {
                    docitem.TranAmount = 2 * normalDoc.DocItems[0].Amount;
                }
            }
            changeResult = await control.Put(ndocid, getresult);
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
            Assert.True(changedDoc.Items.Count == itemcnt);
            itemEnum = changedDoc.Items.GetEnumerator();
            while (itemEnum.MoveNext())
            {
                var docitem = itemEnum.Current;
                if (docitem.ItemID == 2)
                {
                    Assert.Equal(2 * normalDoc.DocItems[0].Amount, docitem.TranAmount);
                }
                else if (docitem.ItemID == 1)
                {
                    Assert.Equal(normalDoc.DocItems[0].Amount, docitem.TranAmount);
                }
            }

            // 4.4 Delete an item
            getresult.Items.Clear(); // Delete all
            getresult.Items.Add(item); // <== Keep the second item. means, delete the first item

            changeResult = await control.Put(ndocid, getresult);
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
            Assert.Single(changedDoc.Items);
            changeDocItem = changedDoc.Items.FirstOrDefault();
            Assert.Equal(normalDoc.DocItems[0].AccountID, changeDocItem.AccountID);
            Assert.Equal(normalDoc.DocItems[0].ControlCenterID, changeDocItem.ControlCenterID);
            Assert.Equal(normalDoc.DocItems[0].OrderID, changeDocItem.OrderID);
            //Assert.Equal(normalDoc.DocItems[0].Amount, changeDocItem.TranAmount); // Amount has been changed above
            Assert.Equal(normalDoc.DocItems[0].Desp, changeDocItem.Desp);
            Assert.Equal(normalDoc.DocItems[0].TranType, changeDocItem.TranType);
            Assert.Equal(item.ItemID, changeDocItem.ItemID);

            // 5. Delete doc.
            var deleteResult = await control.Delete(ndocid);
            Assert.NotNull(deleteResult);
            var deletStatusCodeResult = Assert.IsType<StatusCodeResult>(deleteResult);
            Assert.NotNull(deletStatusCodeResult);
            Assert.Equal(204, deletStatusCodeResult.StatusCode);
            this.listCreatedDocs.Remove(ndocid);

            await context.DisposeAsync();
        }
    }
}
