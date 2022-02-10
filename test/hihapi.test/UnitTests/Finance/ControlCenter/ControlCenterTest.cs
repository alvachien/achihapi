using Xunit;
using hihapi.Models;
using System.Threading.Tasks;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class ControlCenterTest
    {
        private SqliteDatabaseFixture fixture = null;

        public ControlCenterTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<ControlCenterTestData> TestData => new TheoryData<ControlCenterTestData>
        {
            new ControlCenterTestData() {
                ExpectedValidResult = false,
            },
            new ControlCenterTestData() {
                HomeID = DataSetupUtility.Home1ID,
                ExpectedValidResult = false,
            },
            new ControlCenterTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ExpectedValidResult = true,
            },
            new ControlCenterTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ParentID = 0,
                ExpectedValidResult = false,
            },
            new ControlCenterTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ParentID = DataSetupUtility.Home1ControlCenter1ID,
                ExpectedValidResult = true,
            },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestCase_CheckValid(ControlCenterTestData testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            
            FinanceControlCenter obj = new FinanceControlCenter();
            obj.ID = testdata.ID;
            obj.HomeID = testdata.HomeID;
            obj.Name = testdata.Name;
            obj.Comment = testdata.Comment;
            obj.Owner = testdata.Owner;
            obj.ParentID = testdata.ParentID;

            if (testdata.HomeID == DataSetupUtility.Home1ID)
                fixture.InitHome1TestData(context);
            else if (testdata.HomeID == DataSetupUtility.Home2ID)
                fixture.InitHome2TestData(context);

            var isValid = obj.IsValid(context);

            Assert.Equal(testdata.ExpectedValidResult, isValid);

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, 1001, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.Home1CashAccount1ID, DataSetupUtility.TranType_Expense1)]
        [InlineData(DataSetupUtility.Home1ID, 1002, DataSetupUtility.Home1BaseCurrency, DataSetupUtility.Home1CashAccount1ID, DataSetupUtility.TranType_Income1)]
        [InlineData(DataSetupUtility.Home2ID, 1003, DataSetupUtility.Home2BaseCurrency, DataSetupUtility.Home2CashAccount1ID, DataSetupUtility.TranType_Income1)]
        public async Task TestCase_DeleteNotAllowedWithDocumentExist(int hid, int ccid, string tcur, int acntid, int trantype)
        {
            var context = this.fixture.GetCurrentDataContext();

            FinanceControlCenter obj = new FinanceControlCenter();
            obj.HomeID = hid;
            obj.Name = $"DelTest_{hid}";
            obj.ID = ccid;

            // Create a document
            int ndocid = DataSetupUtility.CreateNormalDoc(context, hid, tcur, acntid, trantype, ccid);
            bool rst = obj.IsDeleteAllowed(context);
            fixture.DeleteFinanceDocument(context, ndocid);
            Assert.False(rst);
            rst = obj.IsDeleteAllowed(context);
            Assert.True(rst);

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, 2001)]
        [InlineData(DataSetupUtility.Home2ID, 2002)]
        [InlineData(DataSetupUtility.Home3ID, 2003)]
        [InlineData(DataSetupUtility.Home4ID, 2004)]
        public async Task TestCase_DeleteNotAllowedWithOrderSRuleExist(int hid, int ccid)
        {
            var context = this.fixture.GetCurrentDataContext();

            FinanceControlCenter obj = new FinanceControlCenter();
            obj.HomeID = hid;
            obj.Name = $"DelTest_{hid}";
            obj.ID = ccid;

            // Create an order
            int norderid = DataSetupUtility.CreateOrder(context, hid, obj.ID);
            bool rst = obj.IsDeleteAllowed(context);
            fixture.DeleteFinanceOrder(context, norderid);
            Assert.False(rst);
            rst = obj.IsDeleteAllowed(context);
            Assert.True(rst);

            await context.DisposeAsync();
        }
    }
}
