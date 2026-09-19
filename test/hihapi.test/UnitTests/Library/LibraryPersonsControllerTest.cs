using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers.Library;
using hihapi.Exceptions;
using hihapi.Models.Library;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace hihapi.unittest.Library
{
    [Collection("HIHAPI_UnitTests#1")]
    public class LibraryPersonsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;

        public LibraryPersonsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        // Regression for the production 500: blank role rows (RoleId 0, as serialized by the
        // UI when an assignment row is added but left unselected) used to be fed to EF through
        // the Persons.Add graph and crash SaveChanges with
        // "The value of 'LibraryPersonRoleLinkage.RoleId' is unknown". Nonexistent and
        // foreign-home IDs must be dropped too; only the valid shared role may persist.
        [Fact]
        public async Task TestCase_Post_BlankAndInvalidRoleIdsDoNotCrash()
        {
            var context = fixture.GetCurrentDataContext();

            // A role owned by another home than the target (UserA/USERC are Home1 members).
            var alienRole = new LibraryPersonRole()
            {
                HomeID = DataSetupUtility.Home2ID,
                Name = "unittest-alien-role-" + Guid.NewGuid().ToString("N"),
                Comment = "alien",
            };
            context.PersonRoles.Add(alienRole);
            await context.SaveChangesAsync();

            try
            {
                var control = new LibraryPersonsController(context);
                control.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA),
                    },
                };

                var person = new LibraryPerson()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = "unittest-person-" + Guid.NewGuid().ToString("N"),
                    PersonRoles = new List<LibraryPersonRoleLinkage>()
                    {
                        new LibraryPersonRoleLinkage() { RoleId = 0 },                  // blank UI row
                        new LibraryPersonRoleLinkage() { RoleId = 1 },                  // seeded shared role
                        new LibraryPersonRoleLinkage() { RoleId = 999999 },             // nonexistent
                        new LibraryPersonRoleLinkage() { RoleId = alienRole.Id },       // other user's home
                        new LibraryPersonRoleLinkage() { RoleId = 1 },                  // duplicate
                    },
                };

                var postresult = await control.Post(person);
                var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(postresult);
                Assert.Equal(person.NativeName, created.Entity.NativeName);

                var linked = await ReadLinkedRoleIdsAsync(context, created.Entity.Id);
                Assert.Equal(new List<int>() { 1 }, linked);

                // Cleanup: the linkage FK has no cascade, so clear it before deleting the person.
                ClearPersonRoleLinkages(context, created.Entity.Id);
                await control.Delete(created.Entity.Id);
            }
            finally
            {
                context.PersonRoles.Remove(alienRole);
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }

        // PUT path hardening: the same invalid IDs must be filtered before the raw-SQL
        // re-insert instead of tripping the linkage table's foreign-key constraint.
        [Fact]
        public async Task TestCase_Put_InvalidRoleIdsFiltered()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new LibraryPersonsController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA),
                },
            };

            var person = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-person-" + Guid.NewGuid().ToString("N"),
            };
            var postresult = await control.Post(person);
            var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(postresult);
            int pid = created.Entity.Id;

            var update = new LibraryPerson()
            {
                Id = pid,
                HomeID = DataSetupUtility.Home1ID,
                NativeName = person.NativeName,
                PersonRoles = new List<LibraryPersonRoleLinkage>()
                {
                    new LibraryPersonRoleLinkage() { RoleId = 0 },      // blank row
                    new LibraryPersonRoleLinkage() { RoleId = 2 },      // seeded shared role
                    new LibraryPersonRoleLinkage() { RoleId = 999999 }, // nonexistent
                },
            };
            var putresult = await control.Put(pid, update);
            Assert.IsType<UpdatedODataResult<LibraryPerson>>(putresult);

            var linked = await ReadLinkedRoleIdsAsync(context, pid);
            Assert.Equal(new List<int>() { 2 }, linked);

            // Cleanup: the linkage FK has no cascade, so clear it before deleting the person.
            ClearPersonRoleLinkages(context, pid);
            await control.Delete(pid);
            await context.DisposeAsync();
        }

        // Duplicate-name guards (POST and PUT): within one home, a person's NativeName or
        // non-empty ChineseName must not collide with any existing row's NativeName or
        // non-empty ChineseName. Mirrors the HomeDefinesController duplicate-name tests.
        private static LibraryPersonsController CreateController(hihDataContext context, string user)
        {
            var control = new LibraryPersonsController(context);
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

            var name = "unittest-person-" + Guid.NewGuid().ToString("N");
            var person = new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = name };
            var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(person));
            try
            {
                var dup = new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = name };
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(dup));
                Assert.Contains(name, ex.Message, StringComparison.Ordinal);
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // All three cross-pairings of {new NN, new CN} × {existing NN, existing CN} reject;
        // the reverse-direction case (new CN hits existing NN) is the one a plain
        // NativeName == NativeName check would miss.
        [Fact]
        public async Task TestCase_Post_ChineseNameCrossMatchesThrowBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var native = "unittest-person-" + guid;
            var chinese = "unittest-cn-" + guid;
            var person = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = native,
                ChineseName = chinese,
            };
            var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(person));
            try
            {
                // (a) new NativeName hits the existing ChineseName - reported via the name field
                var exA = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = chinese }));
                Assert.Contains("named", exA.Message, StringComparison.Ordinal);
                Assert.Contains(chinese, exA.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("Chinese name", exA.Message, StringComparison.Ordinal);
                // (b) new ChineseName hits the existing NativeName - the message must name the
                // COLLIDING field, not blindly the (non-colliding) NativeName
                var exB = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-person-b-" + guid, ChineseName = native }));
                Assert.Contains("Chinese name", exB.Message, StringComparison.Ordinal);
                Assert.Contains(native, exB.Message, StringComparison.Ordinal);
                Assert.DoesNotContain("unittest-person-b-", exB.Message, StringComparison.Ordinal);
                // (c) new ChineseName hits the existing ChineseName
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-person-c-" + guid, ChineseName = chinese }));
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // The guard folds trim + case: "X" and "  x  " are the SAME name (that is the point -
        // sloppy re-typing of an existing entry is the common real-world duplicate).
        [Fact]
        public async Task TestCase_Post_CaseAndWhitespaceVarianceThrowsBadRequest()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var name = "unittest-person-" + Guid.NewGuid().ToString("N");
            var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var variant = "  " + name.ToUpperInvariant() + "  ";
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = variant }));
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

            var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "КРОСС" }));
            try
            {
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "кросс" }));
                // NBSP padding: C# Trim strips it, SQL trim() never would - written
                // as escapes so the invisible characters are explicit in source.
                await Assert.ThrowsAsync<BadRequestException>(() => control.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "\u00A0КРОСС\u00A0" }));
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
            var first = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-person-a-" + guid,
                ChineseName = null,
            };
            var createdA = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(first));
            try
            {
                // null vs "" ChineseName on different NativeNames must not collide.
                var second = new LibraryPerson()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = "unittest-person-b-" + guid,
                    ChineseName = string.Empty,
                };
                var createdB = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(second));
                await control.Delete(createdB.Entity.Id);
            }
            finally
            {
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        // Two homes may legitimately carry the same person name - the guard is HomeID-scoped.
        [Fact]
        public async Task TestCase_Post_SameNameDifferentHomeAllowed()
        {
            var context = fixture.GetCurrentDataContext();
            var controlA = CreateController(context, DataSetupUtility.UserA);
            var controlB = CreateController(context, DataSetupUtility.UserB);

            var name = "unittest-person-" + Guid.NewGuid().ToString("N");
            var created1 = Assert.IsType<CreatedODataResult<LibraryPerson>>(await controlA.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            try
            {
                var created2 = Assert.IsType<CreatedODataResult<LibraryPerson>>(await controlB.Post(
                    new LibraryPerson() { HomeID = DataSetupUtility.Home2ID, NativeName = name }));
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
            var createdA = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-person-a-" + guid }));
            var createdB = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-person-b-" + guid }));
            try
            {
                var rename = new LibraryPerson()
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

        // PUT that keeps the record's own names must succeed: the self-exclusion (p.Id != key)
        // makes a no-op save legal (also what keeps TestCase_Put_InvalidRoleIdsFiltered green).
        [Fact]
        public async Task TestCase_Put_SelfRenameNoOp()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var person = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "unittest-person-" + guid,
                ChineseName = "unittest-cn-" + guid,
            };
            var created = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(person));
            try
            {
                var same = new LibraryPerson()
                {
                    Id = created.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = person.NativeName,
                    ChineseName = person.ChineseName,
                };
                var putresult = await control.Put(created.Entity.Id, same);
                Assert.IsType<UpdatedODataResult<LibraryPerson>>(putresult);
            }
            finally
            {
                await control.Delete(created.Entity.Id);
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task TestCase_Put_ChineseNameCrossMatchThrows()
        {
            var context = fixture.GetCurrentDataContext();
            var control = CreateController(context, DataSetupUtility.UserA);

            var guid = Guid.NewGuid().ToString("N");
            var createdA = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-person-a-" + guid }));
            var createdB = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = "unittest-person-b-" + guid }));
            try
            {
                var update = new LibraryPerson()
                {
                    Id = createdB.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = createdB.Entity.NativeName,
                    ChineseName = createdA.Entity.NativeName, // collides with A's NativeName
                };
                var ex = await Assert.ThrowsAsync<BadRequestException>(() => control.Put(createdB.Entity.Id, update));
                Assert.Contains("Chinese name", ex.Message, StringComparison.Ordinal);
                Assert.Contains(createdA.Entity.NativeName, ex.Message, StringComparison.Ordinal);
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
            var name = "unittest-person-" + guid;
            var createdA = Assert.IsType<CreatedODataResult<LibraryPerson>>(await control.Post(
                new LibraryPerson() { HomeID = DataSetupUtility.Home1ID, NativeName = name }));
            var legacy = new LibraryPerson()
            {
                HomeID = DataSetupUtility.Home1ID,
                NativeName = "  " + name.ToUpperInvariant() + "  ",
                CreatedAt = DateTime.Now,
                Createdby = DataSetupUtility.UserA,
            };
            context.Persons.Add(legacy);
            await context.SaveChangesAsync();
            try
            {
                var update = new LibraryPerson()
                {
                    Id = createdA.Entity.Id,
                    HomeID = DataSetupUtility.Home1ID,
                    NativeName = name,
                    Detail = "unittest-detail-" + guid,
                };
                var putresult = await control.Put(createdA.Entity.Id, update);
                Assert.IsType<UpdatedODataResult<LibraryPerson>>(putresult);
            }
            finally
            {
                await control.Delete(legacy.Id);
                await control.Delete(createdA.Entity.Id);
                await context.DisposeAsync();
            }
        }

        private static void ClearPersonRoleLinkages(hihDataContext context, int personId)
        {
            context.Database.ExecuteSqlRaw(
                "DELETE FROM t_lib_person_role WHERE PERSON_ID = $pid",
                new SqliteParameter("$pid", personId));
        }

        // The linkage table has no DbSet, so rows are read back via raw SQL. The connection is
        // opened/closed through EF (Database.OpenConnectionAsync) so its reference counting
        // stays consistent for later operations on the same context.
        private static async Task<List<int>> ReadLinkedRoleIdsAsync(hihDataContext context, int personId)
        {
            var rst = new List<int>();
            await context.Database.OpenConnectionAsync();
            try
            {
                var conn = context.Database.GetDbConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ROLE_ID FROM t_lib_person_role WHERE PERSON_ID = $pid";
                var p = new SqliteParameter("$pid", personId);
                cmd.Parameters.Add(p);
                using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    rst.Add(rdr.GetInt32(0));
                }
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
            return rst.OrderBy(id => id).ToList();
        }
    }
}
