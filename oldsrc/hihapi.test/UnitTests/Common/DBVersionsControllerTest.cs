using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;

namespace hihapi.test.UnitTests
{
    [Collection("HIHAPI_UnitTests#1")]
    public class DBVersionsControllerTest
    {
        SqliteDatabaseFixture fixture = null;

        public DBVersionsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase1_Read()
        {
            var context = this.fixture.GetCurrentDataContext();
            DBVersionsController control = new DBVersionsController(context);

            var version = control.Get();
            Assert.NotEmpty(version);
            var cnt1 = DataSetupUtility.DBVersions.Count();
            var cnt2 = version.Count();
            Assert.Equal(cnt1, cnt2);

            await context.DisposeAsync();
        }

        [Theory]
        [MemberData(nameof(FakedVersions))]
        public async Task TestCase2_POST(int nversion)
        {
            // Arrange
            var dbcontext = this.fixture.GetCurrentDataContext();
            await dbcontext.Database.ExecuteSqlRawAsync("DELETE FROM t_dbversion WHERE VERSIONID > " + nversion.ToString());
            await dbcontext.DisposeAsync();

            // Test
            dbcontext = this.fixture.GetCurrentDataContext();
            DBVersionsController control = new DBVersionsController(dbcontext);
            await control.Post();

            var vers = dbcontext.DBVersions.ToList();
            for(var i = nversion + 1; i <= DBVersionsController.CurrentVersion; i ++)
            {
                Assert.True(vers.Exists(p => p.VersionID == i));
            }
            await dbcontext.DisposeAsync();

            // Reset the db version table
            dbcontext = this.fixture.GetCurrentDataContext();
            await dbcontext.Database.ExecuteSqlRawAsync("DELETE FROM t_dbversion WHERE VERSIONID > 0");
            DataSetupUtility.InitialTable_DBVersion(dbcontext);
            await dbcontext.SaveChangesAsync();

            var lastestVersion = await dbcontext.DBVersions.MaxAsync(p => p.VersionID);
            Assert.True(lastestVersion == DBVersionsController.CurrentVersion);

            await dbcontext.DisposeAsync();
        }

        public static IEnumerable<object[]> FakedVersions
        //{
        //    get
        //    {
        //        //var i1 = new Random().Next(2, 8);
        //        //var i2 = new Random().Next(9, DBVersionsController.CurrentVersion);
        //        return new List<object[]>
        //        {
        //            new object[] { 7 },
        //            new object[] { 11 },
        //        };
        //    }
        //}
        => new List<object[]>
        {
            new object[] {
                new Random().Next(2, 8)
            },
            new object[] {
                new Random().Next(9, DBVersionsController.CurrentVersion)
            }
        };
    }
}
