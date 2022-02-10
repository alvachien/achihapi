﻿using System;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using hihapi.Controllers.Finance;
using Microsoft.AspNetCore.OData.Formatter;
using hihapi.test.common;

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
            Assert.NotNull(user);
            var context = this.fixture.GetCurrentDataContext();

            FinanceReportsController control = new FinanceReportsController(context);

            ODataActionParameters parameters = new ODataActionParameters();
            parameters.Add("HomeID", hid);
            parameters.Add("Year", year);
            parameters.Add("Month", null);

            // 1. No authorization
            try
            {
                control.GetReportByTranType(parameters);
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }

            await context.DisposeAsync();
        }
    }
}

