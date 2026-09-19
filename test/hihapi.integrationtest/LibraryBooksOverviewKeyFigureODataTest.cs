using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using hihapi.Models;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace hihapi.integrationtest
{
    // The unit tests call GetLibraryOverviewKeyFigure directly, so the OData
    // ROUTING of the bound collection action on LibraryBooks and the
    // SERIALIZATION of its rankings (a collection-of-complex property - the
    // first on this model) are only proven over real HTTP here.
    [Collection("HIHAPI_IntegrationTests#1")]
    public class LibraryBooksOverviewKeyFigureODataTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private const string TestAuthUserId = "test-user-id"; // TestAuthHandler NameIdentifier

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public LibraryBooksOverviewKeyFigureODataTest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [Fact]
        public async Task OverviewKeyFigure_Action_RoutesAndSerializesRankings()
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
            var category = new LibraryBookCategory
            {
                HomeID = DataSetupUtility.Home1ID,
                Name = "unittest-integ-cat-" + guid,
            };
            var book = new LibraryBook
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-integ-book-" + guid,
                CreatedAt = DateTime.Now,
                Createdby = TestAuthUserId,
            };
            var record = new LibraryBookReadingRecord
            {
                HomeID = DataSetupUtility.Home1ID,
                BookId = 0, // set after the book is saved
                User = TestAuthUserId,
                FromDate = new DateTime(2026, 9, 1),
                ToDate = new DateTime(2026, 9, 10),
                Status = LibraryBookReadingStatus.Completed,
                Comment = "IntegOvk",
            };
            context.HomeMembers.Add(member);
            context.BookCategories.Add(category);
            context.Books.Add(book);
            await context.SaveChangesAsync();
            record.BookId = book.Id;
            context.BookReadingRecords.Add(record);
            await context.SaveChangesAsync();

            // Linkage tables have no DbSet (controller reconciles them via raw
            // SQL) - mirror that here so the ranking join has a row.
            context.Database.ExecuteSqlRaw(
                "INSERT INTO t_lib_book_ctgy (BOOK_ID, CTGY_ID) VALUES (@b, @c)",
                new SqliteParameter("@b", book.Id),
                new SqliteParameter("@c", category.Id));

            try
            {
                var response = await _client.PostAsync(
                    "LibraryBooks/GetLibraryOverviewKeyFigure",
                    new StringContent(
                        $"{{\"HomeID\":{DataSetupUtility.Home1ID}}}",
                        Encoding.UTF8, "application/json"));

                // 404 here would mean the action never materialized on the
                // LibraryBooks collection; OK proves routing under both prefixes.
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                // The action returns a one-element list (Finance keyfigure
                // precedent) -> OData wraps it in a value array.
                var row = Assert.Single(doc.RootElement.GetProperty("value").EnumerateArray());
                Assert.Equal(DataSetupUtility.Home1ID, row.GetProperty("HomeID").GetInt32());
                Assert.True(row.GetProperty("TotalBooks").GetInt32() >= 1);
                Assert.True(row.GetProperty("AddedThisMonth").GetInt32() >= 1);

                // The collection-of-complex ranking property serializes as an
                // array of {Key,Name,Count} objects.
                var rankings = row.GetProperty("TopCategories").EnumerateArray().ToList();
                var mine = Assert.Single(
                    rankings,
                    r => string.Equals(r.GetProperty("Name").GetString(), category.Name, StringComparison.Ordinal));
                Assert.Equal(1, mine.GetProperty("Count").GetInt32());
                Assert.Equal(category.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    mine.GetProperty("Key").GetString());
            }
            finally
            {
                context.Database.ExecuteSqlRaw(
                    "DELETE FROM t_lib_book_ctgy WHERE BOOK_ID = @b",
                    new SqliteParameter("@b", book.Id));
                context.BookReadingRecords.Remove(record);
                context.Books.Remove(book);
                context.BookCategories.Remove(category);
                context.HomeMembers.Remove(member);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }
    }
}
