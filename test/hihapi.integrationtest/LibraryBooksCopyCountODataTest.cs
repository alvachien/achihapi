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
    // CopyCount is the column the UI's book list now $selects, sorts on and filters
    // on ($filter=CopyCount eq 0 is how the retired books are listed). None of that
    // works unless the property is in the EDM, and an exposed property is exactly
    // what a unit test cannot prove - the model builder is only consulted over real
    // HTTP. A missing property would surface as 400 on every one of these requests,
    // which is also how the bug would reach the user: the whole book list breaks.
    [Collection("HIHAPI_IntegrationTests#1")]
    public class LibraryBooksCopyCountODataTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private const string TestAuthUserId = "test-user-id"; // TestAuthHandler NameIdentifier

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public LibraryBooksCopyCountODataTest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [Fact]
        public async Task Books_Collection_FiltersSelectsAndOrdersOnCopyCount()
        {
            var context = _factory.GetCurrentDataContext();

            var guid = Guid.NewGuid().ToString("N");
            var member = new HomeMember
            {
                HomeID = DataSetupUtility.Home1ID,
                User = TestAuthUserId,
                Relation = HomeMemberRelationType.Self,
                Createdby = TestAuthUserId,
                CreatedAt = DateTime.Now,
            };
            var kept = new LibraryBook
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-integ-kept-" + guid,
                CopyCount = 3,
                CreatedAt = DateTime.Now,
                Createdby = TestAuthUserId,
            };
            var retired = new LibraryBook
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-integ-retired-" + guid,
                CopyCount = 0,
                CreatedAt = DateTime.Now,
                Createdby = TestAuthUserId,
            };
            var unrecorded = new LibraryBook
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-integ-unrecorded-" + guid,
                CreatedAt = DateTime.Now,
                Createdby = TestAuthUserId,
            };
            context.HomeMembers.Add(member);
            context.Books.AddRange(kept, retired, unrecorded);
            await context.SaveChangesAsync();

            try
            {
                // The list's own query shape: $select carries the new column.
                var selectResponse = await _client.GetAsync(
                    "LibraryBooks?$select=Id,NativeName,CopyCount&$orderby=CopyCount%20desc");
                Assert.Equal(HttpStatusCode.OK, selectResponse.StatusCode);

                using (var doc = JsonDocument.Parse(await selectResponse.Content.ReadAsStringAsync()))
                {
                    var rows = doc.RootElement.GetProperty("value").EnumerateArray().ToList();

                    // 3 travels as 3: the value is not swallowed by a zero-guard.
                    var keptRow = Assert.Single(
                        rows, r => string.Equals(r.GetProperty("NativeName").GetString(), kept.NativeName, StringComparison.Ordinal));
                    Assert.Equal(3, keptRow.GetProperty("CopyCount").GetInt32());

                    // 0 travels as 0 and is not omitted from the payload - the
                    // distinction the whole retire feature rests on.
                    var retiredRow = Assert.Single(
                        rows, r => string.Equals(r.GetProperty("NativeName").GetString(), retired.NativeName, StringComparison.Ordinal));
                    Assert.Equal(0, retiredRow.GetProperty("CopyCount").GetInt32());

                    // An unrecorded count serializes as null, and null must never
                    // read as "gone" - only 0 does.
                    var unrecordedRow = Assert.Single(
                        rows, r => string.Equals(r.GetProperty("NativeName").GetString(), unrecorded.NativeName, StringComparison.Ordinal));
                    Assert.Equal(JsonValueKind.Null, unrecordedRow.GetProperty("CopyCount").ValueKind);
                }

                // The filter dialog's retire query: exactly the zero-copy books.
                var filterResponse = await _client.GetAsync("LibraryBooks?$filter=CopyCount%20eq%200&$select=Id,NativeName");
                Assert.Equal(HttpStatusCode.OK, filterResponse.StatusCode);

                using (var doc = JsonDocument.Parse(await filterResponse.Content.ReadAsStringAsync()))
                {
                    var rows = doc.RootElement.GetProperty("value").EnumerateArray().ToList();

                    Assert.Contains(rows, r => string.Equals(r.GetProperty("NativeName").GetString(), retired.NativeName, StringComparison.Ordinal));
                    // The null-count book is NOT a zero-copy book, and the 3-copy
                    // book is not one either.
                    Assert.DoesNotContain(rows, r => string.Equals(r.GetProperty("NativeName").GetString(), unrecorded.NativeName, StringComparison.Ordinal));
                    Assert.DoesNotContain(rows, r => string.Equals(r.GetProperty("NativeName").GetString(), kept.NativeName, StringComparison.Ordinal));
                }
            }
            finally
            {
                context.Books.RemoveRange(kept, retired, unrecorded);
                context.HomeMembers.Remove(member);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }
    }
}
