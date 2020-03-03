using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceControlCentersControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> ccsCreated = new List<Int32>();

        public FinanceControlCentersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceControlCenter>(provider, "FinanceControlCenters");
        }

        public void Dispose()
        {
            CleanupCreatedEntries();

            if (this.provider != null)
            {
                this.provider.Dispose();
                this.provider = null;
            }
        }


        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB)]
        public async Task TestCase1(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();

            // 0. Create control centers for other homes
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }
            if (hid == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
            }
            if (hid == DataSetupUtility.Home3ID)
            {
                fixture.InitHome3TestData(context);
            }
            if (hid == DataSetupUtility.Home4ID)
            {
                fixture.InitHome4TestData(context);
            }
            if (hid == DataSetupUtility.Home5ID)
            {
                fixture.InitHome5TestData(context);
            }

            var existccamt = (from homemem in context.HomeMembers
                           join fincc in context.FinanceControlCenter
                           on new { homemem.HomeID, homemem.User } equals new { fincc.HomeID, User = user }
                           select fincc.ID).ToList().Count();
            var existccamt_curhome = context.FinanceControlCenter.Where(p => p.HomeID == hid).Count();

            // 1. Create first control center
            var control = new FinanceControlCentersController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var cc = new FinanceControlCenter()
            {
                HomeID = hid,
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
            ccsCreated.Add(firstccid);

            // 2. Now read the whole control centers (without Home ID)
            var queryUrl = @"http://localhost/api/FinanceControlCenters";
            var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceControlCenter>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceControlCenter>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1 + existccamt, rst3.Cast<FinanceControlCenter>().Count());

            // 2a. Now read the whole control centers (with Home ID)            
            queryUrl = @"http://localhost/api/FinanceControlCenters?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceControlCenter>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceControlCenter>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1 + existccamt_curhome, rst3.Cast<FinanceControlCenter>().Count());

            // 3. Now create another one!
            cc = new FinanceControlCenter()
            {
                HomeID = hid,
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
            ccsCreated.Add(sndccid);

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
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1 + existccamt_curhome, rst3.Cast<FinanceControlCenter>().Count());

            // 7. Delete the first control center
            rst5 = await control.Delete(firstccid);
            Assert.NotNull(rst5);
            rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 8. Now read the whole control centers
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existccamt_curhome, rst3.Cast<FinanceControlCenter>().Count());

            ccsCreated.Clear();

            context.Dispose();
        }

        private void CleanupCreatedEntries()
        {
            if (ccsCreated.Count > 0)
            {
                var context = this.fixture.GetCurrentDataContext();
                foreach (var cc in ccsCreated)
                    fixture.DeleteFinanceControlCenter(context, cc);

                ccsCreated.Clear();
                context.SaveChanges();
            }
        }
    }
}
