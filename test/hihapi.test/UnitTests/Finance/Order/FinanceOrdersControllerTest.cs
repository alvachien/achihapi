using System;
using Xunit;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.test.UnitTests.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceOrdersControllerTest : IDisposable
    {
        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedID = new List<int>();

        public FinanceOrdersControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<OrderControllerTestData> TestData => new TheoryData<OrderControllerTestData>
        {
            new OrderControllerTestData() {
                HomeID = DataSetupUtility.Home1ID,
                CurrentUser = DataSetupUtility.UserA,
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
            },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestCase_CRUD(OrderControllerTestData testdata)
        {
            var context = this.fixture.GetCurrentDataContext();
            // Pre. setup
            this.fixture.InitHomeTestData(testdata.HomeID, context);

            var control = new FinanceOrdersController(context);

            // 1. No authorization
            try
            {
                control.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            var userclaim = DataSetupUtility.GetClaimForUser(testdata.CurrentUser);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };
            var getrst = control.Get();
            Assert.NotNull(getrst);

            // 2. Create
            FinanceOrder order = new FinanceOrder();
            order.Name = testdata.Name;
            order.HomeID = testdata.HomeID;
            order.Comment = testdata.Comment;
            order.ID = testdata.ID;
            foreach (var rule in testdata.SRule)
                order.SRule.Add(rule);
            order.ValidFrom = testdata.ValidFrom;
            order.ValidTo = testdata.ValidTo;
            var createrst = await control.Post(order);
            Assert.NotNull(createrst);
            var createdorderrst = Assert.IsType<CreatedODataResult<FinanceOrder>>(createrst);
            var createdorder = Assert.IsType<FinanceOrder>(createdorderrst.Entity);
            var norderid = createdorder.ID;
            listCreatedID.Add(norderid);

            // 3. Read all
            getrst = control.Get();
            Assert.NotNull(getrst);

            // 3a. Read single
            var getsinglerst = control.Get(norderid);
            Assert.NotNull(getsinglerst);

            // 4. Change.
            createdorder.Comment += "Changed";
            var changerst = await control.Put(norderid, createdorder);
            Assert.NotNull(changerst);

            await context.DisposeAsync();
        }

        public void Dispose()
        {
            if (this.listCreatedID.Count > 0)
            {
                this.listCreatedID.ForEach(x => this.fixture.DeleteFinanceOrder(this.fixture.GetCurrentDataContext(), x));
                this.listCreatedID.Clear();
                this.fixture.GetCurrentDataContext().SaveChanges();
            }
        }
    }
}
