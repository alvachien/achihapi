using System;
using System.Collections.Generic;
using System.Text;

namespace achihapi.test
{
    internal class SqlScriptHelper
    {
        internal const string FinanceAssetBuyDocument_Init = @"";
        internal const string FinanceAssetBuyDocument_Cleanup = @"";

        internal const string FinanceAssetSoldDocument_Init = @"";
        internal const string FinanceAssetSoldDocument_Cleanup = @"";

        internal const string EventHabitController_Cleanup = @"DELETE FROM[dbo].[t_event_habit] WHERE [ID] > 0; 
                    DBCC CHECKIDENT('t_event_habit', RESEED, 1); 
                    DBCC CHECKIDENT('t_event_habit_detail', RESEED, 1);";
    }
}
