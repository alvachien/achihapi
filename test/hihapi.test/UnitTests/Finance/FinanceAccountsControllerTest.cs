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
    public class FinanceAccountsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;

        public FinanceAccountsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceAccount>(provider, "FinanceAccounts");            
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
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, FinanceAccountCategoriesController.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB, FinanceAccountCategoriesController.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC, FinanceAccountCategoriesController.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD, FinanceAccountCategoriesController.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB, FinanceAccountCategoriesController.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, FinanceAccountCategoriesController.AccountCategory_Deposit)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB, FinanceAccountCategoriesController.AccountCategory_Deposit)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC, FinanceAccountCategoriesController.AccountCategory_Deposit)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD, FinanceAccountCategoriesController.AccountCategory_Deposit)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB, FinanceAccountCategoriesController.AccountCategory_Deposit)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, FinanceAccountCategoriesController.AccountCategory_Creditcard)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, FinanceAccountCategoriesController.AccountCategory_VirtualAccount)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, FinanceAccountCategoriesController.AccountCategory_AccountPayable)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, FinanceAccountCategoriesController.AccountCategory_AccountReceivable)]
        public async Task TestCase1_NormalAccount(int hid, string user, int ctgyid)
        {
            var context = this.fixture.GetCurrentDataContext();
            var secondhid = hid;

            // 0. Create accounts for other homes
            List<FinanceAccount> acntInOtherHomes = new List<FinanceAccount>();
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
            if (secondhid != hid)
            {
                var acnt1 = new FinanceAccount()
                {
                    HomeID = secondhid,
                    Name = "Account " + secondhid.ToString() + ".1_cash",
                    CategoryID = ctgyid,
                    Owner = user
                };
                var ec1 = context.FinanceAccount.Add(acnt1);
                acntInOtherHomes.Add(ec1.Entity);
                context.SaveChanges();
            }

            // 0a. Prepare the context
            var control = new FinanceAccountsController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };

            // 1. Create first account
            var acnt = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account_" + ctgyid.ToString() + ".1",
                CategoryID = ctgyid,
                Owner = user
            };
            var rst = await control.Post(acnt);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceAccount>>(rst);
            Assert.Equal(rst2.Entity.Name, acnt.Name);
            Assert.Equal(rst2.Entity.HomeID, acnt.HomeID);
            Assert.Equal(rst2.Entity.CategoryID, acnt.CategoryID);
            Assert.Equal(rst2.Entity.Owner, user);
            var firstacntid = rst2.Entity.ID;
            Assert.True(firstacntid > 0);

            // 2. Now read the whole accounts (no home ID applied)
            var queryUrl = "http://localhost/api/FinanceAccounts";
            var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceAccount>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceAccount>(odatacontext, req);
            var rst3 = control.Get(options);
            var expacntamt = acntInOtherHomes.Count() + 1;
            Assert.NotNull(rst3);
            Assert.Equal(expacntamt, rst3.Cast<FinanceAccount>().Count());

            // 2a. Read the whole accounts (with home ID applied)
            queryUrl = "http://localhost/api/FinanceAccounts?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceAccount>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceAccount>(odatacontext, req);
            rst3 = control.Get(options);
            expacntamt = 1;
            Assert.NotNull(rst3);
            Assert.Equal(expacntamt, rst3.Cast<FinanceAccount>().Count());

            // 3. Now create another one!
            acnt = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account_" + ctgyid.ToString() + ".2",
                Comment = "Comment 2",
                CategoryID = ctgyid,
                Owner = user
            };
            rst = await control.Post(acnt);
            Assert.NotNull(rst);
            rst2 = Assert.IsType<CreatedODataResult<FinanceAccount>>(rst);
            Assert.Equal(rst2.Entity.Name, acnt.Name);
            Assert.Equal(rst2.Entity.HomeID, acnt.HomeID);
            Assert.Equal(rst2.Entity.CategoryID, acnt.CategoryID);
            Assert.Equal(rst2.Entity.Owner, acnt.Owner);
            var secondacntid = rst2.Entity.ID;
            Assert.True(secondacntid > 0);

            // 4. Change one account
            acnt.Comment = "Comment 2 Updated";
            rst = await control.Put(secondacntid, acnt);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceAccount>>(rst);
            Assert.Equal(rst4.Entity.Name, acnt.Name);
            Assert.Equal(rst4.Entity.HomeID, acnt.HomeID);
            Assert.Equal(rst4.Entity.Comment, acnt.Comment);

            // 5. Delete the second account
            var rst5 = await control.Delete(secondacntid);
            Assert.NotNull(rst5);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 6. Now read the whole accounts
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Cast<FinanceAccount>().Count());

            // 7. Delete the first account
            rst5 = await control.Delete(firstacntid);
            Assert.NotNull(rst5);
            rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 8. Now read the whole control centers
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Cast<FinanceAccount>().Count());

            // LAST. Remove all pre-created control center
            context.FinanceAccount.RemoveRange(acntInOtherHomes);
            await context.SaveChangesAsync();

            await context.DisposeAsync();
        }
    }
}

