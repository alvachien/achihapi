using System;
using Xunit;
using hihapi.Models;
using System.Threading.Tasks;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class PlanTest
    {
        private SqliteDatabaseFixture fixture = null;

        public PlanTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<PlanTestData> TestData => new TheoryData<PlanTestData>
        {
            new PlanTestData()
            {
                ExpectedValidResult = false,
                TestCaseIndex = 1,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                ExpectedValidResult = false,
                TestCaseIndex = 2,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                ExpectedValidResult = false,
                TestCaseIndex = 3,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                ExpectedValidResult = false,
                TestCaseIndex = 4,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                TargetDate = new DateTime(2021, 1, 1),
                StartDate = new DateTime(2022, 1, 1),
                ExpectedValidResult = false,
                TestCaseIndex = 5,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                ExpectedValidResult = false,
                TestCaseIndex = 6,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.Account,
                ExpectedValidResult = false,
                TestCaseIndex = 7,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.Account,
                AccountID = DataSetupUtility.Home1CashAccount1ID,
                TargetBalance = 10000,
                ExpectedValidResult = true,
                TestCaseIndex = 8,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.AccountCategory,
                TargetBalance = 10000,
                ExpectedValidResult = false,
                TestCaseIndex = 9,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.TranType,
                TargetBalance = 10000,
                ExpectedValidResult = false,
                TestCaseIndex = 10,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.ControlCenter,
                TargetBalance = 10000,
                ExpectedValidResult = false,
                TestCaseIndex = 11,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.AccountCategory,
                AccountCategoryID = FinanceAccountCategory.AccountCategory_Cash,
                TargetBalance = 10000,
                ExpectedValidResult = true,
                TestCaseIndex = 12,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home2ID,
                TranCurr = DataSetupUtility.Home2BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.AccountCategory,
                AccountCategoryID = FinanceAccountCategory.AccountCategory_Cash,
                TargetBalance = 10000,
                ExpectedValidResult = true,
                TestCaseIndex = 13,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.TranType,
                TranTypeID = FinanceTransactionType.TranType_AssetValueIncrease,
                TargetBalance = 10000,
                ExpectedValidResult = true,
                TestCaseIndex = 14,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.ControlCenter,
                ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                TargetBalance = 10000,
                ExpectedValidResult = true,
                TestCaseIndex = 15,
            },
            new PlanTestData()
            {
                HomeID = DataSetupUtility.Home2ID,
                TranCurr = DataSetupUtility.Home2BaseCurrency,
                Description = "Desp",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.ControlCenter,
                ControlCenterID = DataSetupUtility.Home2ControlCenter1ID,
                TargetBalance = 10000,
                ExpectedValidResult = true,
                TestCaseIndex = 16,
            },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestCase_CheckValid(PlanTestData testdata)
        {
            var context = this.fixture.GetCurrentDataContext();

            if (testdata.HomeID == 1)
                fixture.InitHome1TestData(context);
            else if (testdata.HomeID == 2)
                fixture.InitHome2TestData(context);

            FinancePlan plan = new FinancePlan();
            plan.AccountCategoryID = testdata.AccountCategoryID;
            plan.AccountID = testdata.AccountID;
            plan.ControlCenterID = testdata.ControlCenterID;
            plan.Description = testdata.Description;
            plan.HomeID = testdata.HomeID;
            plan.ID = testdata.ID;
            plan.PlanType = testdata.PlanType;
            plan.StartDate = testdata.StartDate;
            plan.TargetBalance = testdata.TargetBalance;
            plan.TargetDate = testdata.TargetDate;
            plan.TranCurr = testdata.TranCurr;
            plan.TranTypeID = testdata.TranTypeID;
            var isValid = plan.IsValid(context);

            Assert.Equal(testdata.ExpectedValidResult, isValid);

            await context.DisposeAsync();
        }
    }
}
