using System;
using hihapi.test.common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace hihapi.unittest
{
    public class SqliteDatabaseFixture : IDisposable
    {
        public SqliteDatabaseFixture()
        {
            // Open connections
            DBConnection = new SqliteConnection("DataSource=:memory:");
            DBConnection.Open();

            try
            {
                // Create the schema in the database
                var context = GetCurrentDataContext();
                if (!context.Database.IsSqlite())
                {
                    throw new Exception("Expected SQLite database!");
                }

                // Create tables and views
                DataSetupUtility.CreateDatabaseTables(context.Database);
                DataSetupUtility.CreateDatabaseViews(context.Database);

                context.Database.EnsureCreated();

                // Setup the tables
                DataSetupUtility.InitializeSystemTables(context);
                DataSetupUtility.InitializeHomeDefineAndMemberTables(context);

                context.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            if (DBConnection != null)
            {
                DBConnection.Close();
                DBConnection = null;
            }
        }

        public hihDataContext GetCurrentDataContext()
        {
            var options = new DbContextOptionsBuilder<hihDataContext>()
                .UseSqlite(DBConnection, action =>
                {
                    action.UseRelationalNulls();
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new hihDataContext(options);
            return context;
        }

        public void InitHomeTestData(int hid, hihDataContext context)
        {
            switch (hid)
            {
                case DataSetupUtility.Home1ID:
                    this.InitHome1TestData(context);
                    break;
                case DataSetupUtility.Home2ID:
                    this.InitHome2TestData(context);
                    break;
                case DataSetupUtility.Home3ID:
                    this.InitHome3TestData(context);
                    break;
                case DataSetupUtility.Home4ID:
                    this.InitHome4TestData(context);
                    break;
                case DataSetupUtility.Home5ID:
                    this.InitHome5TestData(context);
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
        }

        public void InitHome1TestData(hihDataContext context)
        {
            if (!this.IsHome1DataInitialized)
            {
                DataSetupUtility.CreateTestingData_Home1(context);
                this.IsHome1DataInitialized = true;
            }
        }
        public void InitHome2TestData(hihDataContext context)
        {
            if (!this.IsHome2DataInitialized)
            {
                DataSetupUtility.CreateTestingData_Home2(context);
                this.IsHome2DataInitialized = true;
            }
        }
        public void InitHome3TestData(hihDataContext context)
        {
            if (!this.IsHome3DataInitialized)
            {
                DataSetupUtility.CreateTestingData_Home3(context);
                this.IsHome3DataInitialized = true;
            }
        }
        public void InitHome4TestData(hihDataContext context)
        {
            if (!this.IsHome4DataInitialized)
            {
                DataSetupUtility.CreateTestingData_Home4(context);
                this.IsHome4DataInitialized = true;
            }
        }
        public void InitHome5TestData(hihDataContext context)
        {
            if (!this.IsHome5DataInitialized)
            {
                DataSetupUtility.CreateTestingData_Home5(context);
                this.IsHome5DataInitialized = true;
            }
        }
        public void InitBlogTestData(hihDataContext context)
        {
            if (!this.IsBlogDataInitialized)
            {
                DataSetupUtility.CreateTestingData_Blog(context);
                this.IsBlogDataInitialized = true;
            }
        }

        public void DeleteFinanceAccount(hihDataContext context, int acntid)
        {
            var param = new SqliteParameter("@id", acntid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_account WHERE ID = @id", param);
        }

        public void DeleteFinanceControlCenter(hihDataContext context, int ccid)
        {
            var param = new SqliteParameter("@id", ccid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_controlcenter WHERE ID = @id", param);
        }

        public void DeleteFinanceOrder(hihDataContext context, int ordid)
        {
            var param = new SqliteParameter("@id", ordid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_order WHERE ID = @id", param);
        }

        public void DeleteFinancePlan(hihDataContext context, int planid)
        {
            var param = new SqliteParameter("@id", planid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_plan WHERE ID = @id", param);
        }

        public void DeleteFinanceDocument(hihDataContext context, int docid)
        {
            var param = new SqliteParameter("@id", docid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_document WHERE ID = @id", param);
        }

        public void DeleteBlogCollection(hihDataContext context, int collid)
        {
            var param = new SqliteParameter("@id", collid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_blog_coll WHERE ID = @id", param);
        }
        public void DeleteBlogPost(hihDataContext context, int postid)
        {
            var param = new SqliteParameter("@id", postid);
            context.Database.ExecuteSqlRaw("DELETE FROM t_blog_post WHERE ID = @id", param);
        }

        protected SqliteConnection DBConnection { get; private set; }
        public bool IsHome1DataInitialized { get; private set; }
        public bool IsHome2DataInitialized { get; private set; }
        public bool IsHome3DataInitialized { get; private set; }
        public bool IsHome4DataInitialized { get; private set; }
        public bool IsHome5DataInitialized { get; private set; }
        public bool IsBlogDataInitialized { get; private set; }
        //public hihDataContext CurrentDataContext { get; private set; }
    }
}
