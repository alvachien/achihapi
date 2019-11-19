using System;
using System.Collections.Generic;
using hihapi.Models;

namespace hihapi.test 
{
    public static class DataSetupUtility
    {
        public static void InitializeDbForTests(hihDataContext db)
        {
            // db.Messages.AddRange(GetSeedingMessages());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(hihDataContext db)
        {
            // db.Messages.RemoveRange(db.Messages);
            InitializeDbForTests(db);
        }
    }
}
