using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using hihapi;
using hihapi.Models;
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
