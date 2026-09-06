using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using hihapi.Models;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace hihapi.integrationtest
{
    // Exercises the full OData pipeline for the reading-records endpoint: the
    // unit tests call the controller directly, so [EnableQuery] - $filter
    // parsing (the OData 4.01 `in` operator used by the UI's title search, and
    // the bare Edm.Date literals the shared filter dialog emits for the
    // .AsDate()-registered date properties), $select and the response
    // serialization are only covered here.
    [Collection("HIHAPI_IntegrationTests#1")]
    public class LibraryBookReadingRecordsODataTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private const string TestAuthUserId = "test-user-id"; // TestAuthHandler NameIdentifier

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public LibraryBookReadingRecordsODataTest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [Fact]
        public async Task Get_FilterWithInOperatorAndDateLiteral_ParsesAndMatches()
        {
            var context = _factory.GetCurrentDataContext();

            // Membership so the home-scoped GET join returns the row, and one
            // reading record for BookId 7 (7 and 8 are the in-list; 9 must not
            // match).
            var member = new HomeMember
            {
                HomeID = DataSetupUtility.Home1ID,
                User = TestAuthUserId,
                Relation = HomeMemberRelationType.Self,
                Createdby = TestAuthUserId,
                CreatedAt = DateTime.Now,
            };
            var record = new LibraryBookReadingRecord
            {
                HomeID = DataSetupUtility.Home1ID,
                BookId = 7,
                User = TestAuthUserId,
                FromDate = new DateTime(2026, 5, 1),
                ToDate = new DateTime(2026, 5, 20),
                Comment = "Integration",
            };
            var otherRecord = new LibraryBookReadingRecord
            {
                HomeID = DataSetupUtility.Home1ID,
                BookId = 9,
                User = TestAuthUserId,
                FromDate = new DateTime(2026, 5, 5),
                ToDate = new DateTime(2026, 5, 6),
                Comment = "OutOfInList",
            };
            context.HomeMembers.Add(member);
            context.BookReadingRecords.AddRange(record, otherRecord);
            await context.SaveChangesAsync();

            try
            {
                var query = "LibraryBookReadingRecords?$count=true"
                    + "&$select=Id,HomeID,BookId,User,FromDate,ToDate,Comment"
                    + "&$filter=HomeID%20eq%201%20and%20(BookId%20in%20(7,8))%20and%20FromDate%20ge%202026-01-01";
                var response = await _client.GetAsync(query);

                // 400 here would mean the OData stack rejects `in` (4.01) or
                // the bare date literal (Edm.Date); OK proves both parse.
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var value = doc.RootElement.GetProperty("value").EnumerateArray().ToList();

                var row = Assert.Single(value);
                Assert.Equal(record.Id, row.GetProperty("Id").GetInt32());
                Assert.Equal(7, row.GetProperty("BookId").GetInt32());

                // The .AsDate() registration must serialize bare yyyy-MM-dd
                // (the UI's parseWireDate contract) rather than a full ISO
                // timestamp.
                var fromDateString = row.GetProperty("FromDate").GetString();
                Assert.Equal("2026-05-01", fromDateString);
            }
            finally
            {
                context.BookReadingRecords.RemoveRange(record, otherRecord);
                context.HomeMembers.Remove(member);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }
    }
}
