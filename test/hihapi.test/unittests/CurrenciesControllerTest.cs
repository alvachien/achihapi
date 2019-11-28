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
    public class CurrenciesControllerTest
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

                    CurrenciesController control = new CurrenciesController(context);
                    
                    // Step 1. Read - nothing
                    var rsts = control.Get();
                    var rstscnt = await rsts.CountAsync();
                    Assert.Equal(0, rstscnt);

                    // // Step 2. Create a new one
                    // var nmod = new Knowledge() {
                    //     Title = "Test 1",
                    //     Category = KnowledgeCategory.Concept,
                    //     Content = "My test 1"
                    // };
                    // var result = control.Post(nmod);
                    // var actionResult = Assert.IsType<Task<IActionResult>>(result);
                    // var actResult = Assert.IsType<CreatedODataResult<Knowledge>>(result.Result);
                    // rstscnt = await context.Knowledges.CountAsync();
                    // Assert.Equal(1, rstscnt);
                    
                    // var nid = actResult.Entity.ID;
                    // var dbrst = await context.Knowledges.SingleOrDefaultAsync(p => p.ID == nid);
                    // Assert.Equal(dbrst.Title, nmod.Title);
                    // Assert.Equal(dbrst.Content, nmod.Content);
                    // Assert.Equal(dbrst.Category, nmod.Category);

                    // // Step 3. Re-read
                    // rsts = control.Get();
                    // rstscnt = await rsts.CountAsync();
                    // Assert.Equal(1, rstscnt);
                    // var firstrst = rsts.ToList()[0];
                    // Assert.Equal(firstrst.Title, nmod.Title);
                    // Assert.Equal(firstrst.Content, nmod.Content);
                    // Assert.Equal(firstrst.Category, nmod.Category);
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
