using System;
using System.Threading.Tasks;
using hihapi.Controllers.Library;
using hihapi.Exceptions;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Xunit;

namespace hihapi.unittest.UnitTests.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryOrganizationsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryOrganizationsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        // Duplicate-name guards (POST and PUT): within one home, an organization's NativeName or
        // non-empty ChineseName must not collide with any existing row's NativeName or
        // non-empty ChineseName. Mirrors the HomeDefinesController duplicate-name tests.
        private static LibraryOrganizationsController CreateController(hihDataContext context, string user)
        {
            var control = new LibraryOrganizationsController(context);
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

            var name = "unittest-org-" + Guid.NewGuid().ToString("N");
            var created = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
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
            var native = "unittest-org-" + guid;
            var chinese = "unittest-org-cn-" + guid;
            var created = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = native,
                    ChineseName = chinese,
                }));
            try
            {
                // (a) new NativeName hits the existing ChineseName - reported via the name field
                var exA = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = chinese }));
                Assert.Contains("named", exA.Message, StringComparison.Ordinal);
                Assert.Contains(chinese, exA.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("Chinese name", exA.Message, StringComparison.Ordinal);
                // (b) new ChineseName hits the existing NativeName - the message must name the
                // COLLIDING field, not blindly the (non-colliding) NativeName
                var exB = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-org-b-" + guid, ChineseName = native }));
                Assert.Contains("Chinese name", exB.Message, StringComparison.Ordinal);
                Assert.Contains(native, exB.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("unittest-org-b-", exB.Message, StringComparison.Ordinal);
                // (c) new ChineseName hits the existing ChineseName
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-org-c-" + guid, ChineseName = chinese }));
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

            var name = "unittest-org-" + Guid.NewGuid().ToString("N");
            var created = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var variant = "  " + name.ToUpperInvariant() + "  ";
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = variant }));
                Assert.Contains(variant, ex.Message, StringComparison.Ordinal);
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Unicode-complete fold (C# Trim + ToLowerInvariant over materialized name
        // pairs, not SQLite's ASCII-only lower()/space-only trim()): a Cyrillic
        // case variant and an NBSP-padded variant are BOTH caught.
        [Fact]
        public async Task TestCase_Post_UnicodeCaseAndNbspVarianceThrowsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var created = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "КРОСС" }));
            try
            {
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "кросс" }));
                // NBSP padding: C# Trim strips it, SQL trim() never would - written
                // as escapes so the invisible characters are explicit in source.
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "\u00A0КРОСС\u00A0" }));
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task TestCase_Post_BlankChineseNameIsNotDuplicate()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var createdA = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = "unittest-org-a-" + guid,
                    ChineseName = null,
                }));
            try
            {
                var createdB = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                    new LibraryOrganization()
                    {
                        HomeID = DataSetupUtility.Home1ID,
                        NativeName = "unittest-org-b-" + guid,
                        ChineseName = string.Empty,
                    }));
                await control.Delete(createdB.Entity.Id);
            }
            finally
            {
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Two homes may legitimately carry the same organization name - the guard is HomeID-scoped.
        [Fact]
        public async Task TestCase_Post_SameNameDifferentHomeAllowed()
        {
            var context = fixture.GetCurrentDataContext();
            var controlA = CreateController(context, DataSetupUtility.UserA);
            var controlB = CreateController(context, DataSetupUtility.UserB);

            var name = "unittest-org-" + Guid.NewGuid().ToString("N");
            var created1 = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await controlA.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var created2 = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await controlB.Post(
                    new LibraryOrganization() { HomeID = DataSetupUtility.Home2ID, NativeName = name }));
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
            var createdA = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-org-a-" + guid }));
            var createdB = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-org-b-" + guid }));
            try
            {
                var rename = new LibraryOrganization()
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
            var org = new LibraryOrganization()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-org-" + guid,
                ChineseName = "unittest-org-cn-" + guid,
            };
            var created = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(org));
            try
            {
                var same = new LibraryOrganization()
                {
                    Id = created.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = org.NativeName,
                    ChineseName = org.ChineseName,
                };
                var putresult = await control.Put(created.Entity.Id, same);
                Assert.IsType<UpdatedODataResult<LibraryOrganization>>(putresult);
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
            var createdA = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-org-a-" + guid }));
            var createdB = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-org-b-" + guid }));
            try
            {
                var update = new LibraryOrganization()
                {
                    Id = createdB.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = createdB.Entity.NativeName,
                    ChineseName = createdA.Entity.NativeName, // collides with A's NativeName
                };
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Put(createdB.Entity.Id, update));
                Assert.Contains("Chinese name", ex.Message, StringComparison.Ordinal);
                Assert.Contains(createdA.Entity.NativeName, ex.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("unittest-org-b-", ex.Message, StringComparison.Ordinal);
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
            var name = "unittest-org-" + guid;
            var createdA = Assert.IsType<CreatedODataResult<LibraryOrganization>>(await control.Post(
                new LibraryOrganization() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            var legacy = new LibraryOrganization()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "  " + name.ToUpperInvariant() + "  ",
                CreatedAt = DateTime.Now,
                Createdby = DataSetupUtility.UserA,
            };
            context.Organizations.Add(legacy);
            await context.SaveChangesAsync();
            try
            {
                var update = new LibraryOrganization()
                {
                    Id = createdA.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = name,
                    Detail = "unittest-detail-" + guid,
                };
                var putresult = await control.Put(createdA.Entity.Id, update);
                Assert.IsType<UpdatedODataResult<LibraryOrganization>>(putresult);
            }
            finally
            {
                await control.Delete(legacy.Id);
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }
    }
}
