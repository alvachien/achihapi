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
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace hihapi.test.UnitTests
{
    [Collection("Collection#1")]
    public class FinanceAssetCategoriesControllerTest
    {
        [Fact]
        public async Task TestCase1()
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

                    // 0. Setup user and home defines
                    DataSetupUtility.InitializeSystemTables(context);
                    DataSetupUtility.InitializeHomeDefineAndMemberTables(context);
                    var ctgyCount = DataSetupUtility.listFinAssetCategory.Count();

                    FinanceAssetCategoriesController control = new FinanceAssetCategoriesController(context);
                    var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserA),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserA),
                    }, "mock"));
                    control.ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = user }
                    };

                    // 1. Read all categories
                    var items = control.Get();
                    var itemcnt = items.Count();
                    Assert.Equal(ctgyCount, itemcnt);

                    // 2. Insert new category
                    var ctgy = new FinanceAssetCategory()
                    {
                        HomeID = DataSetupUtility.Home1ID,
                        Name = "Test 1",
                        Desp = "Test 1"
                    };
                    var rst1 = await control.Post(ctgy);
                    Assert.NotNull(rst1);
                    var rst2 = Assert.IsType<CreatedODataResult<FinanceAssetCategory>>(rst1);
                    Assert.Equal(ctgy.Name, rst2.Entity.Name);
                    Assert.True(rst2.Entity.ID > 0);
                    Assert.Equal(ctgy.Desp, rst2.Entity.Desp);

                    // 3. Read all categories
                    items = control.Get(DataSetupUtility.Home1ID);
                    itemcnt = items.Count();
                    ctgyCount++;
                    Assert.Equal(ctgyCount, itemcnt);
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
