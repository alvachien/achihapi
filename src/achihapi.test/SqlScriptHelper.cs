using System;
using System.Collections.Generic;
using System.Text;

namespace achihapi.test
{
    internal class SqlScriptHelper
    {
        internal const string FinanceAssetBuyDocument_Init = @"
            SET IDENTITY_INSERT dbo.[t_fin_account] ON;
            INSERT INTO [dbo].[t_fin_account]([ID],[HID],[CTGYID],[NAME],[COMMENT],[OWNER],[CREATEDBY])
                   VALUES (1, 1, 1, N'AssetBuy_Test',N'Asset Buy Test', N'Tester', N'Tester');
            SET IDENTITY_INSERT dbo.[t_fin_account] OFF;
            SET IDENTITY_INSERT dbo.[t_fin_controlcenter] ON;
            INSERT INTO [dbo].[t_fin_controlcenter] ([ID],[HID],[NAME],[PARID],[COMMENT],[OWNER],[CREATEDBY])
                   VALUES (1, 1, N'AssetBuyCC', NULL, N'Asset Buy CC', N'Tester', N'Tester');
            SET IDENTITY_INSERT dbo.[t_fin_controlcenter] OFF;
            SET IDENTITY_INSERT dbo.[t_fin_order] ON;
            INSERT INTO [dbo].[t_fin_order] ([ID],[HID],[NAME],[VALID_FROM],[VALID_TO],[COMMENT],[CREATEDBY])
                   VALUES(1, 1, N'AssetBuyOrd', N'1901-01-01', N'2050-01-01', N'Asset buy order', N'Tester');
            SET IDENTITY_INSERT dbo.[t_fin_order] OFF;
            ";
        internal const string FinanceAssetBuyDocument_Cleanup = @"
            DELETE FROM [dbo].[t_fin_document_item] WHERE [DOCID] > 0;
            DELETE FROM [dbo].[t_fin_document] WHERE [ID] > 0; DBCC CHECKIDENT('t_fin_document', RESEED, 1);
            DELETE FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] > 0;
            DELETE FROM [dbo].[t_fin_account] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_account', RESEED, 1);
            DELETE FROM [dbo].[t_fin_controlcenter] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_controlcenter', RESEED, 1);
            DELETE FROM [dbo].[t_fin_order] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_order', RESEED, 1);
            ";

        internal const string FinanceAssetSoldDocument_Init = @"
            SET IDENTITY_INSERT dbo.[t_fin_account] ON;
            INSERT INTO [dbo].[t_fin_account]([ID],[HID],[CTGYID],[NAME],[COMMENT],[OWNER],[CREATEDBY])
                   VALUES (11, 1, 1, N'AssetSold_Test',N'Asset Sold Test', N'Tester', N'Tester');
            SET IDENTITY_INSERT dbo.[t_fin_account] OFF;
            SET IDENTITY_INSERT dbo.[t_fin_controlcenter] ON;
            INSERT INTO [dbo].[t_fin_controlcenter] ([ID],[HID],[NAME],[PARID],[COMMENT],[OWNER],[CREATEDBY])
                   VALUES (11, 1, N'AssetBuyCC', NULL, N'Asset Buy CC', N'Tester', N'Tester');
            SET IDENTITY_INSERT dbo.[t_fin_controlcenter] OFF;
            SET IDENTITY_INSERT dbo.[t_fin_order] ON;
            INSERT INTO [dbo].[t_fin_order] ([ID],[HID],[NAME],[VALID_FROM],[VALID_TO],[COMMENT],[CREATEDBY])
                   VALUES(11, 1, N'AssetBuyOrd', N'1901-01-01', N'2050-01-01', N'Asset buy order', N'Tester');
            SET IDENTITY_INSERT dbo.[t_fin_order] OFF;
            ";
        internal const string FinanceAssetSoldDocument_Cleanup = @"
            DELETE FROM [dbo].[t_fin_document_item] WHERE [DOCID] > 0;
            DELETE FROM [dbo].[t_fin_document] WHERE [ID] > 0; DBCC CHECKIDENT('t_fin_document', RESEED, 1);
            DELETE FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] > 0;
            DELETE FROM [dbo].[t_fin_account] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_account', RESEED, 1);
            DELETE FROM [dbo].[t_fin_controlcenter] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_controlcenter', RESEED, 1);
            DELETE FROM [dbo].[t_fin_order] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_order', RESEED, 1);
            ";

        internal const string EventHabitController_Cleanup = @"DELETE FROM [dbo].[t_event_habit] WHERE [ID] > 0; 
            DBCC CHECKIDENT('t_event_habit', RESEED, 1); 
            DBCC CHECKIDENT('t_event_habit_detail', RESEED, 1);";

        internal const string EventHabitCheckInController_Cleanup = @"DELETE FROM [dbo].[t_event_habit] WHERE [ID] > 0; 
            DELETE FROM [dbo].[t_event_habit_checkin] WHERE [ID] > 0;
            DBCC CHECKIDENT('t_event_habit', RESEED, 1); 
            DBCC CHECKIDENT('t_event_habit_detail', RESEED, 1);
            DBCC CHECKIDENT('t_event_habit_checkin', RESEED, 1);";

        internal const string FinanceAccount_Cleanup = @"
            DELETE FROM [dbo].[t_fin_account] WHERE [ID] > 0;  DBCC CHECKIDENT('t_fin_account', RESEED, 1);
            ";

        internal const Int32 FinanceAssetBuy_AccountID = 1;
        internal const Int32 FinanceAssetBuy_CCID = 1;
        internal const Int32 FinanceAssetBuy_OrderID = 1;
        internal const Int32 FinanceAssetBuy_TranType = 49;
        internal const Int32 FinanceAssetSold_AccountID = 11;
        internal const Int32 FinanceAssetSold_CCID = 11;
        internal const Int32 FinanceAssetSold_OrderID = 11;
        internal const Int32 FinanceAssetSold_TranType = 10;
        internal const string UnitTest_Currency = "CNY";
        internal const Int32 HID_Tester = 1;        
    }
}
