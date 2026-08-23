using System.Threading.Tasks;
using hihapi.Models;
using hihapi.test.common;
using Xunit;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceAccountTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<FinanceAccountTestData> AccountTestData =>
            new TheoryData<FinanceAccountTestData>
            {
                new FinanceAccountTestData()
                {
                    ExpectedValidResult = false,
                    TestCaseIndex = 1,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    ExpectedValidResult = false,
                    TestCaseIndex = 2,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    ExpectedValidResult= false,
                    TestCaseIndex = 3,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    Name = "Test 1",
                    ExpectedValidResult= true,
                    TestCaseIndex = 4,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= true,
                    TestCaseIndex = 5,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_AdvancePayment,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                    TestCaseIndex = 6,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                    TestCaseIndex = 7,
                },
            };

        [Theory]
        [MemberData(nameof(AccountTestData))]
        public async Task TestCase_IsValid(FinanceAccountTestData testData)
        {
            FinanceAccount acnt = new FinanceAccount();
            acnt.HomeID = testData.HomeID;
            acnt.Name = testData.Name;
            acnt.Owner = testData.Owner;
            acnt.Comment = testData.Comment;
            acnt.CategoryID = testData.CategoryID;
            acnt.Status = testData.Status;

            acnt.ExtraAsset = testData.ExtraAsset;
            acnt.ExtraDP = testData.ExtraDP;
            acnt.ExtraLoan = testData.ExtraLoan;

            var context = this.fixture.GetCurrentDataContext();
            var isValid = acnt.IsValid(context);

            Assert.Equal(testData.ExpectedValidResult, isValid);

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceAccountTestData> IsClosedAllowedTestData =>
            new TheoryData<FinanceAccountTestData>
            {
                new FinanceAccountTestData()
                {
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Creditcard,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Deposit,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_VirtualAccount,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = FinanceAccountStatus.Closed,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = FinanceAccountStatus.Frozen,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = FinanceAccountStatus.Normal,
                    ExpectedIsCloseAllowedResult = true,
                },
            };

        [Theory]
        [MemberData(nameof(IsClosedAllowedTestData))]
        public async Task TestCase_IsCloseAllowed(FinanceAccountTestData testData)
        {
            FinanceAccount acnt = new FinanceAccount();
            acnt.HomeID = testData.HomeID;
            acnt.Name = testData.Name;
            acnt.Owner = testData.Owner;
            acnt.Comment = testData.Comment;
            acnt.CategoryID = testData.CategoryID;
            acnt.Status = testData.Status;

            acnt.ExtraAsset = testData.ExtraAsset;
            acnt.ExtraDP = testData.ExtraDP;
            acnt.ExtraLoan = testData.ExtraLoan;

            var context = this.fixture.GetCurrentDataContext();
            var isAllowed = acnt.IsCloseAllowed(context);

            Assert.Equal(testData.ExpectedIsCloseAllowedResult, isAllowed);

            await context.DisposeAsync();
        }
    }

}
