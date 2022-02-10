using System;
using Xunit;
using hihapi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class OrderTest
    {
        private SqliteDatabaseFixture fixture = null;

        public OrderTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<OrderTestData> TestData => new TheoryData<OrderTestData>
        {
            new OrderTestData() {
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ValidFrom = DateTime.Today,
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2022, 1, 1),
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ValidFrom = new DateTime(2022, 1, 1),
                ValidTo = new DateTime(2021, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 100
                    },
                },
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 100
                    },
                },
                ExpectedValidResult = true,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 10
                    },
                },
                ExpectedValidResult = false,
            },
            new OrderTestData() {
                HomeID = DataSetupUtility.Home1ID,
                Name = "Test 1",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 10
                    },
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 95
                    },
                },
                ExpectedValidResult = false,
            },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestCase_CheckValid(OrderTestData testdata)
        {
            var context = this.fixture.GetCurrentDataContext();

            FinanceOrder order = new FinanceOrder();
            order.Name = testdata.Name;
            order.HomeID = testdata.HomeID;
            order.Comment = testdata.Comment;
            order.ID = testdata.ID;
            foreach(var rule in testdata.SRule)
                order.SRule.Add(rule);
            order.ValidFrom = testdata.ValidFrom;
            order.ValidTo = testdata.ValidTo;
            
            var isValid = order.IsValid(context);

            Assert.Equal(testdata.ExpectedValidResult , isValid);

            await context.DisposeAsync();
        }
    }
}
