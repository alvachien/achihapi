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
using Moq;
using System.Security.Claims;
using System.Collections.Generic;

namespace hihapi.test.UnitTests
{
    public class FinanceControlCentersControllerTest
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

                    // 1. Create control center
                    var control = new FinanceControlCentersController(context);
                    var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserA),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserA),
                    }, "mock"));
                    control.ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = user }
                    };
                    var cc = new FinanceControlCenter() {
                        HomeID = DataSetupUtility.Home1ID,
                        Name = "Control Center 1",
                        Comment = "Comment 1",
                        Owner = DataSetupUtility.UserA
                    };
                    var rst = await control.Post(cc);
                    Assert.NotNull(rst);
                    var rst2 = Assert.IsType<CreatedODataResult<FinanceControlCenter>>(rst);
                    Assert.Equal(rst2.Entity.Name, cc.Name);
                    Assert.Equal(rst2.Entity.HomeID, cc.HomeID);
                    Assert.Equal(rst2.Entity.Owner, cc.Owner);

                    // 2. Now read the whole control centers
                    
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
