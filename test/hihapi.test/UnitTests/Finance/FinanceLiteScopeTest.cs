using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers;
using hihapi.Models;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Xunit;

namespace hihapi.unittest.Finance
{
    /// <summary>
    /// Data-scope tests for restricted ("lite") home members (HomeMember.IsLite):
    /// the overview key figures, the account balance actions, the document item
    /// views and the plans must follow the caller's data scope - not just the home
    /// membership. Home1 seeds UserA (self, full), UserB (couple, full), UserC and
    /// UserD (restricted); cash account 3 belongs to UserC.
    /// </summary>
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceLiteScopeTest : IDisposable
    {
        // No seeded document falls into this period (seeds use offsets of
        // DateTime.Today), so the asserted figures can only come from the
        // documents created below.
        const int TestYear = 1998;
        const int TestMonth = 6;

        private SqliteDatabaseFixture fixture = null;
        private List<int> listCreatedDocID = new List<int>();
        private List<int> listCreatedPlanID = new List<int>();

        public FinanceLiteScopeTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public void Dispose()
        {
            if (this.listCreatedDocID.Count > 0)
            {
                this.listCreatedDocID.ForEach(x => this.fixture.DeleteFinanceDocument(this.fixture.GetCurrentDataContext(), x));
                this.fixture.GetCurrentDataContext().SaveChanges();
                this.listCreatedDocID.Clear();
            }
            if (this.listCreatedPlanID.Count > 0)
            {
                this.listCreatedPlanID.ForEach(x => this.fixture.DeleteFinancePlan(this.fixture.GetCurrentDataContext(), x));
                this.fixture.GetCurrentDataContext().SaveChanges();
                this.listCreatedPlanID.Clear();
            }
        }

        private void SetUser(FinanceReportsController control, string user)
        {
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(user) }
            };
        }

        private int CreateNormalDoc(hihDataContext context, int acntid, int trantype, Decimal amount, int day)
        {
            FinanceDocument doc = new FinanceDocument()
            {
                HomeID = DataSetupUtility.Home1ID,
                DocType = FinanceDocumentType.DocType_Normal,
                Desp = $"Litescope_DOC_{acntid}_{trantype}_{day}",
                TranDate = new DateTime(TestYear, TestMonth, day),
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = DateTime.Now,
            };
            doc.Items.Add(new FinanceDocumentItem()
            {
                ItemID = 1,
                AccountID = acntid,
                TranAmount = amount,
                TranType = trantype,
                ControlCenterID = DataSetupUtility.Home1ControlCenter1ID,
            });
            context.FinanceDocument.Add(doc);
            context.SaveChanges();

            listCreatedDocID.Add(doc.ID);
            return doc.ID;
        }

        [Fact]
        public async Task TestCase_OverviewKeyFigure_LiteScope()
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            // Account 1 is owned by UserA (full), account 3 by UserC (restricted).
            CreateNormalDoc(context, DataSetupUtility.Home1CashAccount1ID, DataSetupUtility.TranType_Income1, 1000, 10);
            CreateNormalDoc(context, DataSetupUtility.Home1CashAccount1ID, DataSetupUtility.TranType_Expense1, 200, 11);
            CreateNormalDoc(context, DataSetupUtility.Home1CashAccount3ID, DataSetupUtility.TranType_Income1, 500, 12);

            var parameters = new ODataActionParameters();
            parameters.Add("HomeID", DataSetupUtility.Home1ID);
            parameters.Add("Year", TestYear);
            parameters.Add("Month", TestMonth);
            parameters.Add("ExcludeTransfer", false);

            var control = new FinanceReportsController(context);

            // 1. A full member sees the whole home.
            SetUser(control, DataSetupUtility.UserA);
            var fullRst = Assert.IsType<OkObjectResult>(await control.GetFinanceOverviewKeyFigure(parameters));
            var fullFig = Assert.IsType<List<FinanceOverviewKeyFigure>>(fullRst.Value)[0];
            Assert.Equal(1500m, fullFig.CurrentMonthIncome);
            Assert.Equal(200m, fullFig.CurrentMonthOutgo);
            Assert.Equal(1500m, fullFig.IncomeYTD);

            // 2. A restricted member only sees the accounts they own.
            SetUser(control, DataSetupUtility.UserC);
            var liteRst = Assert.IsType<OkObjectResult>(await control.GetFinanceOverviewKeyFigure(parameters));
            var liteFig = Assert.IsType<List<FinanceOverviewKeyFigure>>(liteRst.Value)[0];
            Assert.Equal(500m, liteFig.CurrentMonthIncome);
            Assert.Equal(0m, liteFig.CurrentMonthOutgo);
            Assert.Equal(500m, liteFig.IncomeYTD);
            Assert.Equal(0m, liteFig.LastMonthIncome);

            // 3. Non-members of the home stay rejected.
            SetUser(control, "USERS");
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => control.GetFinanceOverviewKeyFigure(parameters));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_AccountBalance_LiteScope()
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            var control = new FinanceReportsController(context);
            var parameters = new ODataActionParameters();
            parameters.Add("HomeID", DataSetupUtility.Home1ID);
            parameters.Add("AccountID", DataSetupUtility.Home1CashAccount1ID);

            // 1. A restricted member may not read a foreign account's balance.
            SetUser(control, DataSetupUtility.UserC);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => control.GetAccountBalance(parameters));

            // 2. Their own account works.
            parameters["AccountID"] = DataSetupUtility.Home1CashAccount3ID;
            Assert.IsType<OkObjectResult>(await control.GetAccountBalance(parameters));

            // 3. A full member still sees every account of the home.
            SetUser(control, DataSetupUtility.UserA);
            Assert.IsType<OkObjectResult>(await control.GetAccountBalance(parameters));

            // 4. An account of another home is rejected for everyone
            //    (previously it silently answered 0).
            parameters["HomeID"] = DataSetupUtility.Home3ID;
            parameters["AccountID"] = DataSetupUtility.Home1CashAccount1ID;
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => control.GetAccountBalance(parameters));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_DocumentItemViews_LiteScope()
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            var docA = CreateNormalDoc(context, DataSetupUtility.Home1CashAccount1ID, DataSetupUtility.TranType_Income1, 300, 15);
            var docC = CreateNormalDoc(context, DataSetupUtility.Home1CashAccount3ID, DataSetupUtility.TranType_Income1, 400, 16);

            var control = new FinanceDocumentItemViewsController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA) }
            };
            var fullRst = Assert.IsType<OkObjectResult>(control.Get());
            var fullList = Assert.IsAssignableFrom<IQueryable<FinanceDocumentItemView>>(fullRst.Value).ToList();
            Assert.Contains(fullList, i => i.DocumentID == docA);
            Assert.Contains(fullList, i => i.DocumentID == docC);

            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserC) }
            };
            var liteRst = Assert.IsType<OkObjectResult>(control.Get());
            var liteList = Assert.IsAssignableFrom<IQueryable<FinanceDocumentItemView>>(liteRst.Value).ToList();
            // Restricted in Home1 - and UserC owns more accounts there than the one
            // used above, and is a full member of Home4 - so all checks are limited
            // to Home1's rows.
            var liteHome1 = liteList.Where(i => i.HomeID == DataSetupUtility.Home1ID).ToList();
            Assert.Contains(liteHome1, i => i.DocumentID == docC);
            Assert.DoesNotContain(liteHome1, i => i.DocumentID == docA);
            var cOwnedAcnts = context.FinanceAccount
                .Where(a => a.HomeID == DataSetupUtility.Home1ID && a.Owner == DataSetupUtility.UserC)
                .Select(a => a.ID).ToList();
            Assert.All(liteHome1, i => Assert.Contains(i.AccountID, cOwnedAcnts));

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_Plans_LiteScope()
        {
            var context = this.fixture.GetCurrentDataContext();
            this.fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            var control = new FinancePlansController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA) }
            };

            // Plans on UserA's account 1, on UserC's account 3, and a home-level one.
            var idA = Assert.IsType<CreatedODataResult<FinancePlan>>(
                await control.Post(NewPlan("Litescope_PlanA", DataSetupUtility.Home1CashAccount1ID))).Entity.ID;
            listCreatedPlanID.Add(idA);
            var idC = Assert.IsType<CreatedODataResult<FinancePlan>>(
                await control.Post(NewPlan("Litescope_PlanC", DataSetupUtility.Home1CashAccount3ID))).Entity.ID;
            listCreatedPlanID.Add(idC);
            var catPlan = new FinancePlan()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = "Litescope_PlanCat",
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.AccountCategory,
                AccountCategoryID = FinanceAccountCategory.AccountCategory_Cash,
                TargetBalance = 1000,
            };
            var idCat = Assert.IsType<CreatedODataResult<FinancePlan>>(await control.Post(catPlan)).Entity.ID;
            listCreatedPlanID.Add(idCat);

            // 1. A full member sees all of them.
            var fullRst = Assert.IsType<OkObjectResult>(control.Get());
            var fullList = Assert.IsAssignableFrom<IQueryable<FinancePlan>>(fullRst.Value).ToList();
            Assert.Contains(fullList, p => p.ID == idA);
            Assert.Contains(fullList, p => p.ID == idC);
            Assert.Contains(fullList, p => p.ID == idCat);

            // 2. The restricted member only sees the plan aiming at their own account.
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserC) }
            };
            var liteRst = Assert.IsType<OkObjectResult>(control.Get());
            var liteList = Assert.IsAssignableFrom<IQueryable<FinancePlan>>(liteRst.Value).ToList();
            Assert.Contains(liteList, p => p.ID == idC);
            Assert.DoesNotContain(liteList, p => p.ID == idA);
            Assert.DoesNotContain(liteList, p => p.ID == idCat);

            // 3. Direct key access must not bypass the scope either.
            Assert.Empty(control.Get(idA).Queryable.ToList());
            Assert.Single(control.Get(idC).Queryable.ToList());

            // 4. Restricted members cannot write or delete plans (UI mirrors this).
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => control.Post(NewPlan("Litescope_PlanX", DataSetupUtility.Home1CashAccount3ID)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => control.Delete(idC));

            await context.DisposeAsync();
        }

        private static FinancePlan NewPlan(string desp, int accountid)
        {
            return new FinancePlan()
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = desp,
                StartDate = new DateTime(2021, 1, 1),
                TargetDate = new DateTime(2022, 1, 1),
                PlanType = FinancePlanTypeEnum.Account,
                AccountID = accountid,
                TargetBalance = 1000,
            };
        }
    }
}
