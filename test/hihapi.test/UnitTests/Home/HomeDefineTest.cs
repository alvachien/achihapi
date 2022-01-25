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

namespace hihapi.test.UnitTests.Home
{
    [Collection("HIHAPI_UnitTests#1")]
    public class HomeDefineTest
    {
        private SqliteDatabaseFixture fixture = null;

        public HomeDefineTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestCase_CheckValid()
        {
            var home = new HomeDefine();
            var context = this.fixture.GetCurrentDataContext();
            var isvalid = home.IsValid(context);

            Assert.False(isvalid);
        }
    }
}
