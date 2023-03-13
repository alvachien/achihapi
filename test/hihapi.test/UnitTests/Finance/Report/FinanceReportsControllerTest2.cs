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
using hihapi.Models;

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

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.Home1CashAccount1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID, DataSetupUtility.Home2CashAccount1ID)]
        public async Task TestCase_AccountBalance(String usr, int hid, int acntid)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("AccountID", acntid);
            var userclaim = DataSetupUtility.GetClaimForUser(usr);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            var docitems = context.FinanceDocumentItem
                .Include(prop => prop.DocumentHeader)
                .Where(p => p.AccountID == acntid);
            var trantype = context.FinTransactionType
                .Where(p => p.HomeID == null || p.HomeID == hid);
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
            Assert.Equal(0.00, Math.Abs(Math.Round(amt - balval, 2)));
            
            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.Home1CashAccount1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID, DataSetupUtility.Home2CashAccount1ID)]
        public async Task TestCase_AccountBalanceEx(String usr, int hid, int acntid)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("AccountID", acntid);
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
            List<String> listDateStrs = new List<string>();
            foreach(var date in listDates)
            {
                listDateStrs.Add(date.ToString("yyyy-MM-dd"));
            }
            parameters.Add("SelectedDates", listDateStrs);

            var userclaim = DataSetupUtility.GetClaimForUser(usr);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            listDates.Sort();
            var docitems = context.FinanceDocumentItem
                .Include(prop => prop.DocumentHeader)
                .Where(p => p.AccountID == acntid);
            var trantype = context.FinTransactionType
                .Where(p => p.HomeID == null || p.HomeID == hid);

            var lstdate = DateTime.MinValue;
            Double amt = 0;
            List<FinanceAccountBalancePerDate> dbbals = new List<FinanceAccountBalancePerDate>();
            foreach (var curdate in listDates)
            {
                amt = 0;
                foreach(var di in docitems)
                {
                    if (di.DocumentHeader.TranDate >= lstdate && di.DocumentHeader.TranDate <= curdate)
                    {
                        var isexp = trantype.First(p => di.TranType == p.ID).Expense;

                        if (isexp)
                            amt -= (double)di.TranAmount;
                        else
                            amt += (double)di.TranAmount;

                    }
                }
                dbbals.Add(new FinanceAccountBalancePerDate
                {
                    BalanceDate = curdate,
                    Balance =(decimal)amt,
                });
                lstdate = curdate;
            }

            var apibals = control.GetAccountBalanceEx(parameters);
            Assert.NotNull(apibals);
            var apibalsval = (apibals as OkObjectResult).Value as List<FinanceAccountBalancePerDate>;
            Assert.NotNull(apibalsval);
            Assert.Equal(dbbals.Count, apibalsval.Count);
            
            //Assert.Equal(0.00, Math.Abs(amt - balval));

            await context.DisposeAsync();
        }
    }
}
