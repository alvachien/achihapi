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

        public FinanceControlCentersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinanceControlCenter>(provider, "FinanceControlCenters");
        }

        public void Dispose()
        {
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
            // 0. Create control centers for other homes
            List<FinanceControlCenter> ccInOtherHomes = new List<FinanceControlCenter>();
            if (hid == DataSetupUtility.Home1ID)
            {
                if (user == DataSetupUtility.UserA || user == DataSetupUtility.UserB)
                {
                    var cc1 = new FinanceControlCenter()
                    {
                        HomeID = DataSetupUtility.Home3ID,
                        Name = "Control Center 3.1",
                        Comment = "Comment 3.1",
                        Owner = user
                    };
                    var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                    ccInOtherHomes.Add(ec1.Entity);
                    this.fixture.CurrentDataContext.SaveChanges();
                }
                else if (user == DataSetupUtility.UserC)
                {
                    var cc1 = new FinanceControlCenter()
                    {
                        HomeID = DataSetupUtility.Home4ID,
                        Name = "Control Center 4.1",
                        Comment = "Comment 4.1",
                        Owner = user
                    };
                    var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                    ccInOtherHomes.Add(ec1.Entity);
                    this.fixture.CurrentDataContext.SaveChanges();
                }
                else if (user == DataSetupUtility.UserD)
                {
                    var cc1 = new FinanceControlCenter()
                    {
                        HomeID = DataSetupUtility.Home5ID,
                        Name = "Control Center 5.1",
                        Comment = "Comment 5.1",
                        Owner = user
                    };
                    var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                    ccInOtherHomes.Add(ec1.Entity);
                    this.fixture.CurrentDataContext.SaveChanges();
                }
            }
            else if (hid == DataSetupUtility.Home2ID)
            {
                var cc1 = new FinanceControlCenter()
                {
                    HomeID = DataSetupUtility.Home3ID,
                    Name = "Control Center 3.1",
                    Comment = "Comment 3.1",
                    Owner = user
                };
                var ec1 = this.fixture.CurrentDataContext.FinanceControlCenter.Add(cc1);
                ccInOtherHomes.Add(ec1.Entity);
                this.fixture.CurrentDataContext.SaveChanges();
            }

            // 1. Create first control center
            var control = new FinanceControlCentersController(this.fixture.CurrentDataContext);
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var context = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = context
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

            // 2. Now read the whole control centers (without Home ID)
            var queryUrl = @"http://localhost/api/FinanceControlCenters";
            var req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceControlCenter>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinanceControlCenter>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1 + ccInOtherHomes.Count(), rst3.Cast<FinanceControlCenter>().Count());

            // 2a. Now read the whole control centers (with Home ID)            
            queryUrl = @"http://localhost/api/FinanceControlCenters?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(context, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinanceControlCenter>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinanceControlCenter>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(1, rst3.Cast<FinanceControlCenter>().Count());

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
            Assert.Equal(1, rst3.Cast<FinanceControlCenter>().Count());

            // 7. Delete the first control center
            rst5 = await control.Delete(firstccid);
            Assert.NotNull(rst5);
            rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);

            // 8. Now read the whole control centers
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(0, rst3.Cast<FinanceControlCenter>().Count());

            // LAST. Remove all pre-created control center
            this.fixture.CurrentDataContext.FinanceControlCenter.RemoveRange(ccInOtherHomes);
            this.fixture.CurrentDataContext.SaveChanges();
        }
    }
}
