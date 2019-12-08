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

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountsControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public FinanceAccountsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1_Home1_UserA()
        {
            // 0. Prepare the context
            var control = new FinanceAccountsController(this.fixture.CurrentDataContext);
            var user = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // 1. Create first account
            var acnt = new FinanceAccount()
            {
                HomeID = DataSetupUtility.Home1ID,
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

            // 2. Now read the whole control centers
            var rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Count());

            // 3. Now create another one!
            acnt = new FinanceAccount()
            {
                HomeID = DataSetupUtility.Home1ID,
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
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Count());

            // 7. Delete the first account
            rst5 = await control.Delete(firstacntid);
            Assert.NotNull(rst5);
            rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 8. Now read the whole control centers
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Count());
        }
    }
}

