using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers.Library;
using hihapi.Exceptions;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace hihapi.unittest.UnitTests.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryBooksControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryBooksControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        // Duplicate-name guards (POST and PUT): within one home, a book's NativeName or
        // non-empty ChineseName must not collide with any existing row's NativeName or
        // non-empty ChineseName. Mirrors the HomeDefinesController duplicate-name tests.
        // No author/press/category lists are posted, so no linkage rows exist to clean up.
        private static LibraryBooksController CreateController(hihDataContext context, string user)
        {
            var control = new LibraryBooksController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = DataSetupUtility.GetClaimForUser(user),
                },
            };
            return control;
        }

        [Fact]
        public async Task TestCase_Post_DuplicateNativeNameThrowsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var name = "unittest-book-" + Guid.NewGuid().ToString("N");
            var created = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
                Assert.Contains(name, ex.Message, StringComparison.Ordinal);
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // All three cross-pairings of {new NN, new CN} × {existing NN, existing CN} reject.
        [Fact]
        public async Task TestCase_Post_ChineseNameCrossMatchesThrowBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var native = "unittest-book-" + guid;
            var chinese = "unittest-book-cn-" + guid;
            var created = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = native,
                    ChineseName = chinese,
                }));
            try
            {
                // (a) new NativeName hits the existing ChineseName - reported via the name field
                var exA = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = chinese }));
                Assert.Contains("named", exA.Message, StringComparison.Ordinal);
                Assert.Contains(chinese, exA.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("Chinese name", exA.Message, StringComparison.Ordinal);
                // (b) new ChineseName hits the existing NativeName - the message must name the
                // COLLIDING field, not blindly the (non-colliding) NativeName
                var exB = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-b-" + guid, ChineseName = native }));
                Assert.Contains("Chinese name", exB.Message, StringComparison.Ordinal);
                Assert.Contains(native, exB.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("unittest-book-b-", exB.Message, StringComparison.Ordinal);
                // (c) new ChineseName hits the existing ChineseName
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-c-" + guid, ChineseName = chinese }));
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // The guard folds trim + case: "X" and "  x  " are the SAME name.
        [Fact]
        public async Task TestCase_Post_CaseAndWhitespaceVarianceThrowsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var name = "unittest-book-" + Guid.NewGuid().ToString("N");
            var created = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var variant = "  " + name.ToUpperInvariant() + "  ";
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = variant }));
                Assert.Contains(variant, ex.Message, StringComparison.Ordinal);
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Unicode-complete fold: the guard materializes the per-home name pairs and
        // compares in C# (Trim + ToLowerInvariant), NOT via SQLite's ASCII-only
        // lower() / space-only trim(). A Cyrillic case variant and an NBSP-padded
        // variant are BOTH caught - the old SQL-side fold accepted both silently.
        [Fact]
        public async Task TestCase_Post_UnicodeCaseAndNbspVarianceThrowsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "КРОСС" }));
            try
            {
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "кросс" }));
                // NBSP padding: C# Trim strips it, SQL trim() never would - written
                // as escapes so the invisible characters are explicit in source.
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "\u00A0КРОСС\u00A0" }));
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // ISBN is deliberately NOT part of the duplicate rule: different titles may share an ISBN
        // (e.g. the same book entered with native and Chinese titles as separate records).
        [Fact]
        public async Task TestCase_Post_SameISBNAllowed()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var isbn = "978-" + guid;
            var createdA = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-a-" + guid, ISBN = isbn }));
            try
            {
                var createdB = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-b-" + guid, ISBN = isbn }));
                await control.Delete(createdB.Entity.Id);
            }
            finally
            {
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Two homes may legitimately carry the same book title - the guard is HomeID-scoped.
        [Fact]
        public async Task TestCase_Post_SameNameDifferentHomeAllowed()
        {
            var context = fixture.GetCurrentDataContext();
            var controlA = CreateController(context, DataSetupUtility.UserA);
            var controlB = CreateController(context, DataSetupUtility.UserB);

            var name = "unittest-book-" + Guid.NewGuid().ToString("N");
            var created1 = Assert.IsType<CreatedODataResult<LibraryBook>>(await controlA.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var created2 = Assert.IsType<CreatedODataResult<LibraryBook>>(await controlB.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home2ID, NativeName = name }));
                await controlB.Delete(created2.Entity.Id);
            }
            finally
            {
                await controlA.Delete(created1.Entity.Id);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task TestCase_Put_RenameToExistingNameThrowsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var createdA = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-a-" + guid }));
            var createdB = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-b-" + guid }));
            try
            {
                var rename = new LibraryBook()
                {
                    Id = createdB.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = createdA.Entity.NativeName,
                };
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Put(createdB.Entity.Id, rename));
                Assert.Contains(createdA.Entity.NativeName, ex.Message, StringComparison.Ordinal);
            }
            finally
            {
                await control.Delete(createdB.Entity.Id);
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // PUT that keeps the record's own names must succeed (self-exclusion, p.Id != key).
        [Fact]
        public async Task TestCase_Put_SelfRenameNoOp()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var book = new LibraryBook()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-book-" + guid,
                ChineseName = "unittest-book-cn-" + guid,
            };
            var created = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(book));
            try
            {
                var same = new LibraryBook()
                {
                    Id = created.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = book.NativeName,
                    ChineseName = book.ChineseName,
                };
                var putresult = await control.Put(created.Entity.Id, same);
                Assert.IsType<UpdatedODataResult<LibraryBook>>(putresult);
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // The guard must name the field that actually collided: setting B's ChineseName to a
        // value colliding with A's NativeName reports the ChineseName, not B's unchanged
        // NativeName.
        [Fact]
        public async Task TestCase_Put_ChineseNameCrossMatchThrows()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var createdA = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-a-" + guid }));
            var createdB = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-book-b-" + guid }));
            try
            {
                var update = new LibraryBook()
                {
                    Id = createdB.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = createdB.Entity.NativeName,
                    ChineseName = createdA.Entity.NativeName, // collides with A's NativeName
                };
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Put(createdB.Entity.Id, update));
                Assert.Contains("Chinese name", ex.Message, StringComparison.Ordinal);
                Assert.Contains(createdA.Entity.NativeName, ex.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("unittest-book-b-", ex.Message, StringComparison.Ordinal);
            }
            finally
            {
                await control.Delete(createdB.Entity.Id);
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // PUT lockout regression: a row that ALREADY collides with another row (data predating
        // the guard, inserted here directly to bypass the POST guard) must stay savable when
        // only a non-name field is edited - the guard checks just the CHANGED name fields.
        [Fact]
        public async Task TestCase_Put_PreExistingDuplicateStaysSavable()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var name = "unittest-book-" + guid;
            var createdA = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            var legacy = new LibraryBook()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "  " + name.ToUpperInvariant() + "  ",
                CreatedAt = DateTime.Now,
                Createdby = DataSetupUtility.UserA,
            };
            context.Books.Add(legacy);
            await context.SaveChangesAsync();
            try
            {
                var update = new LibraryBook()
                {
                    Id = createdA.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = name,
                    Detail = "unittest-detail-" + guid,
                };
                var putresult = await control.Put(createdA.Entity.Id, update);
                Assert.IsType<UpdatedODataResult<LibraryBook>>(putresult);
            }
            finally
            {
                await control.Delete(legacy.Id);
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // The server-side overview aggregate (GetLibraryOverviewKeyFigure action):
        // totals, month-window deltas, DISTINCT-book completion counts and the
        // top-3 rankings with display-name fallback + ordinal tie-break - exactly
        // what the old client-side collection walk computed. The neighboring
        // Library suites all clean up after themselves, so Home1's library rows
        // are exclusively this test's while it runs.
        [Fact]
        public async Task TestCase_GetLibraryOverviewKeyFigure()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var cat1 = new LibraryBookCategory() { HomeID = DataSetupUtility.Home1ID, Name = "unittest-ovk-cat1-" + guid };
            var cat2 = new LibraryBookCategory() { HomeID = DataSetupUtility.Home1ID, Name = "unittest-ovk-cat2-" + guid };
            var cat3 = new LibraryBookCategory() { HomeID = DataSetupUtility.Home1ID, Name = "unittest-ovk-cat3-" + guid };
            var authorA = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-ovk-authorA-" + guid,
                CreatedAt = DateTime.Now,
                Createdby = DataSetupUtility.UserA,
            };
            // Empty NativeName -> the author ranking must fall back to ChineseName.
            var authorB = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = string.Empty,
                ChineseName = "unittest-ovk-authorB-cn-" + guid,
                CreatedAt = DateTime.Now,
                Createdby = DataSetupUtility.UserA,
            };
            var pressX = new LibraryOrganization()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-ovk-pressX-" + guid,
                CreatedAt = DateTime.Now,
                Createdby = DataSetupUtility.UserA,
            };
            context.BookCategories.AddRange(cat1, cat2, cat3);
            context.Persons.AddRange(authorA, authorB);
            context.Organizations.Add(pressX);
            await context.SaveChangesAsync();

            var now = DateTime.Now;
            var thisBegin = new DateTime(now.Year, now.Month, 1);
            var lastBegin = thisBegin.AddMonths(-1);

            var books = new List<LibraryBook>();
            for (var i = 1; i <= 4; i++)
            {
                var createdBk = Assert.IsType<CreatedODataResult<LibraryBook>>(await control.Post(
                    new LibraryBook() { HomeID = DataSetupUtility.Home1ID, NativeName = $"unittest-ovk-b{i}-" + guid }));
                books.Add(createdBk.Entity);
            }

            // A fifth book stamped LAST month (POST always stamps "now", so direct).
            var book5 = new LibraryBook()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-ovk-b5-" + guid,
                CreatedAt = lastBegin.AddDays(5),
                Createdby = DataSetupUtility.UserA,
            };
            context.Books.Add(book5);
            await context.SaveChangesAsync();

            // Linkages ride the PUT path (raw-SQL reconciled):
            // b1: cat1+cat2 / authorA / pressX; b2: cat1 / authorA;
            // b3: cat1 / authorB; b4: cat3.
            async Task LinkAsync(LibraryBook book, int[] ctgys, int[] authors, int[] presses)
            {
                await control.Put(book.Id, new LibraryBook()
                {
                    Id = book.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = book.NativeName,
                    BookCategories = ctgys.Select(c => new LibraryBookCategoryLinkage() { CategoryId = c }).ToList(),
                    BookAuthors = authors.Select(a => new LibraryBookAuthorLinkage() { AuthorId = a }).ToList(),
                    BookPresses = presses.Select(p => new LibraryBookPressLinkage() { PressId = p }).ToList(),
                });
            }

            await LinkAsync(books[0], new[] { cat1.Id, cat2.Id }, new[] { authorA.Id }, new[] { pressX.Id });
            await LinkAsync(books[1], new[] { cat1.Id }, new[] { authorA.Id }, Array.Empty<int>());
            await LinkAsync(books[2], new[] { cat1.Id }, new[] { authorB.Id }, Array.Empty<int>());
            await LinkAsync(books[3], new[] { cat3.Id }, Array.Empty<int>(), Array.Empty<int>());

            // Completed books this month: b1 + b4 (b4 twice - must count once);
            // last month: b1. b2 open / b3 aborted never count.
            var records = new List<LibraryBookReadingRecord>()
            {
                new LibraryBookReadingRecord() { HomeID = DataSetupUtility.Home1ID, BookId = books[0].Id, User = DataSetupUtility.UserA, FromDate = thisBegin.AddDays(1), ToDate = thisBegin.AddDays(2), Comment = "ovk", Status = LibraryBookReadingStatus.Completed },
                new LibraryBookReadingRecord() { HomeID = DataSetupUtility.Home1ID, BookId = books[0].Id, User = DataSetupUtility.UserA, FromDate = lastBegin.AddDays(1), ToDate = lastBegin.AddDays(2), Comment = "ovk", Status = LibraryBookReadingStatus.Completed },
                new LibraryBookReadingRecord() { HomeID = DataSetupUtility.Home1ID, BookId = books[1].Id, User = DataSetupUtility.UserA, FromDate = thisBegin.AddDays(1), Comment = "ovk", Status = LibraryBookReadingStatus.Reading },
                new LibraryBookReadingRecord() { HomeID = DataSetupUtility.Home1ID, BookId = books[2].Id, User = DataSetupUtility.UserA, FromDate = thisBegin.AddDays(1), ToDate = thisBegin.AddDays(2), Comment = "ovk", Status = LibraryBookReadingStatus.Aborted },
                new LibraryBookReadingRecord() { HomeID = DataSetupUtility.Home1ID, BookId = books[3].Id, User = DataSetupUtility.UserA, FromDate = thisBegin.AddDays(1), ToDate = thisBegin.AddDays(2), Comment = "ovk", Status = LibraryBookReadingStatus.Completed },
                new LibraryBookReadingRecord() { HomeID = DataSetupUtility.Home1ID, BookId = books[3].Id, User = DataSetupUtility.UserA, FromDate = thisBegin.AddDays(1), ToDate = thisBegin.AddDays(3), Comment = "ovk2", Status = LibraryBookReadingStatus.Completed },
            };
            context.BookReadingRecords.AddRange(records);
            await context.SaveChangesAsync();

            try
            {
                var parameters = new ODataActionParameters { { "HomeID", DataSetupUtility.Home1ID } };
                var result = await control.GetLibraryOverviewKeyFigure(parameters);
                var figures = Assert.IsType<List<LibraryOverviewKeyFigure>>(Assert.IsType<OkObjectResult>(result).Value);
                var figure = Assert.Single(figures);

                Assert.Equal(DataSetupUtility.Home1ID, figure.HomeID);
                Assert.Equal(5, figure.TotalBooks);
                Assert.Equal(4, figure.AddedThisMonth);
                Assert.Equal(1, figure.AddedLastMonth);
                Assert.Equal(2, figure.CompletedThisMonth);
                Assert.Equal(1, figure.CompletedLastMonth);

                var categories = figure.TopCategories;
                Assert.Equal(3, categories.Count);
                Assert.Equal(cat1.Id.ToString(CultureInfo.InvariantCulture), categories[0].Key);
                Assert.Equal(3, categories[0].Count);
                Assert.Equal(cat2.Name, categories[1].Name); // count tie -> ordinal name order
                Assert.Equal(cat3.Name, categories[2].Name);

                Assert.Equal(2, figure.TopAuthors[0].Count);
                Assert.Equal(authorA.NativeName, figure.TopAuthors[0].Name);
                Assert.Equal(authorB.ChineseName, figure.TopAuthors[1].Name); // Native empty -> fallback

                Assert.Single(figure.TopPresses);
                Assert.Equal(pressX.NativeName, figure.TopPresses[0].Name);
                Assert.Equal(1, figure.TopPresses[0].Count);
            }
            finally
            {
                context.BookReadingRecords.RemoveRange(records);
                await context.SaveChangesAsync();
                foreach (var book in books)
                {
                    await control.Delete(book.Id);
                }

                await control.Delete(book5.Id);
                context.BookCategories.RemoveRange(new[] { cat1, cat2, cat3 });
                await context.SaveChangesAsync();
                context.Persons.RemoveRange(new[] { authorA, authorB });
                await context.SaveChangesAsync();
                context.Organizations.Remove(pressX);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task TestCase_GetLibraryOverviewKeyFigure_MissingHomeIDReturnsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);
            try
            {
                var result = await control.GetLibraryOverviewKeyFigure(new ODataActionParameters());
                // ODataController.BadRequest(string) hands back a BadRequestODataResult.
                var bad = Assert.IsType<BadRequestODataResult>(result);
                Assert.Equal(400, bad.StatusCode);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task TestCase_GetLibraryOverviewKeyFigure_WithoutUserThrowsUnauthorized()
        {
            var context = fixture.GetCurrentDataContext();
            var control = new LibraryBooksController(context); // no ControllerContext -> no user
            try
            {
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    control.GetLibraryOverviewKeyFigure(new ODataActionParameters { { "HomeID", DataSetupUtility.Home1ID } }));
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}
