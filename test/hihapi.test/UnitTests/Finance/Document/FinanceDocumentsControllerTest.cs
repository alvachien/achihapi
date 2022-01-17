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
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.test.UnitTests.Finance.Document
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        List<int> listCreatedAccount = new List<int>();
        List<int> listCreatedDocs = new List<int>();

        public FinanceDocumentsControllerTest(SqliteDatabaseFixture fixture)
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
        [InlineData(DataSetupUtility.UserA, DataSetupUtility.Home1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home1ID)]
        [InlineData(DataSetupUtility.UserB, DataSetupUtility.Home2ID)]
        public async Task TestCase_ReportByTranType(string user, int hid)
        {
            var context = this.fixture.GetCurrentDataContext();

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

            await context.DisposeAsync();
        }
    }
}
