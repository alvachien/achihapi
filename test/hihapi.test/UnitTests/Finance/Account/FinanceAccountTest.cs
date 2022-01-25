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

namespace hihapi.test.UnitTests.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceAccountTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestCase_CheckValid()
        {
            FinanceAccount acnt = new FinanceAccount();
            var context = this.fixture.GetCurrentDataContext();
            var isValid = acnt.IsValid(context);

            Assert.False(isValid);
        }
    }
}
