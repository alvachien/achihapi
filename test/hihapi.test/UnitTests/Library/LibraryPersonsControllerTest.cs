using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers.Library;
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
