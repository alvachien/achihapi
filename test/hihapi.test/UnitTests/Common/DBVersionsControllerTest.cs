using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.unittest.Common
{
    [Collection("HIHAPI_UnitTests#1")]
    public class DBVersionsControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public DBVersionsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task TestCase_GetCurrentVersion()
        {
            var context = this.fixture.GetCurrentDataContext();
            DBVersionsController control = new DBVersionsController(context);

            // 1. Get all version
            var getresult = control.Get();
            Assert.NotNull(getresult);
            var okobjresult = Assert.IsType<OkObjectResult>(getresult);
            var objvalues = Assert.IsAssignableFrom<IQueryable<DBVersion>>(okobjresult.Value);
            // DBVersionsController.CurrentVersion
            var verexist = false;
            foreach(var version in objvalues)
            {
                if (version.VersionID == DBVersionsController.CurrentVersion)
                    verexist = true;
            }
            Assert.True(verexist);

            // 2. Get a single version
            var getsingleresult = control.Get(DBVersionsController.CurrentVersion);
            Assert.NotNull(getsingleresult);

            // 3. Check current version
            var postresult = await control.Post();
            Assert.NotNull(postresult);
            var createdrst = Assert.IsType<CreatedODataResult<CheckVersionResult>>(postresult);
            Assert.NotNull(createdrst);
            Assert.Equal(DBVersionsController.CurrentVersion.ToString(), createdrst.Entity.StorageVersion);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_CommonMethods()
        {
            // Just to increase the code coverage
            var context = this.fixture.GetCurrentDataContext();
            DBVersionsController control = new DBVersionsController(context);

            // 1.
            var rst = control.GetRepeatedDates2("2021-01-01", "2022-01-01", (int)RepeatFrequency.Month);
            Assert.NotNull(rst);

            // 2.
            RepeatDatesCalculationInput input = new RepeatDatesCalculationInput();
            input.StartDate = new DateTime(2021, 1, 1);
            input.EndDate = new DateTime(2022, 1, 1);
            input.RepeatType = RepeatFrequency.Month;
            var rst2 = control.GetRepeatedDates(input);
            Assert.NotNull(rst2);

            // 3.
            RepeatDatesWithAmountCalculationInput input2 = new RepeatDatesWithAmountCalculationInput();
            input2.RepeatType = RepeatFrequency.Month;
            input2.StartDate = new DateTime(2021, 1, 1);
            input2.TotalAmount = 10000;
            input2.EndDate = new DateTime(2022, 1, 1);
            input2.Desp = "Test";            
            var rst3 = control.GetRepeatedDatesWithAmount(input2);
            Assert.NotNull(rst3);

            // 4.
            RepeatDatesWithAmountAndInterestCalInput input3 = new RepeatDatesWithAmountAndInterestCalInput();
            input3.RepaymentMethod = LoanRepaymentMethod.DueRepayment;
            input3.StartDate = new DateTime(2021, 1, 1);
            input3.EndDate = new DateTime(2022, 1, 1);
            input3.TotalMonths = 12;
            input3.InterestFreeLoan = true;
            input3.TotalAmount = 10000;
            var rst4 = control.GetRepeatedDatesWithAmountAndInterest(input3);
            Assert.NotNull(rst4);

            await context.DisposeAsync();
        }
    }
}
