using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;

namespace hihapi.test
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
                if (!context.Database.IsSqlite()
                    || context.Database.IsSqlServer())
                {
                    throw new Exception("Faield!");
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
                // Error occurred
            }
            finally
            {
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

            var context = new hihDataContext(options, true);
            return context;
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

        public void DeleteAccount(hihDataContext context, int acntid)
        {
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_account WHERE ID = " + acntid.ToString());
        }

        public void DeleteControlCenter(hihDataContext context, int ccid)
        {
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_controlcenter WHERE ID = " + ccid.ToString());
        }

        public void DeleteOrder(hihDataContext context, int ordid)
        {
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_order WHERE ID = " + ordid.ToString());
        }

        public void DeleteDocument(hihDataContext context, int docid)
        {
            context.Database.ExecuteSqlRaw("DELETE FROM t_fin_document WHERE ID = " + docid.ToString());
        }

        protected SqliteConnection DBConnection { get; private set; }
        public bool IsHome1DataInitialized { get; private set; }
        public bool IsHome2DataInitialized { get; private set; }
        public bool IsHome3DataInitialized { get; private set; }
        public bool IsHome4DataInitialized { get; private set; }
        public bool IsHome5DataInitialized { get; private set; }
        //public hihDataContext CurrentDataContext { get; private set; }
    }
}
