using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Results;
using hihapi.test.common;
using hihapi.Controllers.Library;
using hihapi.Models.Library;

namespace hihapi.unittest.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryPersonsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryPersonsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
