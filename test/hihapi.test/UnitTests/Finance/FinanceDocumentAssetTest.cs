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
using Microsoft.EntityFrameworkCore;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentAssetTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;

        public FinanceDocumentAssetTest(SqliteDatabaseFixture fixture)
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, false)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.UserA, true)]
        public async Task TestCase1(int hid, string currency, string user, Boolean islegacy)
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

            // 1. Buy an asset
            var control = new FinanceDocumentsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // 1a. Prepare data
            var assetbuycontext = new FinanceAssetBuyDocumentCreateContext();
            assetbuycontext.AccountOwner = user;
            assetbuycontext.ControlCenterID = controlCentersCreated[0];
            assetbuycontext.Desp = "Test buy in";
            // assetbuycontext.ExtraAsset = 
            assetbuycontext.HID = hid;
            assetbuycontext.IsLegacy = islegacy;
            assetbuycontext.TranAmount = 2000;
            assetbuycontext.TranCurr = currency;
            assetbuycontext.TranDate = new DateTime(2020, 1, 1);

            if (!islegacy)
            {
                var item2 = new FinanceDocumentItem()
                {
                    ItemID = 1,
                    Desp = "Item 1.1",
                    TranType = 3,
                    TranAmount = 2000,
                    AccountID = accountsCreated[0],
                    ControlCenterID = controlCentersCreated[0],
                };
                assetbuycontext.Items.Add(item2);
            }
            assetbuycontext.ExtraAsset = new FinanceAccountExtraAS();
            assetbuycontext.ExtraAsset.Name = "Asset to buy";
            assetbuycontext.ExtraAsset.CategoryID = context.FinAssetCategories.ToList()[0].ID;
            assetbuycontext.ExtraAsset.Comment = "Test 1";
            
            var resp = await control.PostAssetBuyDocument(assetbuycontext);
            var doc = Assert.IsType<CreatedODataResult<FinanceDocument>>(resp).Entity;
            documentsCreated.Add(doc.ID);
            if (islegacy)
                Assert.True(doc.Items.Count == 1);
            else
                Assert.True(doc.Items.Count == 2);

            var assetacntid = 0;
            if (!islegacy)
            {
                foreach (var docitem in doc.Items)
                {
                    if (docitem.AccountID != accountsCreated[0])
                    {
                        assetacntid = docitem.AccountID;
                        accountsCreated.Add(docitem.AccountID);

                        var acnt = context.FinanceAccount.Find(docitem.AccountID);
                        Assert.NotNull(acnt);
                        var acntExtraAsset = context.FinanceAccountExtraAS.Find(docitem.AccountID);
                        Assert.NotNull(acntExtraAsset);
                        Assert.True(acntExtraAsset.RefenceBuyDocumentID == doc.ID);
                    }
                }
            }
            else
            {
                // The return result has no account info.
                var acntExtraAsset = context.FinanceAccountExtraAS.First(p => p.RefenceBuyDocumentID == doc.ID);
                Assert.NotNull(acntExtraAsset);

                assetacntid = acntExtraAsset.AccountID;
            }
            // Now check in the databse for items
            var ditems = context.FinanceDocumentItem.Where(p => p.AccountID == assetacntid).ToList();
            Assert.True(ditems.Count == 1);
            // Document item view
            var ditemview = (from diview in context.FinanceDocumentItemView
                            where diview.AccountID == assetacntid
                            select new { diview.AccountID, diview.Amount, diview.IsExpense }
                            ).ToList();
            Assert.True(ditemview.Count == 1);
            // Check the account group with expense
            var acntgrpexp = context.FinanceReporAccountGroupAndExpenseView.Where(p => p.AccountID == assetacntid).First();
            Assert.NotNull(acntgrpexp);
            // Check the balance
            var assetbal = context.FinanceReporAccountGroupView.Where(p => p.AccountID == assetacntid).First();
            Assert.NotNull(assetbal);
            Assert.Equal(2000, assetbal.Balance);

            // Okay, now sell it
            var assetsellcontext = new FinanceAssetSellDocumentCreateContext();
            assetsellcontext.AssetAccountID = assetacntid;
            assetsellcontext.ControlCenterID = controlCentersCreated[0];
            assetsellcontext.Desp = "Test sell";
            assetsellcontext.HID = hid;
            assetsellcontext.TranAmount = 1000;
            assetsellcontext.TranCurr = currency;
            assetsellcontext.TranDate = new DateTime(2021, 1, 1);
            // Account which received the money
            var item = new FinanceDocumentItem()
            {
                ItemID = 1,
                Desp = "Item 2.1",
                TranType = FinanceTransactionType.TranType_AssetSoldoutIncome,
                TranAmount = 1000,
                AccountID = accountsCreated[0],
                ControlCenterID = controlCentersCreated[0],
            };
            assetsellcontext.Items.Add(item);

            resp = await control.PostAssetSellDocument(assetsellcontext);
            doc = Assert.IsType<CreatedODataResult<FinanceDocument>>(resp).Entity;
            documentsCreated.Add(doc.ID);

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
