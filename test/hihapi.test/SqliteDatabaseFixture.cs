using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;

namespace hihapi.test
{
    public class SqliteDatabaseFixture : IDisposable
    {
        public SqliteDatabaseFixture()
        {
            hihDataContext.TestingMode = true;

            // Open connections
            DBConnection = new SqliteConnection("DataSource=:memory:");
            DBConnection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<hihDataContext>()
                    .UseSqlite(DBConnection)
                    .Options;

                // Create the schema in the database
                CurrentDataContext = new hihDataContext(options);
                CurrentDataContext.Database.EnsureCreated();

                // Setup the tables
                DataSetupUtility.InitializeSystemTables(CurrentDataContext);
                DataSetupUtility.InitializeHomeDefineAndMemberTables(CurrentDataContext);
            }
            catch
            {
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
            hihDataContext.TestingMode = false;
        }

        protected SqliteConnection DBConnection { get; private set; }
        public hihDataContext CurrentDataContext { get; private set; }
    }
}
