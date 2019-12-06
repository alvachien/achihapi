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
    [Collection("Collection#1")]
    public class DBVersionsControllerTest
    {
        [Fact]
        public async Task Test_Read_Create_ReRead()
        {
            hihDataContext.TestingMode = true;

            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<hihDataContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new hihDataContext(options))
                {
                    context.Database.EnsureCreated();
                    DataSetupUtility.InitialTable_DBVersion(context);
                    await context.SaveChangesAsync();

                    DBVersionsController control = new DBVersionsController(context);
                    var version = control.Get();
                    Assert.NotEmpty(version);
                    var cnt1 = DataSetupUtility.listDBVersion.Count();
                    var cnt2 = version.Count();
                    Assert.Equal(cnt1, cnt2);
                }
            }
            finally
            {
                connection.Close();
            }

            //hihDataContext.TestingMode = false;
        }
    }
}
