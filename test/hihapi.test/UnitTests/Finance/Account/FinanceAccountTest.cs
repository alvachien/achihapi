using System;
using Xunit;
using hihapi.Models;
using System.Threading.Tasks;
using hihapi.test.common;

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
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    ExpectedValidResult = false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    ExpectedValidResult= false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    Name = "Test 1",
                    ExpectedValidResult= true,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= true,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_AdvancePayment,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
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
                    Status = (byte)FinanceAccountStatus.Closed,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = (byte)FinanceAccountStatus.Frozen,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = (byte)FinanceAccountStatus.Normal,
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

    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountExtraDPTest
    {
        private SqliteDatabaseFixture fixture = null;
        public FinanceAccountExtraDPTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestCase_UpdateComment()
        {
            var olddata = new FinanceAccountExtraDP();
            olddata.AccountID = 1;
            olddata.Comment = "Test 1";
            olddata.StartDate = DateTime.Today;
            olddata.EndDate = DateTime.Today.AddMonths(3);
            olddata.RepeatType = RepeatFrequency.Month;
            
            var newdata = new FinanceAccountExtraDP();
            newdata.AccountID = 1;
            newdata.Comment = "Test 2";
            newdata.StartDate = DateTime.Today;
            newdata.EndDate = DateTime.Today.AddMonths(3);
            newdata.RepeatType = RepeatFrequency.Month;

            var updatesql = FinanceAccountExtraDP.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_dp] SET [COMMENT] = N'{newdata.Comment}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateEndDate()
        {
            var olddata = new FinanceAccountExtraDP();
            olddata.AccountID = 1;
            olddata.Comment = "Test 1";
            olddata.StartDate = DateTime.Today;
            olddata.EndDate = DateTime.Today.AddMonths(3);
            olddata.RepeatType = RepeatFrequency.Month;

            var newdata = new FinanceAccountExtraDP();
            newdata.AccountID = 1;
            newdata.Comment = "Test 1";
            newdata.StartDate = DateTime.Today;
            newdata.EndDate = DateTime.Today.AddMonths(2);
            newdata.RepeatType = RepeatFrequency.Month;

            var updatesql = FinanceAccountExtraDP.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_dp] SET [ENDDATE] = '{newdata.EndDate.ToString("yyyy-MM-dd")}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateEndDateAndComment()
        {
            var olddata = new FinanceAccountExtraDP();
            olddata.AccountID = 1;
            olddata.Comment = "Test 1";
            olddata.StartDate = DateTime.Today;
            olddata.EndDate = DateTime.Today.AddMonths(3);
            olddata.RepeatType = RepeatFrequency.Month;

            var newdata = new FinanceAccountExtraDP();
            newdata.AccountID = 1;
            newdata.Comment = "Test 2";
            newdata.StartDate = DateTime.Today;
            newdata.EndDate = DateTime.Today.AddMonths(2);
            newdata.RepeatType = RepeatFrequency.Month;

            var updatesql = FinanceAccountExtraDP.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_dp] SET [COMMENT] = N'{newdata.Comment}',[ENDDATE] = '{newdata.EndDate.ToString("yyyy-MM-dd")}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateRepeatType()
        {
            var olddata = new FinanceAccountExtraDP();
            olddata.AccountID = 1;
            olddata.Comment = "Test 1";
            olddata.StartDate = DateTime.Today;
            olddata.EndDate = DateTime.Today.AddMonths(3);
            olddata.RepeatType = RepeatFrequency.Month;

            var newdata = new FinanceAccountExtraDP();
            newdata.AccountID = 1;
            newdata.Comment = "Test 1";
            newdata.StartDate = DateTime.Today;
            newdata.EndDate = DateTime.Today.AddMonths(3);
            newdata.RepeatType = RepeatFrequency.Fortnight;

            var updatesql = FinanceAccountExtraDP.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_dp] SET [RepeatType] = {((int)newdata.RepeatType).ToString()} WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }
    }

    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountExtraASTest
    {
        private SqliteDatabaseFixture fixture = null;
        public FinanceAccountExtraASTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestCase_UpdateComment()
        {
            var olddata = new FinanceAccountExtraAS();
            olddata.Comment = "Test 1";
            olddata.CategoryID = 1;
            olddata.AccountID = 1;
            olddata.Name = "Test 1";
            olddata.RefenceBuyDocumentID = 1;
            var newdata = new FinanceAccountExtraAS();
            newdata.Comment = "Test 2";
            newdata.CategoryID = 1;
            newdata.AccountID = 1;
            newdata.RefenceBuyDocumentID = 1;
            newdata.Name = "Test 1";

            var updatesql = FinanceAccountExtraAS.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_as] SET [COMMENT] = N'{newdata.Comment}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateName()
        {
            var olddata = new FinanceAccountExtraAS();
            olddata.Comment = "Test 1";
            olddata.CategoryID = 1;
            olddata.AccountID = 1;
            olddata.Name = "Test 1";
            olddata.RefenceBuyDocumentID = 1;
            var newdata = new FinanceAccountExtraAS();
            newdata.Comment = "Test 1";
            newdata.CategoryID = 1;
            newdata.AccountID = 1;
            newdata.RefenceBuyDocumentID = 1;
            newdata.Name = "Test 2";

            var updatesql = FinanceAccountExtraAS.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_as] SET [NAME] = N'{newdata.Name}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateNameAndComment()
        {
            var olddata = new FinanceAccountExtraAS();
            olddata.Comment = "Test 1";
            olddata.CategoryID = 1;
            olddata.AccountID = 1;
            olddata.Name = "Test 1";
            olddata.RefenceBuyDocumentID = 1;
            var newdata = new FinanceAccountExtraAS();
            newdata.Comment = "Test 2";
            newdata.CategoryID = 1;
            newdata.AccountID = 1;
            newdata.RefenceBuyDocumentID = 1;
            newdata.Name = "Test 2";

            var updatesql = FinanceAccountExtraAS.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_as] SET [COMMENT] = N'{newdata.Comment}',[NAME] = N'{newdata.Name}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateCategory()
        {
            var olddata = new FinanceAccountExtraAS();
            olddata.Comment = "Test 1";
            olddata.CategoryID = 1;
            olddata.AccountID = 1;
            olddata.Name = "Test 1";
            olddata.RefenceBuyDocumentID = 1;
            var newdata = new FinanceAccountExtraAS();
            newdata.Comment = "Test 1";
            newdata.CategoryID = 2;
            newdata.AccountID = 1;
            newdata.RefenceBuyDocumentID = 1;
            newdata.Name = "Test 1";

            var updatesql = FinanceAccountExtraAS.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_as] SET [CTGYID] = {newdata.CategoryID} WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }
    }

    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceAccountExtraLoanTest
    {
        private SqliteDatabaseFixture fixture = null;
        public FinanceAccountExtraLoanTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestCase_UpdateOthers()
        {
            FinanceAccountExtraLoan olddata = new FinanceAccountExtraLoan();
            olddata.AccountID = 1;
            olddata.StartDate = DateTime.Today;
            olddata.EndDate = DateTime.Today.AddYears(1);
            olddata.InterestFree = false;
            olddata.Others = "Test 1";
            olddata.RefDocID = 1;
            olddata.RepaymentMethod = LoanRepaymentMethod.DueRepayment;
            FinanceAccountExtraLoan newdata = new FinanceAccountExtraLoan();
            newdata.AccountID = 1;
            newdata.StartDate = DateTime.Today;
            newdata.EndDate = DateTime.Today.AddYears(1);
            newdata.InterestFree = false;
            newdata.Others = "Test 2";
            newdata.RefDocID = 1;
            newdata.RepaymentMethod = LoanRepaymentMethod.DueRepayment;

            var updatesql = FinanceAccountExtraLoan.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            Assert.Equal($"UPDATE [t_fin_account_ext_loan] SET [OTHERS] = N'{newdata.Others}' WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }

        [Fact]
        public void TestCase_UpdateAnnualRate()
        {
            FinanceAccountExtraLoan olddata = new FinanceAccountExtraLoan();
            olddata.AccountID = 1;
            olddata.StartDate = DateTime.Today;
            olddata.EndDate = DateTime.Today.AddYears(1);
            olddata.InterestFree = true;
            olddata.Others = "Test 1";
            olddata.RefDocID = 1;
            olddata.RepaymentMethod = LoanRepaymentMethod.DueRepayment;
            FinanceAccountExtraLoan newdata = new FinanceAccountExtraLoan();
            newdata.AccountID = 1;
            newdata.StartDate = DateTime.Today;
            newdata.EndDate = DateTime.Today.AddYears(1);
            newdata.InterestFree = false;
            newdata.AnnualRate = 1;
            newdata.Others = "Test 1";
            newdata.RefDocID = 1;
            newdata.RepaymentMethod = LoanRepaymentMethod.DueRepayment;

            var updatesql = FinanceAccountExtraLoan.WorkoutDeltaStringForUpdate(olddata, newdata);
            Assert.NotNull(updatesql);
            if (newdata.InterestFree.GetValueOrDefault())
                Assert.Equal($"UPDATE [t_fin_account_ext_loan] SET [ANNUALRATE] = {newdata.AnnualRate},[INTERESTFREE] = 1 WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
            else
                Assert.Equal($"UPDATE [t_fin_account_ext_loan] SET [ANNUALRATE] = {newdata.AnnualRate},[INTERESTFREE] = 0 WHERE [ACCOUNTID] = {newdata.AccountID}", updatesql);
        }
    }
}
