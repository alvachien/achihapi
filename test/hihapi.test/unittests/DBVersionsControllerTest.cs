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

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class DBVersionsControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public DBVersionsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1()
        {
            var context = this.fixture.GetCurrentDataContext();
            DBVersionsController control = new DBVersionsController(context);

            var version = control.Get();
            Assert.NotEmpty(version);
            var cnt1 = DataSetupUtility.DBVersions.Count();
            var cnt2 = version.Count();
            Assert.Equal(cnt1, cnt2);

            await context.DisposeAsync();
        }
    }
}
