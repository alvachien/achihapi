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
    [Collection("Collection#1")]
    public class FinanceOrdersControllerTest
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

                    // 1. Setup control centers
                    context.FinanceControlCenter.Add(new FinanceControlCenter() {
                        HomeID = DataSetupUtility.Home1ID,
                        Name = "Control Center 1",
                        Comment = "Comment 1",
                        Owner = DataSetupUtility.UserA
                    });
                    context.FinanceControlCenter.Add(new FinanceControlCenter() {
                        HomeID = DataSetupUtility.Home1ID,
                        Name = "Control Center 2",
                        Comment = "Comment 2",
                        Owner = DataSetupUtility.UserB
                    });
                    context.SaveChanges();
                    var listCCs = context.FinanceControlCenter.ToList<FinanceControlCenter>();

                    // 2. Create order
                    var control = new FinanceOrdersController(context);
                    var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, DataSetupUtility.UserA),
                        new Claim(ClaimTypes.NameIdentifier, DataSetupUtility.UserA),
                    }, "mock"));
                    control.ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = user }
                    };
                    var ord = new FinanceOrder() {
                        HomeID = DataSetupUtility.Home1ID,
                        Name = "Order 1",
                        Comment = "Comment 1"
                    };
                    var srule = new FinanceOrderSRule()
                    {
                        Order = ord,
                        RuleID = 1,
                        ControlCenterID = listCCs[0].ID,
                        Precent = 100
                    };
                    ord.SRule.Add(srule);
                    var rst = await control.Post(ord);
                    Assert.NotNull(rst);
                    var rst2 = Assert.IsType<CreatedODataResult<FinanceOrder>>(rst);
                    Assert.Equal(rst2.Entity.Name, ord.Name);

                    // 3. Read the order out
                    var rst3 = control.Get(DataSetupUtility.Home1ID);
                    Assert.NotNull(rst3);
                    Assert.Equal(1, rst3.Count());

                    // 4. Change one order
                    var norder = rst2.Entity;
                    norder.Name = "New Order";
                    rst = await control.Put(norder.ID, norder);
                    Assert.NotNull(rst);
                    var rst4 = Assert.IsType<UpdatedODataResult<FinanceOrder>>(rst);
                    Assert.Equal(norder.Name, rst4.Entity.Name);
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
