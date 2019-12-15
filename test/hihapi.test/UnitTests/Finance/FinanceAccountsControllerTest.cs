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
using Moq;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using System.Net.Http;
using Microsoft.OData.UriParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.OData.Edm;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountsControllerTest
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

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        public async Task TestCase1(int hid, string user)
        {
            // 0. Prepare the context
            var control = new FinanceAccountsController(this.fixture.CurrentDataContext);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var queryUrl = "http://localhost/api/FinanceAccounts";
            var context = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            // 1. Create first account
            var acnt = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account 1",
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Owner = DataSetupUtility.UserA
            };
            var rst = await control.Post(acnt);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinanceAccount>>(rst);
            Assert.Equal(rst2.Entity.Name, acnt.Name);
            Assert.Equal(rst2.Entity.HomeID, acnt.HomeID);
            Assert.Equal(rst2.Entity.CategoryID, acnt.CategoryID);
            Assert.Equal(rst2.Entity.Owner, DataSetupUtility.UserA);
            var firstacntid = rst2.Entity.ID;
            Assert.True(firstacntid > 0);

            // 2. Now read the whole accounts
            var req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceAccount>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceAccount>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Cast<FinanceAccount>().Count());

            // 3. Now create another one!
            acnt = new FinanceAccount()
            {
                HomeID = hid,
                Name = "Account 2",
                Comment = "Comment 2",
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Owner = DataSetupUtility.UserA
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
            acnt.Owner = DataSetupUtility.UserB;
            rst = await control.Put(secondacntid, acnt);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceAccount>>(rst);
            Assert.Equal(rst4.Entity.Name, acnt.Name);
            Assert.Equal(rst4.Entity.HomeID, acnt.HomeID);
            Assert.Equal(rst4.Entity.Owner, DataSetupUtility.UserB);

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
        }
    }
}

