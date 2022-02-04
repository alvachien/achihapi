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
    public class LanguagesControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public LanguagesControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(4)] // Zh-hans
        [InlineData(9)] // English
        [InlineData(17)] // Japanese
        public async Task TestCase_ReadData(int nlang)
        {
            var context = fixture.GetCurrentDataContext();
            var control = new LanguagesController(context);

            // 1. Get all languages
            var getresult = control.Get();
            Assert.NotNull(getresult);
            var okgetresult = Assert.IsType<OkObjectResult>(getresult);
            var objvalues = Assert.IsAssignableFrom<IQueryable<Language>>(okgetresult.Value);

            var langexist = true;
            foreach (var item in objvalues)
            {
                if (item.Lcid == nlang)
                    langexist = true;
            }
            Assert.True(langexist);

            // 2. Get single entry
            var getsingleresult = control.Get(nlang);
            Assert.NotNull(getsingleresult);
            Assert.IsType<Language>(getsingleresult);

            await context.DisposeAsync();
        }

    }
}
