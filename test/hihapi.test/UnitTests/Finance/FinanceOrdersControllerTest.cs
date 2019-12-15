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
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceOrdersControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public FinanceOrdersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1_Home1UserA()
        {
            // 1. Setup control centers
            var cccontrol = new FinanceControlCentersController(this.fixture.CurrentDataContext);
            var user = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            cccontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            await cccontrol.Post(new FinanceControlCenter()
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Control Center 1",
                Comment = "Comment 1",
                Owner = DataSetupUtility.UserA
            });
            await cccontrol.Post(new FinanceControlCenter()
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Control Center 2",
                Comment = "Comment 2",
                Owner = DataSetupUtility.UserB
            });
            var listCCs = cccontrol.Get(DataSetupUtility.Home1ID).ToList<FinanceControlCenter>();

            // 2. Create order
            var control = new FinanceOrdersController(this.fixture.CurrentDataContext);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var ord = new FinanceOrder()
            {
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
            var oid = rst2.Entity.ID;
            Assert.True(oid > 0);

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

            // 5. Delete an order
            var rst5 = await control.Delete(oid);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 6. Read the order again
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Count());

            // Last step, delete control centers
            foreach (var ecc in listCCs)
                await cccontrol.Delete(ecc.ID);
            // this.fixture.CurrentDataContext.Database.ExecuteSqlCommand("DELETE FROM t_fin_controlcenter WHERE ID > 0");
        }
    }
}
