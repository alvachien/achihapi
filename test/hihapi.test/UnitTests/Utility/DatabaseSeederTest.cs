using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using hihapi;
using hihapi.Models;
using hihapi.Models.Library;
using hihapi.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace hihapi.unittest.Utility
{
    /// <summary>
    /// ID-range separation between system-delivered and customer-created
    /// configuration rows (DatabaseSeeder.ReserveCustomerIdRanges): system rows
    /// are seeded below the boundary; the sqlite_sequence bump makes the next
    /// DB-generated row land AT the boundary. Each test runs the real startup
    /// path (EnsureCreated + seed + bump) on its own in-memory database, so it
    /// asserts fresh-deployment behavior — and the re-run case asserts what an
    /// existing database sees on the next start.
    /// </summary>
    public class DatabaseSeederTest
    {
        private static async Task<(SqliteConnection Connection, hihDataContext Context)> CreateSeededDatabaseAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<hihDataContext>()
                .UseSqlite(connection)
                .Options;
            var context = new hihDataContext(options);

            await DatabaseSeeder.SeedAsync(context);
            return (connection, context);
        }

        private static long ReadSequence(hihDataContext context, string table)
        {
            var conn = context.Database.GetDbConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT seq FROM sqlite_sequence WHERE name = $n COLLATE NOCASE";
            var p = cmd.CreateParameter();
            p.ParameterName = "$n";
            p.Value = table;
            cmd.Parameters.Add(p);

            var value = cmd.ExecuteScalar();
            Assert.NotNull(value); // the sequence row must exist after seeding
            return Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        // Replaces every stamp above `version` with one AT `version`, so the next start
        // must run each registered step above it. VersionID is the key, so a stamp is
        // replaced rather than edited - in two saves, since one SaveChanges could insert
        // the new key before the delete of its predecessor frees the way.
        private static async Task RollSchemaBackToAsync(hihDataContext context, Int32 version)
        {
            var newer = await context.DBVersions.Where(v => v.VersionID > version).ToListAsync();
            if (newer.Count == 0) return;

            foreach (var stamp in newer)
            {
                context.DBVersions.Remove(stamp);
            }
            await context.SaveChangesAsync();

            context.DBVersions.Add(new DBVersion
            {
                VersionID = version,
                ReleasedDate = newer[0].ReleasedDate,
                AppliedDate = newer[0].AppliedDate,
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task TestCase_SystemRowsSeededBelowBoundary()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            var ctgies = await context.FinAccountCategories.ToListAsync();
            Assert.NotEmpty(ctgies);
            Assert.All(ctgies, c => Assert.True(c.ID < DatabaseSeeder.CustomerIdRangeStart));

            var bookCtgy = await context.BookCategories.ToListAsync();
            Assert.NotEmpty(bookCtgy);
            Assert.All(bookCtgy, c => Assert.True(c.Id < DatabaseSeeder.CustomerIdRangeStart));
        }

        [Fact]
        public async Task TestCase_SequencesRaisedToBoundary()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            foreach (var table in DatabaseSeeder.CustomerBoundedConfigTables)
            {
                Assert.Equal(DatabaseSeeder.CustomerIdRangeStart - 1, ReadSequence(context, table));
            }
        }

        [Fact]
        public async Task TestCase_GeneratedCustomerRowLandsOnBoundary()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            var ctgy = new FinanceAccountCategory { Name = "Cust.Custom", AssetFlag = true };
            context.FinAccountCategories.Add(ctgy);
            await context.SaveChangesAsync();

            Assert.Equal(DatabaseSeeder.CustomerIdRangeStart, ctgy.ID);
        }

        [Fact]
        public async Task TestCase_RepeatedSeedIsIdempotent()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            // A customer-created row consumes exactly the boundary value...
            context.FinAccountCategories.Add(new FinanceAccountCategory { Name = "Cust.Custom", AssetFlag = true });
            await context.SaveChangesAsync();
            var count = await context.FinAccountCategories.CountAsync();

            // ...and a re-seed at startup (the existing-database case) must not
            // duplicate rows nor lower the sequence under the IDs already in
            // use — lowering it would reissue IDs onto existing rows.
            await DatabaseSeeder.SeedAsync(context);

            Assert.Equal(count, await context.FinAccountCategories.CountAsync());
            Assert.Equal(DatabaseSeeder.CustomerIdRangeStart, ReadSequence(context, "T_FIN_ACCOUNT_CTGY"));

            var next = new FinanceAccountCategory { Name = "Cust.Custom2", AssetFlag = true };
            context.FinAccountCategories.Add(next);
            await context.SaveChangesAsync();
            Assert.Equal(DatabaseSeeder.CustomerIdRangeStart + 1, next.ID);
        }

        [Fact]
        public async Task TestCase_FreshDatabaseStampsCurrentVersion()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            // A fresh database gets the current schema straight from the model:
            // exactly one stamp at CurrentVersion, no historical step executed.
            var rows = await context.DBVersions.ToListAsync();
            var single = Assert.Single(rows);
            Assert.Equal(DatabaseSeeder.CurrentVersion, single.VersionID);

            // Re-seeding must neither stamp again nor re-run anything.
            await DatabaseSeeder.SeedAsync(context);
            Assert.Single(await context.DBVersions.ToListAsync());
        }

        [Fact]
        public async Task TestCase_LegacyDatabaseBaselinesThenUpgrades()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            // Simulate a pre-versioning install: schema present, version table empty.
            context.DBVersions.RemoveRange(await context.DBVersions.ToListAsync());
            await context.SaveChangesAsync();

            await DatabaseSeeder.SeedAsync(context);

            var rows = await context.DBVersions.ToListAsync();

            // Baselined at the last delta-era version, then the registered steps
            // catch it up to CurrentVersion - here the reading-records step, whose
            // DDL no-ops because the model-built schema already carries it.
            Assert.Contains(rows, r => r.VersionID == DatabaseSeeder.LegacyBaselineVersion);
            Assert.Equal(DatabaseSeeder.CurrentVersion, rows.Max(r => r.VersionID));
        }

        // The v23 step inserts its own subject rows into T_LIB_BOOKCTGY_DEF - the same table
        // SeedLibraryBookCategories populates. A table-wide Any() guard in that pass read
        // those rows as "already seeded" and skipped, so a database whose category table was
        // empty at startup (a first start that aborted before the seed pass was saved) came
        // out with the 7 subjects only, every one of them parented to an Education row (41)
        // that was never delivered - a dangling tree with no base categories, and the run
        // stamped CurrentVersion, so no later start repaired it.
        [Fact]
        public async Task TestCase_EducationStepDoesNotStrandTheBaseCategorySeed()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            // The state that used to break: no categories at all, and a stored version
            // below v23, so the next start runs the step against an empty table.
            context.BookCategories.RemoveRange(await context.BookCategories.ToListAsync());
            await context.SaveChangesAsync();
            await RollSchemaBackToAsync(context, 22);

            await DatabaseSeeder.SeedAsync(context);

            var categories = await context.BookCategories.AsNoTracking().ToListAsync();

            // The base set survives the step: roots and the Education parent are there...
            Assert.Contains(categories, c => c.Id == 1);
            Assert.Contains(categories, c => c.Id == 41);
            Assert.Contains(categories, c => c.Id == 61);
            // ...the subjects the step delivers are present exactly once...
            Assert.Equal(7, categories.Count(c => c.Id >= 42 && c.Id <= 48));
            Assert.Single(categories, c => c.Id == 42);
            // ...and nothing points at a parent that does not exist.
            Assert.All(categories.Where(c => c.ParentID != null),
                c => Assert.Contains(categories, p => p.Id == c.ParentID));
        }

        // The v23 step's real job: a deployed database already carries the older system
        // set, with Finance (61) as a ROOT and Accounting (62) hanging under 61. The step
        // must add the seven subjects and move both finance rows under Education (41).
        // Neither half had ever been executed by a test - the subjects were inserted by
        // hand-built SQL and the re-parents only ever ran on a real pre-v23 database.
        [Fact]
        public async Task TestCase_EducationStepDeliversSubjectsAndReparentsOnAPopulatedDatabase()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            // Reach the pre-v23 shape: the subjects not delivered yet, and the two
            // finance rows still where they used to sit.
            var subjects = await context.BookCategories.Where(c => c.Id >= 42 && c.Id <= 48).ToListAsync();
            context.BookCategories.RemoveRange(subjects);
            var finance = await context.BookCategories.SingleAsync(c => c.Id == 61);
            var accounting = await context.BookCategories.SingleAsync(c => c.Id == 62);
            finance.ParentID = null;   // 61 used to be a root
            accounting.ParentID = 61;  // 62 used to hang under Finance
            await context.SaveChangesAsync();
            await RollSchemaBackToAsync(context, 22);

            await DatabaseSeeder.SeedAsync(context);

            var categories = await context.BookCategories.AsNoTracking().ToListAsync();

            // The step delivered exactly the seven subjects...
            Assert.Equal(7, categories.Count(c => c.Id >= 42 && c.Id <= 48));
            // ...and re-parented the two finance rows under Education (41), keeping their
            // historic ids and their linked books untouched.
            Assert.Equal(41, categories.Single(c => c.Id == 61).ParentID);
            Assert.Equal(41, categories.Single(c => c.Id == 62).ParentID);
        }

        // The v24 step is the one carrying a data hazard: adding COPY_COUNT with the
        // wrong default would mark every catalogued book as "gone" (0 copies) at once.
        // This drives a pre-v24 database through the real startup path and asserts the
        // row that predates the column comes out as ONE copy.
        [Fact]
        public async Task TestCase_CopyCountStepAddsColumnAndBackfillsLegacyBooksToOwned()
        {
            var (connection, context) = await CreateSeededDatabaseAsync();
            await using var _ = connection;
            await using var __ = context;

            var home = new HomeDefine()
            {
                Name = "unittest-copycount-" + Guid.NewGuid().ToString("N"),
                Host = "unittest",
                BaseCurrency = "CNY",
            };
            context.HomeDefines.Add(home);
            await context.SaveChangesAsync();

            context.Books.Add(new LibraryBook() { HomeID = home.ID, NativeName = "legacy book" });
            await context.SaveChangesAsync();

            // Reach the pre-v24 state: the column gone (EnsureCreatedAsync built the
            // CURRENT schema from the model, so it has to be dropped) and the version
            // table rolled back below the step so the next start must run it.
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE T_LIB_BOOK_DEF DROP COLUMN COPY_COUNT");
            // VersionID is the key, so the stamp is replaced rather than edited -
            // in two saves, since one SaveChanges could insert the new key before
            // the delete of its predecessor frees the way.
            var stamp = await context.DBVersions.SingleAsync(v => v.VersionID == DatabaseSeeder.CurrentVersion);
            context.DBVersions.Remove(stamp);
            await context.SaveChangesAsync();
            context.DBVersions.Add(new DBVersion
            {
                VersionID = DatabaseSeeder.CurrentVersion - 1,
                ReleasedDate = stamp.ReleasedDate,
                AppliedDate = stamp.AppliedDate,
            });
            await context.SaveChangesAsync();

            await DatabaseSeeder.SeedAsync(context);

            // AsNoTracking: the tracked instance still holds the pre-drop NULL.
            var legacy = await context.Books.AsNoTracking().SingleAsync(b => b.NativeName == "legacy book");
            Assert.Equal(1, legacy.CopyCount);
        }

        [Fact]
        public void TestCase_EveryVersionAboveBaselineHasAStep()
        {
            // Drift guard for the new-entity checklist: bumping CurrentVersion
            // without registering the matching step would silently leave existing
            // databases un-migrated, and a step above CurrentVersion would never run.
            Assert.Equal(DatabaseSeeder.CurrentVersion, DatabaseSeeder.SchemaUpgrades.Max(u => u.Version));
            for (var v = DatabaseSeeder.LegacyBaselineVersion + 1; v <= DatabaseSeeder.CurrentVersion; v++)
            {
                var version = v;
                Assert.Contains(DatabaseSeeder.SchemaUpgrades, u => u.Version == version);
            }
        }
    }
}
