using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using hihapi.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace hihapi.test
{
    public static class DataSetupUtility
    {
        public static List<HomeDefine> HomeDefines { get; private set; }
        public static List<HomeMember> HomeMembers { get; private set; }
        public static List<DBVersion> DBVersions { get; private set; }
        public static List<Currency> Currencies { get; private set; }
        public static List<Language> Languages { get; private set; }
        public static List<FinanceAccountCategory> FinanceAccountCategories { get; private set; }
        public static List<FinanceAssetCategory> FinanceAssetCategories { get; private set; }
        public static List<FinanceDocumentType> FinanceDocumentTypes { get; private set; }
        public static List<FinanceTransactionType> FinanceTransactionTypes { get; private set; }


        /// <summary>
        /// Testing data
        /// Home 1
        ///     [Host] User A
        ///     User B
        ///     User C
        ///     User D
        /// Home 2
        ///     [Host] User B
        /// Home 3
        ///     [Host] User A
        ///     User B
        /// Home 4
        ///     [Host] User C
        /// Home 5
        ///     [Host] User D
        /// </summary>
        public const string UserA = "USERA";
        public const string UserB = "USERB";
        public const string UserC = "USERC";
        public const string UserD = "USERD";
        public const int Home1ID = 1;
        public const string Home1BaseCurrency = "CNY";
        public const int Home2ID = 2;
        public const string Home2BaseCurrency = "CNY";
        public const int Home3ID = 3;
        public const string Home3BaseCurrency = "CNY";
        public const int Home4ID = 4;
        public const string Home4BaseCurrency = "USD";
        public const int Home5ID = 5;
        public const string Home5BaseCurrency = "EUR";
        public const string IntegrationTestClient = "hihapi.test.integration";
        public const string IntegrationTestIdentityServerUrl = "http://localhost:5005";
        public const string IntegrationTestAPIScope = "api.hih";
        public const string IntegrationTestPassword = "password";

        static DataSetupUtility()
        {
            DBVersions = new List<DBVersion>();
            HomeDefines = new List<HomeDefine>();
            HomeMembers = new List<HomeMember>();
            Currencies = new List<Currency>();
            Languages = new List<Language>();
            FinanceAccountCategories = new List<FinanceAccountCategory>();
            FinanceAssetCategories = new List<FinanceAssetCategory>();
            FinanceDocumentTypes = new List<FinanceDocumentType>();
            FinanceTransactionTypes = new List<FinanceTransactionType>();

            // Setup tables
            SetupTable_DBVersion();
            SetupTable_Currency();
            SetupTable_Language();
            SetupTable_HomeDefineAndMember();
            SetupTable_FinAccountCategory();
            SetupTable_FinDocumentType();
            SetupTable_FinAssertCategory();
            SetupTable_FinTransactionType();
        }

        public static ClaimsPrincipal GetClaimForUser(String usr)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, usr),
                new Claim(ClaimTypes.NameIdentifier, usr),
            }, "mock"));
        }

        public static void CreateDatabaseTables(DatabaseFacade database) 
        {
            // Home defines
            database.ExecuteSqlRaw(@"CREATE TABLE T_HOMEDEF (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            NAME nvarchar(50) NOT NULL,
	            DETAILS nvarchar(50) NULL,
	            HOST nvarchar(50) NOT NULL,
	            BASECURR nvarchar(5) NOT NULL,
	            CREATEDBY nvarchar(50) NOT NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL )"
            );

            // Home members
            database.ExecuteSqlRaw(@"CREATE TABLE T_HOMEMEM (
	            HID INTEGER NOT NULL,
	            USER nvarchar(50) NOT NULL,
	            DISPLAYAS nvarchar(50) NULL,
	            RELT smallint NOT NULL,
	            CREATEDBY nvarchar(50) NOT NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL,
                PRIMARY KEY(HID, USER),
                FOREIGN KEY(HID) REFERENCES T_HOMEDEF(ID) )"
            );

            // Currency
            database.ExecuteSqlRaw(@"CREATE TABLE T_FIN_CURRENCY (
	            CURR nvarchar(5) PRIMARY KEY NOT NULL,
	            NAME nvarchar(45) NOT NULL,
	            SYMBOL nvarchar(30) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Language
            database.ExecuteSqlRaw(@"CREATE TABLE t_language (
	            LCID int PRIMARY KEY NOT NULL,
	            ISONAME nvarchar(20) NOT NULL,
	            ENNAME nvarchar(100) NOT NULL,
	            NAVNAME nvarchar(100) NOT NULL,
	            APPFLAG bit NULL )");

            // Finance account category
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account_ctgy (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(30) NOT NULL,
	            ASSETFLAG bit NOT NULL DEFAULT 1,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Finance account
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            CTGYID int NOT NULL,
	            NAME nvarchar(30) NOT NULL,
	            COMMENT nvarchar(45) NULL,
	            OWNER nvarchar(50) NULL,
	            STATUS tinyint NULL,
	            CREATEDBY nvarchar(50) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL )");

            // Finance account: DP
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account_ext_dp (
	            ACCOUNTID int PRIMARY KEY NOT NULL,
	            DIRECT bit NOT NULL,
	            STARTDATE date NOT NULL,
	            ENDDATE date NOT NULL,
	            RPTTYPE tinyint NOT NULL,
	            REFDOCID int NOT NULL,
	            DEFRRDAYS nvarchar(100) NULL,
	            COMMENT nvarchar(45) NULL, 
                FOREIGN KEY(ACCOUNTID) REFERENCES t_fin_account(ID)
                ) "
            );

            // Control center
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_controlcenter (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            NAME nvarchar(30) NOT NULL,
	            PARID int NULL,
	            COMMENT nvarchar(45) NULL,
	            OWNER nvarchar(40) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Finance doc. type
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_doc_type (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(30) NOT NULL,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )"
            );

            // Document
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_document (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            DOCTYPE smallint NOT NULL,
	            TRANDATE date NOT NULL,
	            TRANCURR nvarchar(5) NOT NULL,
	            DESP nvarchar(45) NOT NULL,
	            EXGRATE decimal(17, 4) NULL,
	            EXGRATE_PLAN bit NULL,
	            EXGRATE_PLAN2 bit NULL,
	            TRANCURR2 nvarchar(5) NULL,
	            EXGRATE2 decimal(17, 4) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Document Item
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_document_item (
	            DOCID int NOT NULL, 
	            ITEMID int NOT NULL,
	            ACCOUNTID int NOT NULL,
	            TRANTYPE int NOT NULL,
	            TRANAMOUNT decimal(17, 2) NOT NULL,
	            USECURR2 bit NULL,
	            CONTROLCENTERID int NULL,
	            ORDERID int NULL,
	            DESP nvarchar(45) NULL,
                PRIMARY KEY(DOCID, ITEMID) )");

            // Order
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_order (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            NAME nvarchar(30) NOT NULL,
	            VALID_FROM date NOT NULL,
	            VALID_TO date NOT NULL,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Order Srule
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_order_srule (
	            ORDID int NOT NULL, 
	            RULEID int NOT NULL,
	            CONTROLCENTERID int NOT NULL,
	            PRECENT int NOT NULL,
	            COMMENT nvarchar(45) NULL,
                PRIMARY KEY(ORDID, RULEID)
                )");

            // Template DP
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_tmpdoc_dp (
	            DOCID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            REFDOCID int NULL,
	            ACCOUNTID int NOT NULL,
	            TRANDATE date NOT NULL,
	            TRANTYPE int NOT NULL,
	            TRANAMOUNT decimal(17, 2) NOT NULL,
	            CONTROLCENTERID int NULL,
	            ORDERID int NULL,
	            DESP nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Tran. type
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_tran_type (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(30) NOT NULL,
	            EXPENSE bit NOT NULL,
	            PARID int NULL,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Asset category
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_asset_ctgy (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(50) NOT NULL,
	            DESP nvarchar(50) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // Account Extra Asset
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account_ext_as (
	            ACCOUNTID int PRIMARY KEY NOT NULL,
	            CTGYID int NOT NULL,
	            NAME nvarchar(50) NOT NULL,
	            REFDOC_BUY int NOT NULL,
	            COMMENT nvarchar(100) NULL,
	            REFDOC_SOLD int NULL,
                FOREIGN KEY(ACCOUNTID) REFERENCES t_fin_account(ID) )");

            // Account Extra Loan
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account_ext_loan (
	            ACCOUNTID int PRIMARY KEY NOT NULL,
	            STARTDATE datetime NOT NULL,
	            ANNUALRATE decimal(17, 2) NULL,
	            INTERESTFREE bit NULL,
	            REPAYMETHOD tinyint NULL,
	            TOTALMONTH smallint NULL,
	            REFDOCID int NOT NULL,
	            OTHERS nvarchar(100) NULL,
                FOREIGN KEY(ACCOUNTID) REFERENCES t_fin_account(ID) )");

            // Template Loan
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_tmpdoc_loan (
	            DOCID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            REFDOCID int NULL,
	            ACCOUNTID int NOT NULL,
	            TRANDATE date NOT NULL,
	            TRANAMOUNT decimal(17, 2) NOT NULL,
	            INTERESTAMOUNT decimal(17, 2) NULL,
	            CONTROLCENTERID int NULL,
	            ORDERID int NULL,
	            DESP nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL )");

            // DB version
            database.ExecuteSqlRaw(@"CREATE TABLE T_DBVERSION (
                VersionID    INT      PRIMARY KEY NOT NULL,
                ReleasedDate DATETIME NOT NULL,
                AppliedDate  DATETIME NOT NULL )");
        }

        public static void CreateDatabaseViews()
        {

        }

        public static void InitializeSystemTables(hihDataContext db)
        {
            InitialTable_DBVersion(db);
            InitialTable_Currency(db);
            InitialTable_Language(db);
            InitialTable_FinAccountCategory(db);
            InitialTable_FinAssetCategory(db);
            InitialTable_FinDocumentType(db);
            InitialTable_FinTransactionType(db);
            db.SaveChanges();
        }

        public static void InitializeHomeDefineAndMemberTables(hihDataContext db)
        {
            InitialTable_HomeDefineAndMember(db);
            db.SaveChanges();
        }

        public static void InitialTable_DBVersion(hihDataContext db)
        {
            db.DBVersions.AddRange(DataSetupUtility.DBVersions);
        }
        private static void InitialTable_Currency(hihDataContext db)
        {
            db.Currencies.AddRange(DataSetupUtility.Currencies);
        }
        private static void InitialTable_Language(hihDataContext db)
        {
            db.Languages.AddRange(DataSetupUtility.Languages);
        }
        private static void InitialTable_FinAccountCategory(hihDataContext db)
        {
            db.FinAccountCategories.AddRange(DataSetupUtility.FinanceAccountCategories);
        }
        private static void InitialTable_FinDocumentType(hihDataContext db)
        {
            db.FinDocumentTypes.AddRange(DataSetupUtility.FinanceDocumentTypes);
        }
        private static void InitialTable_FinAssetCategory(hihDataContext db)
        {
            db.FinAssetCategories.AddRange(DataSetupUtility.FinanceAssetCategories);
        }
        private static void InitialTable_FinTransactionType(hihDataContext db)
        {
            db.FinTransactionType.AddRange(DataSetupUtility.FinanceTransactionTypes);
        }

        private static void InitialTable_HomeDefineAndMember(hihDataContext db)
        {
            db.HomeDefines.AddRange(DataSetupUtility.HomeDefines);
            db.HomeMembers.AddRange(DataSetupUtility.HomeMembers);
        }

        private static void SetupTable_HomeDefineAndMember()
        {
            // Home 1
            // Member A (host)
            // Member B
            // Member C
            // Member D
            HomeDefines.Add(new HomeDefine()
            {
                ID = Home1ID,
                BaseCurrency = Home1BaseCurrency,
                Name = "Home 1",
                Host = UserA,
                Createdby = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User A",
                Relation = HomeMemberRelationType.Self,
                User = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User B",
                Relation = HomeMemberRelationType.Couple,
                User = UserB
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User C",
                Relation = HomeMemberRelationType.Child,
                User = UserC
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User D",
                Relation = HomeMemberRelationType.Child,
                User = UserD
            });

            // Home 2
            // Member B (Host)
            HomeDefines.Add(new HomeDefine()
            {
                ID = Home2ID,
                BaseCurrency = Home2BaseCurrency,
                Name = "Home 2",
                Host = UserB,
                Createdby = UserB,
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home2ID,
                DisplayAs = "User B",
                Relation = HomeMemberRelationType.Self,
                User = UserB
            });

            // Home 3
            // Member A (Host)
            // Member B
            HomeDefines.Add(new HomeDefine()
            {
                ID = Home3ID,
                BaseCurrency = Home3BaseCurrency,
                Name = "Home 3",
                Host = UserA,
                Createdby = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home3ID,
                DisplayAs = "User A",
                Relation = HomeMemberRelationType.Self,
                User = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home3ID,
                DisplayAs = "User B",
                Relation = HomeMemberRelationType.Couple,
                User = UserB
            });

            // Home 4
            // Member C (Host)
            HomeDefines.Add(new HomeDefine()
            {
                ID = Home4ID,
                BaseCurrency = Home4BaseCurrency,
                Name = "Home 4",
                Host = UserC,
                Createdby = UserC,
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home4ID,
                DisplayAs = "User C",
                Relation = HomeMemberRelationType.Self,
                User = UserC
            });

            // Home 5
            // Member D (host)
            HomeDefines.Add(new HomeDefine()
            {
                ID = Home5ID,
                BaseCurrency = Home5BaseCurrency,
                Name = "Home 5",
                Host = UserD,
                Createdby = UserD,
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home5ID,
                DisplayAs = "User D",
                Relation = HomeMemberRelationType.Self,
                User = UserD
            });
        }
        private static void SetupTable_DBVersion()
        {
            // Versions
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (1,'2018.07.04');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (2,'2018.07.05');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (3,'2018.07.10');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (4,'2018.07.11');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (5,'2018.08.04');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (6,'2018.08.05');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (7,'2018.10.10');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (8,'2018.11.1');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (9,'2018.11.2');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (10,'2018.11.3');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (11,'2018.12.20');
            // INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (12,'2019.4.20');
            DBVersions.Add(new DBVersion()
            {
                VersionID = 1,
                ReleasedDate = new DateTime(2018, 7, 4)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 2,
                ReleasedDate = new DateTime(2018, 7, 5)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 3,
                ReleasedDate = new DateTime(2018, 7, 10)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 4,
                ReleasedDate = new DateTime(2018, 7, 11)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 5,
                ReleasedDate = new DateTime(2018, 8, 4)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 6,
                ReleasedDate = new DateTime(2018, 8, 5)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 7,
                ReleasedDate = new DateTime(2018, 10, 10)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 8,
                ReleasedDate = new DateTime(2018, 11, 1)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 9,
                ReleasedDate = new DateTime(2018, 11, 2)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 10,
                ReleasedDate = new DateTime(2018, 11, 3)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 11,
                ReleasedDate = new DateTime(2018, 12, 20)
            });
            DBVersions.Add(new DBVersion()
            {
                VersionID = 12,
                ReleasedDate = new DateTime(2019, 4, 20)
            });
        }

        private static void SetupTable_Currency()
        {
            Currencies.Add(new Currency() { Curr = "CNY", Name = "Sys.Currency.CNY", Symbol = "¥" });
            Currencies.Add(new Currency() { Curr = "EUR", Name = "Sys.Currency.EUR", Symbol = "€" });
            Currencies.Add(new Currency() { Curr = "HKD", Name = "Sys.Currency.HKD", Symbol = "HK$" });
            Currencies.Add(new Currency() { Curr = "JPY", Name = "Sys.Currency.JPY", Symbol = "¥" });
            Currencies.Add(new Currency() { Curr = "KRW", Name = "Sys.Currency.KRW", Symbol = "₩" });
            Currencies.Add(new Currency() { Curr = "TWD", Name = "Sys.Currency.TWD", Symbol = "TW$" });
            Currencies.Add(new Currency() { Curr = "USD", Name = "Sys.Currency.USD", Symbol = "$" });
        }

        private static void SetupTable_Language()
        {
            Languages.Add(new Language() { Lcid = 4, ISOName = "zh-Hans", EnglishName = "Chinese (Simplified)", NativeName = "简体中文", AppFlag = true });
            Languages.Add(new Language() { Lcid = 9, ISOName = "en", EnglishName = "English", NativeName = "English", AppFlag = true });
            Languages.Add(new Language() { Lcid = 17, ISOName = "ja", EnglishName = "Japanese", NativeName = "日本语", AppFlag = false });
            Languages.Add(new Language() { Lcid = 31748, ISOName = "zh-Hant", EnglishName = "Chinese (Traditional)", NativeName = "繁體中文", AppFlag = false });
        }

        private static void SetupTable_FinAccountCategory()
        {
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 1, Name = "Sys.AcntCty.Cash", AssetFlag = true, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 2, Name = "Sys.AcntCty.DepositAccount", AssetFlag = true, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 3, Name = "Sys.AcntCty.CreditCard", AssetFlag = false, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 4, Name = "Sys.AcntCty.AccountPayable", AssetFlag = false, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 5, Name = "Sys.AcntCty.AccountReceviable", AssetFlag = true, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 6, Name = "Sys.AcntCty.VirtualAccount", AssetFlag = true, Comment = "如支付宝等" });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 7, Name = "Sys.AcntCty.AssetAccount", AssetFlag = true, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 8, Name = "Sys.AcntCty.AdvancedPayment", AssetFlag = true, Comment = null });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 9, Name = "Sys.AcntCty.BorrowFrom", AssetFlag = false, Comment = "借入款、贷款" });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 10, Name = "Sys.AcntCty.LendTo", AssetFlag = true, Comment = "借出款" });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 11, Name = "Sys.AcntCty.AdvancedRecv", AssetFlag = false, Comment = "预收款" });
            FinanceAccountCategories.Add(new FinanceAccountCategory() { ID = 12, Name = "Sys.AcntCty.Insurance", AssetFlag = true, Comment = "保险" });
        }

        private static void SetupTable_FinDocumentType()
        {
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 1, Name = "Sys.DocTy.Normal", Comment = "普通" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 2, Name = "Sys.DocTy.Transfer", Comment = "转账" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 3, Name = "Sys.DocTy.CurrExg", Comment = "兑换不同的货币" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 4, Name = "Sys.DocTy.Installment", Comment = "分期付款" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 5, Name = "Sys.DocTy.AdvancedPayment", Comment = "预付款" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 6, Name = "Sys.DocTy.CreditCardRepay", Comment = "信用卡还款" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 7, Name = "Sys.DocTy.AssetBuyIn", Comment = "购入资产或大件家用器具" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 8, Name = "Sys.DocTy.AssetSoldOut", Comment = "出售资产或大件家用器具" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 9, Name = "Sys.DocTy.BorrowFrom", Comment = "借款、贷款等" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 10, Name = "Sys.DocTy.LendTo", Comment = "借出款" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 11, Name = "Sys.DocTy.Repay", Comment = "借款、贷款等" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 12, Name = "Sys.DocTy.AdvancedRecv", Comment = "预收款" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 13, Name = "Sys.DocTy.AssetValChg", Comment = "资产净值变动" });
            FinanceDocumentTypes.Add(new FinanceDocumentType() { ID = 14, Name = "Sys.DocTy.Insurance", Comment = "保险" });
        }

        private static void SetupTable_FinAssertCategory()
        {
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 1, Name = "Sys.AssCtgy.Apartment", Desp = "公寓" });
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 2, Name = "Sys.AssCtgy.Automobile", Desp = "机动车" });
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 3, Name = "Sys.AssCtgy.Furniture", Desp = "家具" });
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 4, Name = "Sys.AssCtgy.HouseAppliances", Desp = "家用电器" });
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 5, Name = "Sys.AssCtgy.Camera", Desp = "相机" });
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 6, Name = "Sys.AssCtgy.Computer", Desp = "计算机" });
            FinanceAssetCategories.Add(new FinanceAssetCategory() { ID = 7, Name = "Sys.AssCtgy.MobileDevice", Desp = "移动设备" });
        }

        private static void SetupTable_FinTransactionType()
        {
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 2, Name = "主业收入", Expense = false, ParID = null, Comment = "主业收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 3, Name = "工资", Expense = false, ParID = 2, Comment = "工资" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 4, Name = "奖金", Expense = false, ParID = 2, Comment = "奖金" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 35, Name = "津贴", Expense = false, ParID = 2, Comment = "津贴类，如加班等" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 5, Name = "投资、保险、博彩类收入", Expense = false, ParID = null, Comment = "投资、保险、博彩类收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 6, Name = "股票收益", Expense = false, ParID = 5, Comment = "股票收益" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 7, Name = "基金收益", Expense = false, ParID = 5, Comment = "基金收益" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 8, Name = "利息收入", Expense = false, ParID = 5, Comment = "银行利息收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 13, Name = "彩票收益", Expense = false, ParID = 5, Comment = "彩票中奖类收益" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 36, Name = "保险报销收入", Expense = false, ParID = 5, Comment = "保险报销收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 84, Name = "房租收入", Expense = false, ParID = 5, Comment = "房租收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 87, Name = "借贷还款收入", Expense = false, ParID = 5, Comment = "借贷还款收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 90, Name = "资产增值", Expense = false, ParID = 5, Comment = "资产增值" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 93, Name = "资产出售收益", Expense = false, ParID = 5, Comment = "资产出售收益" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 10, Name = "其它收入", Expense = false, ParID = null, Comment = "其它收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 1, Name = "起始资金", Expense = false, ParID = 10, Comment = "起始资金" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 37, Name = "转账收入", Expense = false, ParID = 10, Comment = "转账收入" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 80, Name = "贷款入账", Expense = false, ParID = 10, Comment = "贷款入账" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 91, Name = "预收款收入", Expense = false, ParID = 10, Comment = "预收款收入" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 30, Name = "人情交往类", Expense = false, ParID = null, Comment = "人情交往类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 33, Name = "红包收入", Expense = false, ParID = 30, Comment = "红包收入" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 9, Name = "生活类开支", Expense = true, ParID = null, Comment = "生活类开支" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 11, Name = "物业类支出", Expense = true, ParID = 9, Comment = "物业类支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 14, Name = "小区物业费", Expense = true, ParID = 11, Comment = "小区物业费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 15, Name = "水费", Expense = true, ParID = 11, Comment = "水费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 16, Name = "电费", Expense = true, ParID = 11, Comment = "电费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 17, Name = "天然气费", Expense = true, ParID = 11, Comment = "天然气费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 18, Name = "物业维修费", Expense = true, ParID = 11, Comment = "物业维修费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 26, Name = "通讯费", Expense = true, ParID = 9, Comment = "通讯费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 27, Name = "固定电话/宽带", Expense = true, ParID = 26, Comment = "固定电话/宽带" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 28, Name = "手机费", Expense = true, ParID = 26, Comment = "手机费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 38, Name = "衣服饰品", Expense = true, ParID = 9, Comment = "衣服饰品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 39, Name = "食品酒水", Expense = true, ParID = 9, Comment = "食品酒水" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 40, Name = "衣服鞋帽", Expense = true, ParID = 38, Comment = "衣服鞋帽" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 41, Name = "化妆饰品", Expense = true, ParID = 38, Comment = "化妆饰品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 42, Name = "水果类", Expense = true, ParID = 39, Comment = "水果类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 43, Name = "零食类", Expense = true, ParID = 39, Comment = "零食类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 44, Name = "烟酒茶类", Expense = true, ParID = 39, Comment = "烟酒茶类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 45, Name = "咖啡外卖类", Expense = true, ParID = 39, Comment = "咖啡外卖类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 46, Name = "早中晚餐", Expense = true, ParID = 39, Comment = "早中晚餐" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 49, Name = "休闲娱乐", Expense = true, ParID = 9, Comment = "休闲娱乐" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 50, Name = "旅游度假", Expense = true, ParID = 49, Comment = "旅游度假" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 51, Name = "电影演出", Expense = true, ParID = 49, Comment = "电影演出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 52, Name = "摄影外拍类", Expense = true, ParID = 49, Comment = "摄影外拍类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 53, Name = "腐败聚会类", Expense = true, ParID = 49, Comment = "腐败聚会类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 54, Name = "学习进修", Expense = true, ParID = 9, Comment = "学习进修" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 58, Name = "书刊杂志", Expense = true, ParID = 54, Comment = "书刊杂志" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 59, Name = "培训进修", Expense = true, ParID = 54, Comment = "培训进修" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 61, Name = "日常用品", Expense = true, ParID = 9, Comment = "日常用品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 62, Name = "日用品", Expense = true, ParID = 61, Comment = "日用品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 63, Name = "电子产品类", Expense = true, ParID = 61, Comment = "电子产品类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 64, Name = "厨房用具", Expense = true, ParID = 61, Comment = "厨房用具" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 65, Name = "洗涤用品", Expense = true, ParID = 61, Comment = "洗涤用品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 66, Name = "大家电类", Expense = true, ParID = 61, Comment = "大家电类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 67, Name = "保健护理用品", Expense = true, ParID = 61, Comment = "保健护理用品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 68, Name = "喂哺用品", Expense = true, ParID = 61, Comment = "喂哺用品" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 79, Name = "有线电视费", Expense = true, ParID = 11, Comment = "有线电视费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 85, Name = "房租支出", Expense = true, ParID = 11, Comment = "房租支出" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 12, Name = "私家车支出", Expense = true, ParID = null, Comment = "私家车支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 19, Name = "车辆保养", Expense = true, ParID = 12, Comment = "车辆保养" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 20, Name = "汽油费", Expense = true, ParID = 12, Comment = "汽油费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 21, Name = "车辆保险费", Expense = true, ParID = 12, Comment = "车辆保险费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 22, Name = "停车费", Expense = true, ParID = 12, Comment = "停车费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 23, Name = "车辆维修", Expense = true, ParID = 12, Comment = "车辆维修" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 57, Name = "违章付款类", Expense = true, ParID = 12, Comment = "违章付款类" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 24, Name = "其它支出", Expense = true, ParID = null, Comment = "其它支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 82, Name = "起始负债", Expense = true, ParID = 24, Comment = "起始负债" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 60, Name = "转账支出", Expense = true, ParID = 24, Comment = "转账支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 81, Name = "借出款项", Expense = true, ParID = 24, Comment = "借出款项" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 88, Name = "预付款支出", Expense = true, ParID = 24, Comment = "预付款支出" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 25, Name = "投资、保险、博彩类支出", Expense = true, ParID = null, Comment = "投资、保险、博彩类支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 29, Name = "彩票支出", Expense = true, ParID = 25, Comment = "彩票投注等支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 34, Name = "保单投保、续保支出", Expense = true, ParID = 25, Comment = "保单投保、续保支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 55, Name = "银行利息支出", Expense = true, ParID = 25, Comment = "银行利息支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 56, Name = "银行手续费支出", Expense = true, ParID = 25, Comment = "银行手续费支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 83, Name = "投资手续费支出", Expense = true, ParID = 25, Comment = "投资手续费支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 86, Name = "偿还借贷款", Expense = true, ParID = 25, Comment = "偿还借贷款" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 89, Name = "资产减值", Expense = true, ParID = 25, Comment = "资产减值" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 92, Name = "资产出售费用", Expense = true, ParID = 25, Comment = "资产出售费用" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 31, Name = "人际交往", Expense = true, ParID = null, Comment = "人际交往" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 32, Name = "红包支出", Expense = true, ParID = 31, Comment = "红包支出" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 47, Name = "请客送礼", Expense = true, ParID = 31, Comment = "请客送礼" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 48, Name = "孝敬家长", Expense = true, ParID = 31, Comment = "孝敬家长" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 69, Name = "公共交通类", Expense = true, ParID = null, Comment = "公共交通类" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 70, Name = "公交地铁等", Expense = true, ParID = 69, Comment = "公交地铁等" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 71, Name = "长途客车等", Expense = true, ParID = 69, Comment = "长途客车等" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 72, Name = "火车动车等", Expense = true, ParID = 69, Comment = "火车动车等" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 73, Name = "飞机等", Expense = true, ParID = 69, Comment = "飞机等" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 74, Name = "出租车等", Expense = true, ParID = 69, Comment = "出租车等" });

            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 75, Name = "医疗保健", Expense = true, ParID = null, Comment = "医疗保健" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 76, Name = "诊疗费", Expense = true, ParID = 75, Comment = "诊疗费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 77, Name = "医药费", Expense = true, ParID = 75, Comment = "医药费" });
            FinanceTransactionTypes.Add(new FinanceTransactionType() { ID = 78, Name = "保健品费", Expense = true, ParID = 75, Comment = "保健品费" });
        }
    }
}
