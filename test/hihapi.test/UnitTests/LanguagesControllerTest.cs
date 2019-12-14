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
using Moq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNet.OData.Query;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LanguagesControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public LanguagesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1()
        {
            LanguagesController control = new LanguagesController(this.fixture.CurrentDataContext);

            // Step 1. Read all
            //ODataQueryOption opt = new ODataQueryOptions();
            var rsts = control.Get();
            var rstscnt = await rsts.CountAsync();
            var cnt1 = DataSetupUtility.Languages.Count;
            Assert.Equal(cnt1, rstscnt);
        }
    }
}

