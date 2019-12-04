using System;
using Xunit;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;

namespace hihapi.test.UnitTests
{
    public class FinanceAccountCategoriesControllerTest
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

                    // Add system
                    context.FinAccountCategories.Add(new FinanceAccountCategory() {
                        HomeID = null,
                        Name = "Test 1",
                        AssetFlag = true,
                        Comment = "Test 1"
                    });
                    // Add Home defined
                    context.FinAccountCategories.Add(new FinanceAccountCategory() {
                        HomeID = 1,
                        Name = "HID 1.Test 1",
                        AssetFlag = true,
                        Comment = "Test 1"
                    });
                    await context.SaveChangesAsync();

                    FinanceAccountCategoriesController control = new FinanceAccountCategoriesController(context);
                    // For non-home case
                    var items = control.Get();
                    Assert.Single(items);
                    var firstitem = await items.FirstOrDefaultAsync<FinanceAccountCategory>();
                    Assert.Equal(1, firstitem.ID);
                    Assert.Null(firstitem.HomeID);
                }
            }
            finally
            {
                connection.Close();
            }

            hihDataContext.TestingMode = false;
        }
    }
}
