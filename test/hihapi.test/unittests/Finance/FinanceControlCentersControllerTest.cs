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
    public class FinanceControlCentersControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public FinanceControlCentersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1_Home1_UserA()
        {
            // 1. Create first control center
            var control = new FinanceControlCentersController(this.fixture.CurrentDataContext);
            var user = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            var cc = new FinanceControlCenter()
            {
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
            var firstccid = rst2.Entity.ID;
            Assert.True(firstccid > 0);

            // 2. Now read the whole control centers
            var rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Count());

            // 3. Now create another one!
            cc = new FinanceControlCenter()
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Control Center 2",
                Comment = "Comment 2",
                ParentID = rst2.Entity.ID,
                Owner = DataSetupUtility.UserA
            };
            rst = await control.Post(cc);
            Assert.NotNull(rst);
            rst2 = Assert.IsType<CreatedODataResult<FinanceControlCenter>>(rst);
            Assert.Equal(rst2.Entity.Name, cc.Name);
            Assert.Equal(rst2.Entity.HomeID, cc.HomeID);
            Assert.Equal(rst2.Entity.Owner, cc.Owner);
            var sndccid = rst2.Entity.ID;
            Assert.True(sndccid > 0);

            // 4. Change one control center
            cc.Owner = DataSetupUtility.UserB;
            rst = await control.Put(sndccid, cc);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinanceControlCenter>>(rst);
            Assert.Equal(rst4.Entity.Name, cc.Name);
            Assert.Equal(rst4.Entity.HomeID, cc.HomeID);
            Assert.Equal(rst4.Entity.Owner, DataSetupUtility.UserB);

            // 5. Delete the second control center
            var rst5 = await control.Delete(sndccid);
            Assert.NotNull(rst5);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 6. Now read the whole control centers
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Count());

            // 7. Delete the first control center
            rst5 = await control.Delete(firstccid);
            Assert.NotNull(rst5);
            rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 8. Now read the whole control centers
            rst3 = control.Get(DataSetupUtility.Home1ID);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Count());
        }
    }
}
