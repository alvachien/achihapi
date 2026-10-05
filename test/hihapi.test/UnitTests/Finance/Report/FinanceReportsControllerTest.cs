using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hihapi.Controllers;
using hihapi.Exceptions;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Xunit;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceReportsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        List<int> listCreatedAccount = new List<int>();
        List<int> listCreatedDocs = new List<int>();

        public FinanceReportsControllerTest(SqliteDatabaseFixture fixture)
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

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, 2022)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, 2022)]
        public async Task TestCase_ReportByTranType(string user, int hid, int year)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("Year", year);
            parameters.Add("Month", null);

            // 1. No authorization
            try
            {
                await control.GetReportByTranType(parameters);
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
            var rst = await control.GetReportByTranType(parameters);
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_ReportByTranTypeWithInvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.GetReportByTranType(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_ReportByAccountWithInvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.GetReportByAccount(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByControlCenterWithInvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.GetReportByControlCenter(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByOrderWithInvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.GetReportByOrder(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByTranTypeMOM_InvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("HomeID", "The HomeIDfield is required.");
            try
            {
                await control.GetReportByTranTypeMOM(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.TranType_Expense1, "1", null)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, DataSetupUtility.TranType_Expense1, "1", false)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, DataSetupUtility.TranType_Expense2, "2", false)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, DataSetupUtility.TranType_Expense2, "3", false)]
        public async Task TestCase_ReportByTranTypeMOM(string user, int hid, int ttid,
            string period, Boolean? child)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("TransactionType", ttid);
            parameters.Add("Period", period);
            if (child != null)
                parameters.Add("IncludeChildren", child.Value);

            // 1. No authorization
            try
            {
                await control.GetReportByTranTypeMOM(parameters);
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
            var rst = await control.GetReportByTranTypeMOM(parameters);
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByAccount_InvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("HomeID", "The HomeIDfield is required.");
            try
            {
                await control.GetReportByAccount(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID)]
        public async Task TestCase_GetReportByAccount(String user, int hid)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);

            // 1. No authorization
            try
            {
                await control.GetReportByAccount(parameters);
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
            var rst = await control.GetReportByAccount(parameters);
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByAccountMOM_InvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("HomeID", "The HomeIDfield is required.");
            try
            {
                await control.GetReportByAccountMOM(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID)]
        public async Task TestCase_GetReportByControlCenter(String user, int hid)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);

            // 1. No authorization
            try
            {
                await control.GetReportByControlCenter(parameters);
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
            var rst = await control.GetReportByControlCenter(parameters);
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.Home1CashAccount1ID, "1")]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.Home1CashAccount1ID, "2")]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, DataSetupUtility.Home1CashAccount3ID, "3")]
        public async Task TestCase_ReportByAccountMOM(string user, int hid, int acntid, string period)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("AccountID", acntid);
            parameters.Add("Period", period);

            // 1. No authorization
            try
            {
                await control.GetReportByAccountMOM(parameters);
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
            var rst = await control.GetReportByAccountMOM(parameters);
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByControlCenter_InvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("HomeID", "The HomeIDfield is required.");
            try
            {
                await control.GetReportByControlCenter(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GetReportByControlCenterMOM_InvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();
            FinanceReportsController control = new FinanceReportsController(context);
            control.ModelState.AddModelError("HomeID", "The HomeIDfield is required.");
            try
            {
                await control.GetReportByControlCenterMOM(new ODataActionParameters());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.Home1ControlCenter1ID, "1", true)]
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID, DataSetupUtility.Home1ControlCenter1ID, "2", true)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID, DataSetupUtility.Home1ControlCenter2ID, "3", null)]
        public async Task TestCase_ReportByControlCenterMOM(string user, int hid, int ccid,
            string period, Boolean? child)
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(hid, context);

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("ControlCenterID", ccid);
            parameters.Add("Period", period);
            if (child != null)
                parameters.Add("IncludeChildren", child.Value);

            // 1. No authorization
            try
            {
                await control.GetReportByControlCenterMOM(parameters);
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
            var rst = await control.GetReportByControlCenterMOM(parameters);
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }
    }
}

