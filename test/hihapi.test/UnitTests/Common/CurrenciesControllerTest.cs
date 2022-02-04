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

namespace hihapi.test.UnitTests.Common
{
    [Collection("HIHAPI_UnitTests#1")]
    public class CurrenciesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public CurrenciesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData("CNY")]
        [InlineData("USD")]
        [InlineData("EUR")]
        public async Task TestCase_ReadData(string strcurr)
        {
            var context = fixture.GetCurrentDataContext();
            var control = new CurrenciesController(context);

            // 1. Get all currencies
            var getresult = control.Get();
            Assert.NotNull(getresult);
            var okgetresult = Assert.IsType<OkObjectResult>(getresult);
            var objvalues = Assert.IsAssignableFrom<IQueryable<Currency>>(okgetresult.Value);

            var currexist = false;
            foreach(var item in objvalues)
            {
                if (item.Curr == strcurr)
                    currexist = true;
            }
            Assert.True(currexist);

            // 2. Get single currency
            var getsingleresult = control.Get(strcurr);
            Assert.NotNull(getsingleresult);
            Assert.IsType<OkObjectResult>(getsingleresult);

            await context.DisposeAsync();
        }
    }
}
