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
    public class FinancePlansControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private ServiceProvider provider = null;
        private IEdmModel model = null;
        private List<Int32> plansCreated = new List<Int32>();

        public FinancePlansControllerTest(SqliteDatabaseFixture fixture)
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
            if (plansCreated.Count > 0)
            {
                var context = this.fixture.GetCurrentDataContext();
                foreach (var ord in plansCreated)
                    fixture.DeletePlan(context, ord);

                plansCreated.Clear();
                context.SaveChanges();
            }
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, FinancePlanTypeEnum.Account)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, FinancePlanTypeEnum.AccountCategory)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, FinancePlanTypeEnum.ControlCenter)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.Home1BaseCurrency, FinancePlanTypeEnum.TranType)]
        public void TestModel(int hid, string curr, FinancePlanTypeEnum pty)
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

            var plan = new FinancePlan();
            Assert.False(plan.IsValid(context));
            plan.HomeID = hid;
            Assert.False(plan.IsValid(context));
            plan.TranCurr = curr;
            Assert.False(plan.IsValid(context));
            plan.Description = "test";
            Assert.False(plan.IsValid(context));
            plan.StartDate = DateTime.Today;
            plan.TargetDate = DateTime.Today;
            Assert.False(plan.IsValid(context));
            plan.TargetDate = DateTime.Today.AddDays(100);
            Assert.False(plan.IsValid(context));
            plan.PlanType = pty;
            switch(pty)
            {
                case FinancePlanTypeEnum.Account:
                    plan.AccountID = context.FinanceAccount.Where(p => p.HomeID == hid).FirstOrDefault().ID;
                    Assert.True(plan.IsValid(context));
                    break;

                case FinancePlanTypeEnum.AccountCategory:
                    plan.AccountCategoryID = context.FinAccountCategories.FirstOrDefault().ID;
                    Assert.True(plan.IsValid(context));
                    break;

                case FinancePlanTypeEnum.ControlCenter:
                    plan.ControlCenterID = context.FinanceControlCenter.Where(p => p.HomeID == hid).FirstOrDefault().ID;
                    Assert.True(plan.IsValid(context));
                    break;

                case FinancePlanTypeEnum.TranType:
                    plan.TranTypeID = context.FinTransactionType.FirstOrDefault().ID;
                    Assert.True(plan.IsValid(context));
                    break;

                default:
                    break;
            }
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserB)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserC)]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserD)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB)]
        public async Task TestController1(int hid, string user)
        {
            var context = this.fixture.GetCurrentDataContext();
            var curr = "";
            if (hid == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
                curr = DataSetupUtility.Home1BaseCurrency;
            }
            if (hid == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
                curr = DataSetupUtility.Home2BaseCurrency;
            }
            if (hid == DataSetupUtility.Home3ID)
            {
                fixture.InitHome3TestData(context);
                curr = DataSetupUtility.Home3BaseCurrency;
            }
            if (hid == DataSetupUtility.Home4ID)
            {
                fixture.InitHome4TestData(context);
                curr = DataSetupUtility.Home4BaseCurrency;
            }
            if (hid == DataSetupUtility.Home5ID)
            {
                fixture.InitHome5TestData(context);
                curr = DataSetupUtility.Home5BaseCurrency;
            }

            // 1. Prepare dta
            var userclaim = DataSetupUtility.GetClaimForUser(user);
            var httpctx = UnitTestUtility.GetDefaultHttpContext(provider, userclaim);

            var existamt = (from homemem in context.HomeMembers
                            join finplan in context.FinancePlan
                            on new { homemem.HomeID, homemem.User } equals new { finplan.HomeID, User = user }
                            select finplan.ID).ToList().Count();
            var existamt_curhome = context.FinancePlan.Where(p => p.HomeID == hid).Count();

            // 2. Create plan
            var control = new FinancePlansController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = httpctx
            };
            var plan = new FinancePlan()
            {
                HomeID = hid,
                PlanType = FinancePlanTypeEnum.Account,
                TranCurr = curr,
                AccountID = context.FinanceAccount.Where(p => p.HomeID == hid).FirstOrDefault().ID,
                StartDate = DateTime.Today,
                TargetBalance = 10000,
                TargetDate = DateTime.Today.AddDays(100),
                Description = "Test",
            };
            var rst = await control.Post(plan);
            Assert.NotNull(rst);
            var rst2 = Assert.IsType<CreatedODataResult<FinancePlan>>(rst);
            Assert.Equal(rst2.Entity.TranCurr, plan.TranCurr);
            Assert.Equal(rst2.Entity.TargetBalance, plan.TargetBalance);
            var oid = rst2.Entity.ID;
            Assert.True(oid > 0);
            plansCreated.Add(oid);

            // 3. Read the plan out (without Home ID)
            var queryUrl = "http://localhost/api/FinancePlans";
            var req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            var odatacontext = UnitTestUtility.GetODataQueryContext<FinancePlan>(this.model);
            var options = UnitTestUtility.GetODataQueryOptions<FinancePlan>(odatacontext, req);
            var rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt + 1, rst3.Cast<FinancePlan>().Count());

            // 3a. Read the plan out (with Home ID)
            queryUrl = "http://localhost/api/FinancePlans?$filter=HomeID eq " + hid.ToString();
            req = UnitTestUtility.GetHttpRequest(httpctx, "GET", queryUrl);
            //var odatacontext = UnitTestUtility.GetODataQueryContext<FinancePlan>(this.model);
            options = UnitTestUtility.GetODataQueryOptions<FinancePlan>(odatacontext, req);
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt_curhome + 1, rst3.Cast<FinancePlan>().Count());

            // 4. Change one plan
            var nplan = rst2.Entity;
            nplan.Description = "Test > 2";
            rst = await control.Put(nplan.ID, nplan);
            Assert.NotNull(rst);
            var rst4 = Assert.IsType<UpdatedODataResult<FinancePlan>>(rst);
            Assert.Equal(nplan.Description, rst4.Entity.Description);

            // 5. Delete a plan
            var rst5 = await control.Delete(oid);
            Assert.NotNull(rst4);
            var rst6 = Assert.IsType<StatusCodeResult>(rst5);
            Assert.Equal(204, rst6.StatusCode);
            plansCreated.Clear();

            // 6. Read the plan again
            rst3 = control.Get(options);
            Assert.NotNull(rst3);
            Assert.Equal(existamt_curhome, rst3.Cast<FinancePlan>().Count());

            await context.DisposeAsync();
        }
    }
}
