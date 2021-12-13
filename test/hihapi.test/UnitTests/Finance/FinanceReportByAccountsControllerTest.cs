using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNet.OData.Results;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceReportByAccountsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> plansCreated = new List<Int32>();

        public FinanceReportByAccountsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;

            this.provider = UnitTestUtility.GetServiceProvider();
            this.model = UnitTestUtility.GetEdmModel<FinancePlan>(provider, "FinancePlans");
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

        private void CleanupCreatedEntries()
        {
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB)]
        public void TestController1(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
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

            // 1. Prepare dta
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);

            // 2. Test it
            var controller = new FinanceReportByAccountsController(context);
            Assert.NotNull(controller);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var rsts = controller.Get();
            var curhmemquery = (from homemem in context.HomeMembers where homemem.HomeID == hid && homemem.User == user select homemem).FirstOrDefault();
            var curhmem = Assert.IsType<HomeMember>(curhmemquery);
            var expamt = context.FinanceAccount.Where(p => p.HomeID == hid).Count();
            var actamt = rsts.ToList().Where(p => p.HomeID == hid).Count();
            if (curhmem.IsChild.HasValue && curhmem.IsChild == true)
            {
                expamt = context.FinanceAccount.Where(p => p.HomeID == hid && p.Owner == user).Count();
                Assert.Equal(expamt, actamt);
            }
            else
            {
                Assert.Equal(expamt, actamt);
            }
        }
    }
}
