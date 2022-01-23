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
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.test.UnitTests.Finance.Account
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public FinanceAccountsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
            if (this.listCreatedID.Count > 0)
            {
                this.listCreatedID.ForEach(x => this.fixture.DeleteFinanceAccount(this.fixture.GetCurrentDataContext(), x));                

                this.listCreatedID.Clear();
            }
            this.fixture.GetCurrentDataContext().SaveChanges();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, FinanceAccountCategory.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, FinanceAccountCategory.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID, FinanceAccountCategory.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home3ID, FinanceAccountCategory.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.UserC, DataSetupUtility.Home4ID, FinanceAccountCategory.AccountCategory_Cash)]
        [InlineData(DataSetupUtility.UserD, DataSetupUtility.Home5ID, FinanceAccountCategory.AccountCategory_Cash)]
        public async Task TestCase_Account(string user, int hid, int ctgyid)
        {
            var context = this.fixture.GetCurrentDataContext();
            // Pre. setup
            this.fixture.InitHomeTestData(hid, context);
            //var accountCount = context.FinanceAccount.Where(p => p.HomeID == hid).Count();

            FinanceAccountsController control = new FinanceAccountsController(context);

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

            // 2. Initial account list
            var result = control.Get();
            Assert.NotNull(result);
            var okresult = Assert.IsType<OkObjectResult>(result);
            var accounts = Assert.IsAssignableFrom<IEnumerable<FinanceAccount>>(okresult.Value as IEnumerable<FinanceAccount>);
            //var cnt = accounts.Count();
            //Assert.Equal(accountCount, cnt);

            // 3. Create account
            FinanceAccount acnt = new FinanceAccount();
            acnt.CategoryID = ctgyid;
            acnt.Name = user + ctgyid.ToString();
            acnt.Owner = user;
            acnt.HomeID = hid;
            acnt.Status = (byte)FinanceAccountStatus.Normal;
            acnt.Comment = acnt.Name;

            var postrst = await control.Post(acnt);
            Assert.NotNull(postrst);
            var createdresult = Assert.IsType<CreatedODataResult<FinanceAccount>>(postrst);
            Assert.NotNull(createdresult);

            var nacntid = createdresult.Entity.ID;
            this.listCreatedID.Add(nacntid);
            // Verify it in DB.
            var dbacnts = (from dbacnt in context.FinanceAccount
                          where dbacnt.ID == nacntid && dbacnt.HomeID == hid select dbacnt).ToList();
            Assert.Single(dbacnts);
            Assert.Equal(acnt.Name, dbacnts[0].Name);
            Assert.Equal(acnt.Owner, dbacnts[0].Owner);
            Assert.Equal(acnt.CategoryID, ctgyid);
            Assert.Equal(FinanceAccountStatus.Normal, (FinanceAccountStatus)dbacnts[0].Status);
            Assert.Equal(acnt.Comment, dbacnts[0].Comment);

            // 4. Get account
            var getresult = control.Get(nacntid);
            Assert.NotNull(getresult);
            Assert.Equal(acnt.Name, getresult.Name);
            Assert.Equal(acnt.Owner, getresult.Owner);
            Assert.Equal(acnt.CategoryID, getresult.CategoryID);
            Assert.Equal(FinanceAccountStatus.Normal, (FinanceAccountStatus)getresult.Status);
            Assert.Equal(acnt.Comment, getresult.Comment);

            await context.DisposeAsync();
        }
    }
}
