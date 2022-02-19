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
            new OrderControllerTestData() {
                HomeID = DataSetupUtility.Home1ID,
                CurrentUser = DataSetupUtility.UserB,
                Name = "Test 2",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 20,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 2,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 80,
                    },
                },
            },
            new OrderControllerTestData() {
                HomeID = DataSetupUtility.Home1ID,
                CurrentUser = DataSetupUtility.UserB,
                Name = "Test 2",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 20,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 2,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 80,
                    },
                },
                ChangedSRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 80,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 2,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 20,
                    },
                },
            },
            new OrderControllerTestData() {
                HomeID = DataSetupUtility.Home1ID,
                CurrentUser = DataSetupUtility.UserB,
                Name = "Test 2",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 20,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 2,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 80,
                    },
                },
                ChangedSRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 20,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 2,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 50,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 3,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter3ID,
                        Precent = 30,
                    },
                },
            },
            new OrderControllerTestData() {
                HomeID = DataSetupUtility.Home1ID,
                CurrentUser = DataSetupUtility.UserB,
                Name = "Test 2",
                ValidFrom = new DateTime(2021, 1, 1),
                ValidTo = new DateTime(2023, 1, 1),
                SRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 20,
                    },
                    new FinanceOrderSRule() {
                        RuleID = 2,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter2ID,
                        Precent = 80,
                    },
                },
                ChangedSRule = new List<FinanceOrderSRule> {
                    new FinanceOrderSRule() {
                        RuleID = 1,
                        ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
                        Precent = 100,
                    }
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
            Assert.Equal(testdata.Name, createdorder.Name);
            Assert.Equal(testdata.HomeID, createdorder.HomeID);
            Assert.Equal(testdata.Comment, createdorder.Comment);
            Assert.Equal(testdata.ValidFrom, createdorder.ValidFrom);
            Assert.Equal(testdata.ValidTo, createdorder.ValidTo);
            var norderid = createdorder.ID;
            listCreatedID.Add(norderid);

            // 3. Read all
            getrst = control.Get();
            Assert.NotNull(getrst);
            var getokrst = Assert.IsType<OkObjectResult>(getrst);
            var getorders = Assert.IsAssignableFrom<IQueryable<FinanceOrder>>(getokrst.Value);
            var idxFornewcreated = getorders.ToList<FinanceOrder>().FindIndex(p => p.ID == norderid);
            Assert.NotEqual(-1, idxFornewcreated);

            // 3a. Read single
            var getsinglerst = control.Get(norderid);
            Assert.NotNull(getsinglerst);
            var getsingleorder = Assert.IsType<FinanceOrder>(getsinglerst);
            Assert.Equal(testdata.Name, getsingleorder.Name);
            Assert.Equal(testdata.HomeID, getsingleorder.HomeID);
            Assert.Equal(testdata.Comment, getsingleorder.Comment);
            Assert.Equal(testdata.ValidFrom, getsingleorder.ValidFrom);
            Assert.Equal(testdata.ValidTo, getsingleorder.ValidTo);

            // 3b. Read SRules.
            var srulecontrol = new FinanceOrderSRulesController(context);
            try
            {
                srulecontrol.Get();
            }
            catch (Exception exp)
            {
                Assert.IsType<UnauthorizedAccessException>(exp);
            }
            srulecontrol.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userclaim }
            };
            var srulegetrst = control.Get();
            Assert.NotNull(srulegetrst);

            // 4. Simple Change: Name and comment.
            Delta<FinanceOrder> delta = new Delta<FinanceOrder>();
            delta.UpdatableProperties.Clear();
            delta.UpdatableProperties.Add("Comment");
            delta.UpdatableProperties.Add("Name");
            createdorder.Comment += "Changed";
            delta.TrySetPropertyValue("Comment", createdorder.Comment);
            createdorder.Name += "Changed";
            delta.TrySetPropertyValue("Name", createdorder.Name);
            var patchresult = await control.Patch(norderid, delta);
            var patcheddoc = Assert.IsType<UpdatedODataResult<FinanceOrder>>(patchresult);
            Assert.NotNull(patcheddoc);
            Assert.Equal(createdorder.Comment, patcheddoc.Entity.Comment);
            Assert.Equal(createdorder.Name, patcheddoc.Entity.Name);
            Assert.Equal(createdorder.HomeID, patcheddoc.Entity.HomeID);
            Assert.Equal(createdorder.ValidFrom, patcheddoc.Entity.ValidFrom);
            Assert.Equal(createdorder.ValidTo, patcheddoc.Entity.ValidTo);

            // 4a. Change the validity
            delta = new Delta<FinanceOrder>();
            delta.UpdatableProperties.Clear();
            delta.UpdatableProperties.Add("ValidTo");
            createdorder.ValidTo = new DateTime(createdorder.ValidTo.Ticks).AddMonths(1);
            delta.TrySetPropertyValue("ValidTo", createdorder.ValidTo);
            patchresult = await control.Patch(norderid, delta);
            patcheddoc = Assert.IsType<UpdatedODataResult<FinanceOrder>>(patchresult);
            Assert.NotNull(patcheddoc);
            Assert.Equal(createdorder.Comment, patcheddoc.Entity.Comment);
            Assert.Equal(createdorder.Name, patcheddoc.Entity.Name);
            Assert.Equal(createdorder.HomeID, patcheddoc.Entity.HomeID);
            Assert.Equal(createdorder.ValidFrom, patcheddoc.Entity.ValidFrom);
            Assert.Equal(createdorder.ValidTo, patcheddoc.Entity.ValidTo);

            // 4b. Change the rules
            if (testdata.ChangedSRule.Count > 0)
            {
                createdorder.SRule.Clear();
                testdata.ChangedSRule.ForEach(srulr => createdorder.SRule.Add(srulr));
                var changerst = await control.Put(norderid, createdorder);
                Assert.NotNull(changerst);
                var changedrst = Assert.IsType<UpdatedODataResult<FinanceOrder>>(changerst);
                var changedorder = Assert.IsAssignableFrom<FinanceOrder>(changedrst.Entity);
                Assert.Equal(createdorder.Name, changedorder.Name);
                Assert.Equal(createdorder.HomeID, changedorder.HomeID);
                Assert.Equal(createdorder.Comment, changedorder.Comment);
                Assert.Equal(createdorder.ValidFrom, changedorder.ValidFrom);
                Assert.Equal(createdorder.ValidTo, changedorder.ValidTo);
                Assert.Equal(testdata.ChangedSRule.Count, changedorder.SRule.Count);
            }

            // 5. Delete
            var deleterst = await control.Delete(norderid);
            Assert.NotNull(deleterst);
            var deleteokrst = Assert.IsType<StatusCodeResult>(deleterst);
            Assert.Equal(204, deleteokrst.StatusCode);

            listCreatedID.Remove(norderid);

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(DataSetupUtility.Home1ID, 9999)]
        public async Task TestCase_DeleteNonExistOrder(int hid, int norder)
        {
            var context = this.fixture.GetCurrentDataContext();
            
            // Arrange
            this.fixture.InitHomeTestData(hid, context);

            // Action
            var control = new FinanceOrdersController(context);
            try
            {
                var rst = control.Delete(norder);
                Assert.IsType<BadRequestResult>(rst);
            }
            catch(Exception)
            {
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PutWithInvalidModel()
        {
            var context = this.fixture.GetCurrentDataContext();

            // Action
            var control = new FinanceOrdersController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.Put(9999, null);
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostWithInvalidMode()
        {
            var context = this.fixture.GetCurrentDataContext();

            // Action
            var control = new FinanceOrdersController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.Post(new FinanceOrder());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PatchWithInvalidMode()
        {
            var context = this.fixture.GetCurrentDataContext();

            // Action
            var control = new FinanceOrdersController(context);
            control.ModelState.AddModelError("Name", "The Name field is required.");
            try
            {
                await control.Patch(999, new Delta<FinanceOrder>());
            }
            catch (Exception exp)
            {
                Assert.IsType<BadRequestException>(exp);
            }

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
