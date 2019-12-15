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
                var options = new DbContextOptionsBuilder<hihDataContext>()
                    .UseSqlite(DBConnection, action =>
                    {
                        action.UseRelationalNulls();                        
                    })
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                    .EnableSensitiveDataLogging()
                    .Options;

                // Create the schema in the database
                CurrentDataContext = new hihDataContext(options, true);
                if (!CurrentDataContext.Database.IsSqlite()
                    || CurrentDataContext.Database.IsSqlServer())
                {
                    throw new Exception("Faield!");
                }
                CurrentDataContext.Database.EnsureCreated();
                

                // Setup the tables
                DataSetupUtility.InitializeSystemTables(CurrentDataContext);
                DataSetupUtility.InitializeHomeDefineAndMemberTables(CurrentDataContext);
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
            //hihDataContext.TestingMode = false;
        }

        protected SqliteConnection DBConnection { get; private set; }
        public hihDataContext CurrentDataContext { get; private set; }
    }
}
