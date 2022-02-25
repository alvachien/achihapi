using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;
using hihapi.test.common;
using Microsoft.AspNetCore.OData.Deltas;
using hihapi.Exceptions;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinancePlansControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public FinancePlansControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
            if (this.listCreatedID.Count > 0)
            {
                this.listCreatedID.ForEach(x => this.fixture.DeleteFinancePlan(this.fixture.GetCurrentDataContext(), x));

                this.listCreatedID.Clear();
            }
            this.fixture.GetCurrentDataContext().SaveChanges();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, DataSetupUtility.UserA, DataSetupUtility.Home1CashAccount1ID)]
        [InlineData(DataSetupUtility.Home2ID, DataSetupUtility.UserB, DataSetupUtility.Home2CashAccount1ID)]
        public async Task TestCase_CURD(int hid, string usr, int accountid)
        {
            var context = fixture.GetCurrentDataContext();
            // Pre. setup
            this.fixture.InitHomeTestData(hid, context);

            var control = new FinancePlansController(context);

            // Step 1. Get all
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(usr);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };
            var getallrst = control.Get();
            Assert.NotNull(getallrst);

            // Step 2. Create
            var testdata = new FinancePlan
            {
                HomeID = hid,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.AccountCategory,
                AccountCategoryID = FinanceAccountCategory.AccountCategory_Cash,
                TargetBalance = 10000,
            };
            var postrst = await control.Post(testdata);
            Assert.NotNull(postrst);
            var postcreatedrst = Assert.IsType<CreatedODataResult<FinancePlan>>(postrst);
            var createdid = postcreatedrst.Entity.ID;
            Assert.Equal(testdata.HomeID, postcreatedrst.Entity.HomeID);
            Assert.Equal(testdata.TranCurr, postcreatedrst.Entity.TranCurr);
            Assert.Equal(testdata.Description, postcreatedrst.Entity.Description);
            Assert.Equal(testdata.StartDate, postcreatedrst.Entity.StartDate);
            Assert.Equal(testdata.TargetDate, postcreatedrst.Entity.TargetDate);
            Assert.Equal(testdata.PlanType, postcreatedrst.Entity.PlanType);
            Assert.Equal(testdata.AccountCategoryID, postcreatedrst.Entity.AccountCategoryID);
            Assert.Equal(testdata.TargetBalance, postcreatedrst.Entity.TargetBalance);
            Assert.Equal(testdata.AccountID, postcreatedrst.Entity.AccountID);
            listCreatedID.Add(createdid);

            // Step 3. Read single
            var getrst = control.Get(createdid);
            Assert.NotNull(getrst);
            var getobj = Assert.IsType<SingleResult<FinancePlan>>(getrst);
            Assert.NotNull(getobj);
            var getobjlist = getobj.Queryable.ToList();
            Assert.Single(getobjlist);
            var readplan = getobjlist[0];
            Assert.Equal(testdata.HomeID, readplan.HomeID);
            Assert.Equal(testdata.TranCurr, readplan.TranCurr);
            Assert.Equal(testdata.Description, readplan.Description);
            Assert.Equal(testdata.StartDate, readplan.StartDate);
            Assert.Equal(testdata.TargetDate, readplan.TargetDate);
            Assert.Equal(testdata.PlanType, readplan.PlanType);
            Assert.Equal(testdata.AccountCategoryID, readplan.AccountCategoryID);
            Assert.Equal(testdata.TargetBalance, readplan.TargetBalance);
            Assert.Equal(testdata.AccountID, readplan.AccountID);
            Assert.Equal(createdid, readplan.ID);

            // Step 4. Change it via Put
            readplan.Description += "Test";
            readplan.PlanType = FinancePlanTypeEnum.Account;
            readplan.AccountCategoryID = null;
            readplan.AccountID = accountid;
            var putrst = await control.Put(createdid, readplan);
            Assert.NotNull(putrst);
            var changedrst = Assert.IsType<UpdatedODataResult<FinancePlan>>(putrst);
            Assert.NotNull(changedrst);
            Assert.Equal(readplan.Description, changedrst.Entity.Description);
            Assert.Equal(readplan.PlanType, changedrst.Entity.PlanType);
            Assert.Equal(readplan.AccountCategoryID, changedrst.Entity.AccountCategoryID);
            Assert.Equal(readplan.AccountID, changedrst.Entity.AccountID);

            // Step 5. Delete it
            var delrst = await control.Delete(createdid);
            Assert.NotNull(delrst);
            Assert.IsType<StatusCodeResult>(delrst);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_GettWithInvalidUser()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);

            try
            {
                control.Get(999);
            }
            catch (Exception ex)
            {
                Assert.IsType<UnauthorizedAccessException>(ex);
            }

            await context.DisposeAsync();
        }


        [Fact]
        public async Task TestCase_PostWithInvalidModelState()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);
            control.ModelState.AddModelError("StartDate", "The StartDate field is required.");

            try
            {
                await control.Post(new FinancePlan());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostWithInvalidUser()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);

            try
            {
                await control.Post(new FinancePlan());
            }
            catch (Exception ex)
            {
                Assert.IsType<UnauthorizedAccessException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostWithInvalidInput()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);
            var userclaim = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };

            try
            {
                await control.Post(new FinancePlan());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PutWithInvalidModelState()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);
            control.ModelState.AddModelError("StartDate", "The StartDate field is required.");

            try
            {
                await control.Put(999, new FinancePlan());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PutWithMismatchID()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);

            try
            {
                await control.Put(999, new FinancePlan() { ID = 1, });
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PutWithInvalidUser()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);

            try
            {
                await control.Put(999, new FinancePlan() { ID = 999, });
            }
            catch (Exception ex)
            {
                Assert.IsType<UnauthorizedAccessException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_DeleteWithInvalidID()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinancePlansController(context);

            try
            {
                await control.Delete(999);
            }
            catch (Exception ex)
            {
                Assert.IsType<NotFoundException>(ex);
            }

            await context.DisposeAsync();
        }
    }
}
