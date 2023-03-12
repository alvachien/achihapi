using System;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using hihapi.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using hihapi.test.common;
using hihapi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.OData.ModelBuilder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Identity.Client;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceReportsControllerTest2 : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedAccount = new List<int>();
        private List<int> listCreatedDocs = new List<int>();
        private Boolean isDataPrepared = false;

        public FinanceReportsControllerTest2(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }
        public void Dispose()
        {
            if (this.listCreatedAccount.Count > 0)
            {
                this.listCreatedAccount.ForEach(x => this.fixture.DeleteFinanceAccount(this.fixture.GetCurrentDataContext(), x));
                this.fixture.GetCurrentDataContext().SaveChanges();

                this.listCreatedAccount.Clear();
            }
            if (this.listCreatedDocs.Count > 0)
            {
                this.listCreatedDocs.ForEach(x => this.fixture.DeleteFinanceDocument(this.fixture.GetCurrentDataContext(), x));
                this.fixture.GetCurrentDataContext().SaveChanges();

                this.listCreatedDocs.Clear();
            }
        }

        private void prepareData()
        {
            if (!isDataPrepared)
            {
                // documents already there
                
                isDataPrepared = true;
            }
        }

        [Fact]
        public async Task TestCase_AccountBalance()
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", DataSetupUtility.Home1ID);
            parameters.Add("AccountID", DataSetupUtility.Home1CashAccount1ID);
            var userclaim = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var docitems = context.FinanceDocumentItem
                .Include(prop => prop.DocumentHeader)
                .Where(p => p.AccountID == DataSetupUtility.Home1CashAccount1ID);
            var trantype = context.FinTransactionType
                .Where(p => p.HomeID == null || p.HomeID == DataSetupUtility.Home1ID);
            Double amt = 0;
            foreach(var di in docitems)
            {
                var isexp = trantype.First(p => di.TranType == p.ID).Expense;

                if (isexp)
                    amt -= (double)di.TranAmount;
                else
                    amt += (double)di.TranAmount;
            }

            var balance = control.GetAccountBalance(parameters); 
            Assert.NotNull(balance);
            var balval = (double)((balance as OkObjectResult).Value);
            Assert.Equal(0.00, Math.Abs(amt - balval));
            
            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_AccountBalanceEx()
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", DataSetupUtility.Home1ID);
            parameters.Add("AccountID", DataSetupUtility.Home1CashAccount1ID);
            parameters.Add("SelectedDates", DataSetupUtility.Home1CashAccount1ID);
            List<DateTime> listDates = new List<DateTime>();
            var curyear = DateTime.Now.Year;
            var curmonth = DateTime.Now.Month;
            // Past 3 months
            if (curmonth == 1)
            {
                listDates.Add(new DateTime(curyear - 1, 11, 30));
                listDates.Add(new DateTime(curyear - 1, 12, 31));
                listDates.Add(new DateTime(curyear, curmonth, 31));
            }
            else if (curmonth == 2)
            {
                listDates.Add(new DateTime(curyear - 1, 12, 31));
                listDates.Add(new DateTime(curyear, 1, 31));
                listDates.Add(new DateTime(curyear, 3, 1).AddDays(-1));
            } 
            else
            {
                listDates.Add(new DateTime(curyear, curmonth - 1, 1).AddDays(-1));
                listDates.Add(new DateTime(curyear, curmonth, 1).AddDays(-1));
                listDates.Add(new DateTime(curyear, curmonth + 1, 1).AddDays(-1));
            }

            var userclaim = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var docitems = context.FinanceDocumentItem
                .Include(prop => prop.DocumentHeader)
                .Where(p => p.AccountID == DataSetupUtility.Home1CashAccount1ID);
            var trantype = context.FinTransactionType
                .Where(p => p.HomeID == null || p.HomeID == DataSetupUtility.Home1ID);
            Double amt = 0;
            foreach (var di in docitems)
            {
                var isexp = trantype.First(p => di.TranType == p.ID).Expense;

                if (isexp)
                    amt -= (double)di.TranAmount;
                else
                    amt += (double)di.TranAmount;
            }

            var balance = control.GetAccountBalanceEx(parameters);
            Assert.NotNull(balance);
            var balval = (double)((balance as OkObjectResult).Value);
            Assert.Equal(0.00, Math.Abs(amt - balval));

            await context.DisposeAsync();
        }
    }
}
