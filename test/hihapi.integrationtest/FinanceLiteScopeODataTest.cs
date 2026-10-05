using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using hihapi.Models;
using hihapi.test.common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace hihapi.integrationtest
{
    /// <summary>
    /// Restricted ("lite") member scoping over real HTTP: the collection queries
    /// behind /FinancePlans and /FinanceDocumentItemViews must compose with the
    /// OData query options, and the report actions must refuse foreign accounts -
    /// the unit tests call the controllers directly and cannot prove routing or
    /// serialization. TestAuthHandler authenticates every request as a fixed user,
    /// so the assertions all cover that single (restricted) identity.
    /// </summary>
    [Collection("HIHAPI_IntegrationTests#1")]
    public class FinanceLiteScopeODataTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private const string TestAuthUserId = "test-user-id"; // TestAuthHandler NameIdentifier

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public FinanceLiteScopeODataTest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [Fact]
        public async Task LiteMember_Plans_Views_KeyFigures_And_Balance_Are_Scoped()
        {
            var context = _factory.GetCurrentDataContext();
            var guid = Guid.NewGuid().ToString("N");

            // The test user is a restricted member of Home1 (which the factory
            // seeds with its system transaction types; Home1 has no finance rows
            // until this test adds them).
            var member = new HomeMember
            {
                HomeID = DataSetupUtility.Home1ID,
                User = TestAuthUserId,
                DisplayAs = "Integ Lite",
                Relation = HomeMemberRelationType.Couple,
                IsLite = true,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = DateTime.Now,
            };
            var acntOwn = new FinanceAccount
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "integ-lite-own-" + guid,
                Owner = TestAuthUserId,
                CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = DateTime.Now,
            };
            var acntForeign = new FinanceAccount
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "integ-lite-foreign-" + guid,
                Owner = DataSetupUtility.UserA,
                CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = DateTime.Now,
            };
            context.HomeMembers.Add(member);
            context.FinanceAccount.Add(acntOwn);
            context.FinanceAccount.Add(acntForeign);
            await context.SaveChangesAsync();

            var planOwn = NewPlan("integ-lite-plan-own-" + guid, acntOwn.ID);
            var planForeign = NewPlan("integ-lite-plan-foreign-" + guid, acntForeign.ID);
            context.FinancePlan.AddRange(planOwn, planForeign);

            var now = DateTime.Now;
            var docOwn = NewDoc("integ-lite-doc-own-" + guid, acntOwn.ID, 400m, now);
            var docForeign = NewDoc("integ-lite-doc-foreign-" + guid, acntForeign.ID, 700m, now);
            context.FinanceDocument.AddRange(docOwn, docForeign);
            await context.SaveChangesAsync();

            try
            {
                // 1. Plan collection: only the plan aiming at the member's own account.
                var plansResp = await _client.GetAsync(
                    $"FinancePlans?$filter=HomeID eq {DataSetupUtility.Home1ID}");
                Assert.Equal(HttpStatusCode.OK, plansResp.StatusCode);
                var planNames = await ReadStringValuesAsync(plansResp, "Description");
                Assert.Contains(planOwn.Description, planNames);
                Assert.DoesNotContain(planForeign.Description, planNames);

                // 2. Document item views (the search/insight pipeline): the foreign
                //    account's item must not be reachable, also via the key query.
                var viewsResp = await _client.GetAsync(
                    $"FinanceDocumentItemViews?$filter=HomeID eq {DataSetupUtility.Home1ID}");
                Assert.Equal(HttpStatusCode.OK, viewsResp.StatusCode);
                var viewDocIds = await ReadIntValuesAsync(viewsResp, "DocumentID");
                Assert.Contains(docOwn.ID, viewDocIds);
                Assert.DoesNotContain(docForeign.ID, viewDocIds);

                var singleResp = await _client.GetAsync($"FinancePlans({planForeign.ID})");
                Assert.Equal(HttpStatusCode.NotFound, singleResp.StatusCode);

                // 3. Overview key figures: scoped to the owned account (400, not 1100).
                var kfResp = await _client.PostAsync(
                    "FinanceReports/GetFinanceOverviewKeyFigure",
                    new StringContent(
                        $"{{\"HomeID\":{DataSetupUtility.Home1ID},\"Year\":{now.Year},\"Month\":{now.Month},\"ExcludeTransfer\":false}}",
                        Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.OK, kfResp.StatusCode);
                using (var doc = JsonDocument.Parse(await kfResp.Content.ReadAsStringAsync()))
                {
                    var row = Assert.Single(doc.RootElement.GetProperty("value").EnumerateArray());
                    Assert.Equal(400m, row.GetProperty("CurrentMonthIncome").GetDecimal());
                    Assert.Equal(0m, row.GetProperty("CurrentMonthOutgo").GetDecimal());
                    Assert.Equal(400m, row.GetProperty("IncomeYTD").GetDecimal());
                }

                // 4. Account balance: own account OK, foreign account refused (401).
                var balOwn = await _client.PostAsync(
                    "FinanceReports/GetAccountBalance",
                    new StringContent(
                        $"{{\"HomeID\":{DataSetupUtility.Home1ID},\"AccountID\":{acntOwn.ID}}}",
                        Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.OK, balOwn.StatusCode);

                var balForeign = await _client.PostAsync(
                    "FinanceReports/GetAccountBalance",
                    new StringContent(
                        $"{{\"HomeID\":{DataSetupUtility.Home1ID},\"AccountID\":{acntForeign.ID}}}",
                        Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.Unauthorized, balForeign.StatusCode);

                // 5. Wire format: the EDM property was renamed IsChild→IsLite
                //    (2026-10-05). OData payloads must carry the new name and never
                //    the old one — the SQLite column ISCHILD is an EF mapping detail
                //    that must not leak into the contract.
                var memResp = await _client.GetAsync(
                    $"HomeMembers?$filter=HomeID eq {DataSetupUtility.Home1ID} and User eq '{TestAuthUserId}'");
                Assert.Equal(HttpStatusCode.OK, memResp.StatusCode);
                var memJson = await memResp.Content.ReadAsStringAsync();
                Assert.Contains("IsLite", memJson);
                Assert.DoesNotContain("IsChild", memJson);
                using (var memDoc = JsonDocument.Parse(memJson))
                {
                    var memRow = Assert.Single(memDoc.RootElement.GetProperty("value").EnumerateArray());
                    Assert.True(memRow.GetProperty("IsLite").GetBoolean());
                }

                // (Plan write refusal for restricted members is proven at
                //  controller level in FinanceLiteScopeTest.TestCase_Plans_LiteScope.)
            }
            finally
            {
                context.Database.ExecuteSqlRaw(
                    "DELETE FROM T_FIN_DOCUMENT_ITEM WHERE DOCID IN (@d1, @d2)",
                    new SqliteParameter("@d1", docOwn.ID),
                    new SqliteParameter("@d2", docForeign.ID));
                context.Database.ExecuteSqlRaw(
                    "DELETE FROM T_FIN_DOCUMENT WHERE ID IN (@d1, @d2)",
                    new SqliteParameter("@d1", docOwn.ID),
                    new SqliteParameter("@d2", docForeign.ID));
                context.FinancePlan.RemoveRange(planOwn, planForeign);
                context.FinanceAccount.RemoveRange(acntOwn, acntForeign);
                context.HomeMembers.Remove(member);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }

        private static async Task<List<string>> ReadStringValuesAsync(HttpResponseMessage resp, string property)
        {
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("value").EnumerateArray()
                .Select(e => e.GetProperty(property).GetString())
                .ToList();
        }

        private static async Task<List<int>> ReadIntValuesAsync(HttpResponseMessage resp, string property)
        {
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("value").EnumerateArray()
                .Select(e => e.GetProperty(property).GetInt32())
                .ToList();
        }

        private static FinancePlan NewPlan(string desp, int acntid)
        {
            var now = DateTime.Now;
            return new FinancePlan
            {
                HomeID = DataSetupUtility.Home1ID,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Description = desp,
                StartDate = new DateTime(now.Year, 1, 1),
                TargetDate = new DateTime(now.Year + 1, 1, 1),
                PlanType = FinancePlanTypeEnum.Account,
                AccountID = acntid,
                TargetBalance = 1000,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = now,
            };
        }

        private static FinanceDocument NewDoc(string desp, int acntid, decimal amount, DateTime trandate)
        {
            var doc = new FinanceDocument
            {
                HomeID = DataSetupUtility.Home1ID,
                DocType = FinanceDocumentType.DocType_Normal,
                TranDate = trandate,
                TranCurr = DataSetupUtility.Home1BaseCurrency,
                Desp = desp,
                Createdby = DataSetupUtility.UserA,
                CreatedAt = DateTime.Now,
            };
            doc.Items.Add(new FinanceDocumentItem
            {
                ItemID = 1,
                AccountID = acntid,
                TranType = DataSetupUtility.TranType_Income1,
                TranAmount = amount,
            });
            return doc;
        }
    }
}
