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

namespace hihapi.test
{
    public class DBVersionsControllerTest
    {
        [Fact]
        public async Task Test_Read_Create_ReRead()
        {
        }
    }
}
