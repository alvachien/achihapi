using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using hihapi.Models;
using hihapi.Controllers;
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

        #region Create tables and Views
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
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE,
                CONSTRAINT UK_t_homedef_NAME UNIQUE (NAME) )"
            );

            // Home members
            database.ExecuteSqlRaw(@"CREATE TABLE T_HOMEMEM (
	            HID INTEGER NOT NULL,
	            USER nvarchar(50) NOT NULL,
	            DISPLAYAS nvarchar(50) NULL,
	            RELT smallint NOT NULL,
	            CREATEDBY nvarchar(50) NOT NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE,
                CONSTRAINT PK_t_homemem PRIMARY KEY (HID, USER),
                CONSTRAINT FK_t_homemem_HID FOREIGN KEY(HID) REFERENCES T_HOMEDEF(ID) ON DELETE CASCADE ON UPDATE CASCADE )"
            );
            // Home message - TBD

            // Currency
            database.ExecuteSqlRaw(@"CREATE TABLE T_FIN_CURRENCY (
	            CURR nvarchar(5) PRIMARY KEY NOT NULL,
	            NAME nvarchar(45) NOT NULL,
	            SYMBOL nvarchar(30) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )");

            // Language
            database.ExecuteSqlRaw(@"CREATE TABLE t_language (
	            LCID int PRIMARY KEY NOT NULL,
	            ISONAME nvarchar(20) NOT NULL,
	            ENNAME nvarchar(100) NOT NULL,
	            NAVNAME nvarchar(100) NOT NULL,
	            APPFLAG bit NULL )");

            // Finance account category
            database.ExecuteSqlRaw(@"CREATE TABLE T_FIN_ACCOUNT_CTGY (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(30) NOT NULL,
	            ASSETFLAG bit NOT NULL DEFAULT 1,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE
                )");

            // Finance account
            database.ExecuteSqlRaw(@"CREATE TABLE T_FIN_ACCOUNT (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            CTGYID int NOT NULL,
	            NAME nvarchar(30) NOT NULL,
	            COMMENT nvarchar(45) NULL,
	            OWNER nvarchar(50) NULL,
	            STATUS tinyint NULL,
	            CREATEDBY nvarchar(50) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE
                )");

            // Finance account: DP
            database.ExecuteSqlRaw(@"CREATE TABLE T_FIN_ACCOUNT_EXT_DP (
	            ACCOUNTID int PRIMARY KEY NOT NULL,
	            DIRECT bit NOT NULL,
	            STARTDATE date NOT NULL,
	            ENDDATE date NOT NULL,
	            RPTTYPE tinyint NOT NULL,
	            REFDOCID int NOT NULL,
	            DEFRRDAYS nvarchar(100) NULL,
	            COMMENT nvarchar(45) NULL, 
                CONSTRAINT FK_t_fin_account_ext_dp_id FOREIGN KEY(ACCOUNTID) REFERENCES t_fin_account(ID)
                    ON DELETE CASCADE ON UPDATE CASCADE
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
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )");

            // Finance doc. type
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_doc_type (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(30) NOT NULL,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )"
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
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )");

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
                CONSTRAINT PK_t_fin_document_item PRIMARY KEY(DOCID, ITEMID),
                CONSTRAINT FK_t_fin_document_header FOREIGN KEY (DOCID) REFERENCES t_fin_document ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
                )");

            // Order
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_order (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NOT NULL,
	            NAME nvarchar(30) NOT NULL,
	            VALID_FROM date NOT NULL,
	            VALID_TO date NOT NULL,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )");

            // Order Srule
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_order_srule (
	            ORDID int NOT NULL, 
	            RULEID int NOT NULL,
	            CONTROLCENTERID int NOT NULL,
	            PRECENT int NOT NULL,
	            COMMENT nvarchar(45) NULL,
                CONSTRAINT PK_t_fin_order PRIMARY KEY(ORDID, RULEID),
                CONSTRAINT FK_t_fin_order_srule_order FOREIGN KEY (ORDID) REFERENCES t_fin_order ([ID]) 
                    ON DELETE CASCADE ON UPDATE CASCADE
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
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE,
                CONSTRAINT FK_t_fin_tmpdoc_dp_account FOREIGN KEY (ACCOUNTID) REFERENCES T_FIN_ACCOUNT (ID) 
                    ON DELETE CASCADE ON UPDATE CASCADE
                )");

            // Tran. type
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_tran_type (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(30) NOT NULL,
	            EXPENSE bit NOT NULL,
	            PARID int NULL,
	            COMMENT nvarchar(45) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )");

            // Asset category
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_asset_ctgy (
	            ID INTEGER PRIMARY KEY AUTOINCREMENT,
	            HID int NULL,
	            NAME nvarchar(50) NOT NULL,
	            DESP nvarchar(50) NULL,
	            CREATEDBY nvarchar(40) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE )");

            // Account Extra Asset
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account_ext_as (
                ACCOUNTID   INT            NOT NULL,
                CTGYID      INT            NOT NULL,
                NAME        NVARCHAR (50)  NOT NULL,
                REFDOC_BUY  INT            NOT NULL,
                COMMENT     NVARCHAR (100) NULL,
                REFDOC_SOLD INT            NULL,
                CONSTRAINT FK_t_fin_account_ext_as_ACNTID FOREIGN KEY (ACCOUNTID) REFERENCES t_fin_account (ID)
                    ON DELETE CASCADE ON UPDATE CASCADE
                )");

            // Account Extra Loan
            database.ExecuteSqlRaw(@"CREATE TABLE t_fin_account_ext_loan (
                ACCOUNTID     INT             NOT NULL,
                STARTDATE     DATETIME        NOT NULL,
                ANNUALRATE    DECIMAL (17, 2) NULL,
                INTERESTFREE  BIT             NULL,
                REPAYMETHOD   TINYINT         NULL,
                TOTALMONTH    SMALLINT        NULL,
                REFDOCID      INT             NOT NULL,
                OTHERS        NVARCHAR (100)  NULL,
                ENDDATE       DATE            DEFAULT CURRENT_DATE,
                PAYINGACCOUNT INT             NULL,
                PARTNER       NVARCHAR (50)   NULL,
                CONSTRAINT FK_t_fin_account_ext_loan_ID FOREIGN KEY (ACCOUNTID) REFERENCES t_fin_account (ID)
                    ON DELETE CASCADE ON UPDATE CASCADE
                )");

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
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(40) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE,
                CONSTRAINT FK_t_fin_tmpdoc_loan_account FOREIGN KEY (ACCOUNTID) REFERENCES t_fin_account (ID)
                    ON DELETE CASCADE ON UPDATE CASCADE
                )");

            // DB version
            database.ExecuteSqlRaw(@"CREATE TABLE T_DBVERSION (
                VersionID    INT      PRIMARY KEY NOT NULL,
                ReleasedDate DATETIME NOT NULL,
                AppliedDate  DATETIME NOT NULL
                )");
        }

        public static void CreateDatabaseViews(DatabaseFacade database)
        {
            // View: 
            database.ExecuteSqlRaw(@"CREATE VIEW V_FIN_DOCUMENT_ITEM AS 
                WITH docitem AS (
                SELECT 
                    T_FIN_DOCUMENT_ITEM.DOCID,
                    T_FIN_DOCUMENT_ITEM.ITEMID,
		            T_FIN_DOCUMENT.HID,
		            T_FIN_DOCUMENT.TRANDATE,
		            T_FIN_DOCUMENT.DESP AS DOCDESP,
                    T_FIN_DOCUMENT_ITEM.ACCOUNTID,
                    T_FIN_DOCUMENT_ITEM.TRANTYPE,
		            T_FIN_TRAN_TYPE.NAME AS TRANTYPENAME,
		            T_FIN_TRAN_TYPE.EXPENSE AS TRANTYPE_EXP,
		            T_FIN_DOCUMENT_ITEM.USECURR2,
                    CASE WHEN T_FIN_DOCUMENT_ITEM.USECURR2 IS NULL OR T_FIN_DOCUMENT_ITEM.USECURR2 = ''
                        THEN T_FIN_DOCUMENT.TRANCURR
                        ELSE T_FIN_DOCUMENT.TRANCURR2
                    END AS TRANCURR,
                    T_FIN_DOCUMENT_ITEM.TRANAMOUNT AS TRANAMOUNT_ORG,
                    CASE
                        WHEN T_FIN_TRAN_TYPE.EXPENSE = 1 THEN T_FIN_DOCUMENT_ITEM.TRANAMOUNT * -1
                        WHEN T_FIN_TRAN_TYPE.EXPENSE = 0 THEN T_FIN_DOCUMENT_ITEM.TRANAMOUNT
                    END AS TRANAMOUNT,
                    T_FIN_DOCUMENT_ITEM.CONTROLCENTERID,
                    T_FIN_DOCUMENT_ITEM.ORDERID,
                    T_FIN_DOCUMENT_ITEM.DESP,
                    T_FIN_DOCUMENT.EXGRATE,
                    T_FIN_DOCUMENT.EXGRATE2
                FROM
                    T_FIN_DOCUMENT_ITEM
		            INNER JOIN T_FIN_TRAN_TYPE ON T_FIN_DOCUMENT_ITEM.TRANTYPE = T_FIN_TRAN_TYPE.ID
                    INNER JOIN T_FIN_DOCUMENT ON T_FIN_DOCUMENT_ITEM.DOCID = T_FIN_DOCUMENT.ID
                )
                SELECT 
                    DOCID,
                    ITEMID,
		            HID,
		            TRANDATE,
		            DOCDESP,
                    ACCOUNTID,
                    TRANTYPE,
		            TRANTYPENAME,
		            TRANTYPE_EXP,
                    TRANCURR,
                    TRANAMOUNT_ORG,
                    TRANAMOUNT,
                    CASE WHEN USECURR2 IS NULL AND EXGRATE IS NOT NULL AND EXGRATE <> 0 THEN TRANAMOUNT * EXGRATE / 100                         
                         WHEN USECURR2 IS NOT NULL AND EXGRATE2 IS NOT NULL AND EXGRATE2 <> 0 THEN TRANAMOUNT * EXGRATE2 / 100
                         ELSE TRANAMOUNT
                    END AS TRANAMOUNT_LC,
                    CONTROLCENTERID,
                    ORDERID,
                    DESP
                FROM docitem");

            // View
            database.ExecuteSqlRaw(@"CREATE VIEW V_FIN_DOCUMENT_ITEM1
                AS
                SELECT
                        V_FIN_DOCUMENT_ITEM.DOCID,
                        V_FIN_DOCUMENT_ITEM.ITEMID,
                        V_FIN_DOCUMENT_ITEM.HID,
                        V_FIN_DOCUMENT_ITEM.TRANDATE,
                        V_FIN_DOCUMENT_ITEM.DOCDESP,
                        V_FIN_DOCUMENT_ITEM.ACCOUNTID,
                        T_FIN_ACCOUNT.NAME AS ACCOUNTNAME,
                        V_FIN_DOCUMENT_ITEM.TRANTYPE,
                        V_FIN_DOCUMENT_ITEM.TRANTYPENAME,
                        V_FIN_DOCUMENT_ITEM.TRANTYPE_EXP,
                        V_FIN_DOCUMENT_ITEM.USECURR2,
                        V_FIN_DOCUMENT_ITEM.TRANCURR,
                        V_FIN_DOCUMENT_ITEM.TRANAMOUNT_ORG,
                        V_FIN_DOCUMENT_ITEM.TRANAMOUNT,
                        V_FIN_DOCUMENT_ITEM.TRANAMOUNT_LC,
                        V_FIN_DOCUMENT_ITEM.CONTROLCENTERID,
                        T_FIN_CONTROLCENTER.NAME AS CONTROLCENTERNAME,
                        V_FIN_DOCUMENT_ITEM.ORDERID,
                        T_FIN_ORDER.NAME AS ORDERNAME,
                        V_FIN_DOCUMENT_ITEM.DESP
                    FROM
                        V_FIN_DOCUMENT_ITEM
                        INNER JOIN T_FIN_ACCOUNT ON V_FIN_DOCUMENT_ITEM.ACCOUNTID = T_FIN_ACCOUNT.ID
                        LEFT OUTER JOIN T_FIN_CONTROLCENTER ON V_FIN_DOCUMENT_ITEM.CONTROLCENTERID = T_FIN_CONTROLCENTER.ID
                        LEFT OUTER JOIN T_FIN_ORDER ON V_FIN_DOCUMENT_ITEM.ORDERID = T_FIN_ORDER.ID");

            // View
            database.ExecuteSqlRaw(@"CREATE VIEW v_fin_grp_acnt
                AS
                SELECT hid,
                       accountid,
		               sum(tranamount_lc) AS balance_lc
                    from
                        v_fin_document_item
		                group by hid, accountid");

            // View
            database.ExecuteSqlRaw(@"CREATE VIEW v_fin_grp_acnt_tranexp
                AS
                SELECT hid,
                       accountid,
		               TRANTYPE_EXP,
		               sum(tranamount_lc) AS balance_lc
                    from
                        v_fin_document_item
		                group by hid, accountid, TRANTYPE_EXP");

            // View
            //database.ExecuteSqlRaw(@"CREATE VIEW v_fin_report_bs
            //    AS 
            //    SELECT tab_a.hid,
            //        tab_a.accountid,
            //        tab_a.ACCOUNTNAME,
            //        tab_a.ACCOUNTCTGYID,
            //        tab_a.ACCOUNTCTGYNAME,
            //           tab_a.balance_lc AS debit_balance,
            //           tab_b.balance_lc AS credit_balance,
            //           (tab_a.balance_lc - tab_b.balance_lc) AS balance
            //     FROM 
            //     (SELECT 
            //      t_fin_account.ID AS ACCOUNTID,
            //      t_fin_account.HID AS HID,
            //      t_fin_account.NAME AS ACCOUNTNAME,
            //      t_fin_account_ctgy.ID AS ACCOUNTCTGYID,
            //      t_fin_account_ctgy.NAME AS ACCOUNTCTGYNAME,
            //      (case
            //                when (v_fin_grp_acnt_tranexp.balance_lc is not null) then v_fin_grp_acnt_tranexp.balance_lc
            //                else 0.0
            //            end) AS balance_lc
            //     FROM t_fin_account
            //     JOIN t_fin_account_ctgy ON t_fin_account.CTGYID = t_fin_account_ctgy.ID
            //     LEFT OUTER JOIN v_fin_grp_acnt_tranexp ON t_fin_account.ID = v_fin_grp_acnt_tranexp.accountid
            //      AND v_fin_grp_acnt_tranexp.trantype_exp = 0 ) tab_a

            //     JOIN 

            //     ( SELECT t_fin_account.ID AS ACCOUNTID,
            //      t_fin_account.NAME AS ACCOUNTNAME,
            //      t_fin_account_ctgy.ID AS ACCOUNTCTGYID,
            //      t_fin_account_ctgy.NAME AS ACCOUNTCTGYNAME,
            //      (case
            //                when (v_fin_grp_acnt_tranexp.balance_lc is not null) then v_fin_grp_acnt_tranexp.balance_lc * -1
            //                else 0.0
            //            end) AS balance_lc
            //     FROM t_fin_account
            //     JOIN t_fin_account_ctgy ON t_fin_account.CTGYID = t_fin_account_ctgy.ID
            //     LEFT OUTER JOIN v_fin_grp_acnt_tranexp ON t_fin_account.ID = v_fin_grp_acnt_tranexp.accountid
            //      AND v_fin_grp_acnt_tranexp.trantype_exp = 1 ) tab_b
            //     ON tab_a.ACCOUNTID = tab_b.ACCOUNTID");
            database.ExecuteSqlRaw(@"CREATE VIEW v_fin_report_bs
                AS 
                WITH table_a AS (
                SELECT a1.hid,
                    a2.accountid,
                    case when a2.balance_lc IS null then 0.0 else a2.balance_lc end as balance
                 FROM t_fin_account as a1
                    LEFT OUTER JOIN v_fin_grp_acnt_tranexp as a2 
                    on a1.ID = a2.accountid and a2.trantype_exp = 0),

                table_b AS (
                SELECT b1.hid,
                    b2.accountid,
                    case when b2.balance_lc IS null then 0.0 else b2.balance_lc end as balance
                 FROM t_fin_account as b1
                    LEFT OUTER JOIN v_fin_grp_acnt_tranexp as b2 
                    on b1.ID = b2.accountid and b2.trantype_exp = 1)

                select table_a.hid,
                       table_a.accountid,
                       table_a.balance as debit_balance,
                       table_b.balance as credit_balance,
                       table_a.balance - table_b.balance as balance
                 from table_a inner join table_b on table_a.accountid = table_b.accountid ");
        }
        #endregion

        #region Update table with system data and configuration data
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
        #endregion

        #region Setup system data and configuration data
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
                User = UserA,
                Createdby = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User B",
                Relation = HomeMemberRelationType.Couple,
                User = UserB,
                Createdby = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User C",
                Relation = HomeMemberRelationType.Child,
                User = UserC,
                Createdby = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User D",
                Relation = HomeMemberRelationType.Child,
                User = UserD,
                Createdby = UserA
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
                User = UserB,
                Createdby = UserB
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
                User = UserA,
                Createdby = UserA
            });
            HomeMembers.Add(new HomeMember()
            {
                HomeID = Home3ID,
                DisplayAs = "User B",
                Relation = HomeMemberRelationType.Couple,
                User = UserB,
                Createdby = UserA
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
                User = UserC,
                Createdby = UserC
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
                User = UserD,
                Createdby = UserD
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
        #endregion

        #region Setup testing data
        private static List<int> NotAllowedTranTypes
        {
            get
            {
                return new List<int>
                {
                    1,
                    37,
                    60,
                    80,
                    81,
                    82,
                    86,
                    87,
                    88,
                    89,
                    90,
                    91,
                    92,
                    93,
                };
            }
        }
        /// <summary>
        /// Home 1
        ///     [Host] User A
        ///     User B
        ///     User C
        ///     User D
        /// 
        ///  Accounts:
        ///     Cash Account 1, owned by User A
        ///     Cash Account 2, owned by User B
        ///     Cash Account 3, owned by User C
        ///     Cash Account 4, owned by User D
        ///     Deposit Account 5, owned by User A
        ///     Deposit Account 6, owned by User A, Closed
        ///     Deposit Account 7, owned by User A
        ///     Deposit Account 8, owned by User B
        ///     Deposit Account 9, owned by User B
        ///     Deposit Account 10, owned by User C
        ///     Deposit Account 11, owned by User D
        ///     Creditcard Account 12, owned by User A
        ///     Creditcard Account 13, owned by User A
        ///     Creditcard Account 14, owned by User B
        ///     Creditcard Account 15, owned by User B, Closed
        ///     Creditcard Account 16, owned by User B
        ///     Virtual Account 17, owned by User A
        ///     Virtual Account 18, owned by User B
        ///     
        /// Control centers
        ///     CC 1, no owner
        ///         CC2, parent: CC1
        ///             CC3, parent CC2, owned by user A
        ///             CC4, parent CC2, owned by user B
        ///             CC5, parent CC2, no owner
        ///                 CC6, parent CC5, owned by user C
        ///                 CC7, parent CC5, owned by user D
        ///         CC8, parent: CC1, no owner
        ///        
        /// Orders
        ///     Order 1, valid from (-12 months to -6 months)
        ///         Rule: CC3 25%, CC4 25%, and CC8 50%
        ///     Order 2, valid from (-7 months to 5 months)
        ///         Rule: CC3 25%, CC4 25% and CC5 50%
        ///     Order 3, valid from (4 months to 16 months)
        ///         Rule: CC6 50%, CC7 50%
        /// 
        /// </summary>
        public static void CreateTestingData_Home1(hihDataContext db)
        {
            // Accounts
            #region Accounts
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 1,
                HomeID = Home1ID,
                Name = "Cash Account 1",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 2,
                HomeID = Home1ID,
                Name = "Cash Account 2",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 3,
                HomeID = Home1ID,
                Name = "Cash Account 3",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 4,
                HomeID = Home1ID,
                Name = "Cash Account 4",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 5,
                HomeID = Home1ID,
                Name = "Deposit Account 5",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 6,
                HomeID = Home1ID,
                Name = "Deposit Account 6",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Closed,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 7,
                HomeID = Home1ID,
                Name = "Deposit Account 7",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 8,
                HomeID = Home1ID,
                Name = "Deposit Account 8",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 9,
                HomeID = Home1ID,
                Name = "Depoist Account 9",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 10,
                HomeID = Home1ID,
                Name = "Depoist Account 10",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 11,
                HomeID = Home1ID,
                Name = "Depoist Account 11",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 12,
                HomeID = Home1ID,
                Name = "Creditcard Account 12",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 13,
                HomeID = Home1ID,
                Name = "Creditcard Account 13",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 14,
                HomeID = Home1ID,
                Name = "Creditcard Account 14",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 15,
                HomeID = Home1ID,
                Name = "Creditcard Account 15",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Closed,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 16,
                HomeID = Home1ID,
                Name = "Creditcard Account 16",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 17,
                HomeID = Home1ID,
                Name = "Virutal Account 17",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal,
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 18,
                HomeID = Home1ID,
                Name = "Virutal Account 18",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal,
            });
            #endregion
            // Control centers
            #region Control Centers
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 1,
                HomeID = Home1ID,
                Name = "CC1",
                Comment = "CC1"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 2,
                HomeID = Home1ID,
                Name = "CC2",
                ParentID = 1,
                Comment = "CC2"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 3,
                HomeID = Home1ID,
                Name = "CC3",
                ParentID = 2,
                Owner = UserA,
                Comment = "CC3"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 4,
                HomeID = Home1ID,
                Name = "CC4",
                ParentID = 2,
                Owner = UserB,
                Comment = "CC4"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 5,
                HomeID = Home1ID,
                Name = "CC5",
                ParentID = 2,
                Comment = "CC5"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 6,
                HomeID = Home1ID,
                Name = "CC6",
                ParentID = 5,
                Owner = UserC,
                Comment = "CC6"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 7,
                HomeID = Home1ID,
                Name = "CC7",
                ParentID = 5,
                Owner = UserD,
                Comment = "CC7"
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 8,
                HomeID = Home1ID,
                Name = "CC8",
                ParentID = 1,
                Comment = "CC8"
            });
            #endregion
            // Orders
            #region Orders
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 1,
                HomeID = Home1ID,
                Name = "Order 1",
                ValidFrom = DateTime.Today.AddMonths(-12),
                ValidTo = DateTime.Today.AddMonths(-6),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 3,
                        Precent = 25
                    },
                    new FinanceOrderSRule
                    {
                        RuleID = 2,
                        ControlCenterID = 4,
                        Precent = 25
                    },
                    new FinanceOrderSRule
                    {
                        RuleID = 3,
                        ControlCenterID = 8,
                        Precent = 50
                    },
                }
            });
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 2,
                HomeID = Home1ID,
                Name = "Order 2",
                ValidFrom = DateTime.Today.AddMonths(-7),
                ValidTo = DateTime.Today.AddMonths(5),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 3,
                        Precent = 25
                    },
                    new FinanceOrderSRule
                    {
                        RuleID = 2,
                        ControlCenterID = 4,
                        Precent = 25
                    },
                    new FinanceOrderSRule
                    {
                        RuleID = 3,
                        ControlCenterID = 5,
                        Precent = 50
                    },
                }
            });
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 3,
                HomeID = Home1ID,
                Name = "Order 3",
                ValidFrom = DateTime.Today.AddMonths(4),
                ValidTo = DateTime.Today.AddMonths(16),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 6,
                        Precent = 50
                    },
                    new FinanceOrderSRule
                    {
                        RuleID = 2,
                        ControlCenterID = 7,
                        Precent = 50
                    },
                }
            });
            #endregion
            // Documents
            #region Documents
            // -12 - -7
            var acnts = new int[]
            {
                1, 2, 3, 4, 5, 6, 8, 12, 14, 15
            };
            var ccs = new int[]
            {
                1, 2, 3, 4, 5, 6, 7, 8
            };
            var ords = new int[]
            {
                1
            };
            for(int i = -12; i < -7; i ++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while(dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 3);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = Home1ID,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home1BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(0, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[0];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = ccs[new Random().Next(0, ccs.Length - 1)];
                            }
                            ndoc.Items.Add(ndocitem);
                        }

                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            // -7 - -6
            ords = new int[]
            {
                1,
                2
            };
            for (int i = -7; i < -6; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 3);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = Home1ID,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home1BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[new Random().Next(0, 1)];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = ccs[new Random().Next(0, ccs.Length - 1)];
                            }
                            ndoc.Items.Add(ndocitem);
                        }

                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            // -6 - 4
            ords = new int[]
            {
                2
            };
            for (int i = -6; i < 4; i += 2)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 2);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(2);
                    var docamt = new Random().Next(0, 1);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = Home1ID,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home1BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[0];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = ccs[new Random().Next(0, ccs.Length - 1)];
                            }
                            ndoc.Items.Add(ndocitem);
                        }
                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            // 4 - 5
            ords = new int[]
            {
                2,
                3
            };
            for (int i = 4; i < 5; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(3);
                    var docamt = new Random().Next(0, 1);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = Home1ID,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home1BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[new Random().Next(0, 1)];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = ccs[new Random().Next(0, ccs.Length - 1)];
                            }
                            ndoc.Items.Add(ndocitem);
                        }
                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            // 5 - 16
            ords = new int[]
            {
                3
            };
            for (int i = 5; i < 16; i += 3)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 3);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(7);
                    var docamt = new Random().Next(0, 1);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = Home1ID,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home1BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[0];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = ccs[new Random().Next(0, ccs.Length - 1)];
                            }
                            ndoc.Items.Add(ndocitem);
                        }

                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            #endregion

            // Save it
            db.SaveChanges();
        }
        
        /// <summary>
        /// Home 2
        ///     [Host] User B
        /// 
        ///  Accounts:
        ///     Cash Account 101, owned by User B
        ///     Deposit Account 102, owned by User B
        ///     Deposit Account 103, owned by User B, Closed
        ///     Creditcard Account 104, owned by User B
        ///     Creditcard Account 105, owned by User B, Closed
        ///     Virtual Account 106, owned by User B
        ///     
        /// Control centers
        ///     CC 101, owned by user B
        ///        
        /// Orders
        ///     Order 101, valid from (-12 months to 1 months)
        ///         Rule: CC101 100%
        ///     Order 102, valid from (-1 months to 24 months)
        ///         Rule: CC101 100%
        /// 
        /// </summary>
        public static void CreateTestingData_Home2(hihDataContext db, int hid = Home2ID)
        {
            // Accounts
            #region Accounts
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 101,
                HomeID = hid,
                Name = "Cash Account 101",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 102,
                HomeID = hid,
                Name = "Deposit Account 102",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 103,
                HomeID = hid,
                Name = "Deposit Account 103",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Closed
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 104,
                HomeID = hid,
                Name = "Creditcard Account 104",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 105,
                HomeID = hid,
                Name = "Creditcard Account 105",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Closed
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 106,
                HomeID = hid,
                Name = "Virtual Account 106",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal
            });
            #endregion
            // Control centers
            #region Control Centers
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 101,
                HomeID = hid,
                Name = "CC101",
                Comment = "CC101",
                Owner = UserB
            });
            #endregion
            // Orders
            #region Orders
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 101,
                HomeID = hid,
                Name = "Order 101",
                ValidFrom = DateTime.Today.AddMonths(-12),
                ValidTo = DateTime.Today.AddDays(5),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 101,
                        Precent = 100
                    },
                }
            });
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 102,
                HomeID = hid,
                Name = "Order 102",
                ValidFrom = DateTime.Today.AddMonths(-1),
                ValidTo = DateTime.Today.AddMonths(24),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 101,
                        Precent = 100
                    },
                }
            });
            #endregion
            // Documents
            #region Documents
            // -12 - -1
            var acnts = new int[]
            {
                101, 102, 104, 106
            };
            for (int i = -12; i < -1; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 1);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = hid,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home2BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = 101;
                            }
                            else
                            {
                                ndocitem.ControlCenterID = 101;
                            }
                            ndoc.Items.Add(ndocitem);
                        }

                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            // -1 - 1
            var ords = new int[]
            {
                101,
                102
            };
            for (int i = -1; i < 1; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(3);
                    var docamt = new Random().Next(0, 1);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = hid,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home2BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[new Random().Next(0, 1)];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = 101;
                            }

                            ndoc.Items.Add(ndocitem);
                        }
                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            // 1 - 24
            ords = new int[]
            {
                102
            };
            for (int i = 1; i < 24; i += 3)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 3);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 1);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = hid,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home2BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[0];
                            }
                            else
                            {
                                ndocitem.ControlCenterID = 101;
                            }

                            ndoc.Items.Add(ndocitem);
                        }
                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            #endregion

            // Save it
            db.SaveChanges();
        }
        
        /// <summary>
        /// Home 3
        ///     [Host] User A
        ///     User B
        /// 
        ///  Accounts:
        ///     Cash Account 201, owned by User A
        ///     Cash Account 202, owned by User B
        ///     Deposit Account 203, owned by User A
        ///     Deposit Account 204, owned by User B, Closed
        ///     Deposit Account 205, owned by User B
        ///     Creditcard Account 206, owned by User A
        ///     Creditcard Account 207, owned by User B
        ///     Creditcard Account 208, owned by User B, Closed
        ///     Virtual Account 209, owned by User A
        ///     Virtual Account 210, owned by User B
        ///     
        /// Control centers
        ///     CC 201, no owner
        ///         CC 202, owned by User A
        ///         CC 203, owned by User B
        ///        
        /// Orders
        ///     Order 201, valid from (-12 months to 12 months)
        ///         Rule: CC201 100%
        ///     Order 202, valid from (-12 months to 12 months)
        ///         Rule: CC202 80%, CC203 20%
        /// 
        /// </summary>
        public static void CreateTestingData_Home3(hihDataContext db, int hid = Home3ID)
        {
            // Accounts
            #region Accounts
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 201,
                HomeID = hid,
                Name = "Cash Account 201",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 202,
                HomeID = hid,
                Name = "Deposit Account 202",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 203,
                HomeID = hid,
                Name = "Deposit Account 203",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 204,
                HomeID = hid,
                Name = "Deposit Account 204",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Closed
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 205,
                HomeID = hid,
                Name = "Deposit Account 205",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 206,
                HomeID = hid,
                Name = "Creditcard Account 206",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 207,
                HomeID = hid,
                Name = "Creditcard Account 207",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 208,
                HomeID = hid,
                Name = "Creditcard Account 208",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Closed
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 209,
                HomeID = hid,
                Name = "Virtual Account 209",
                Owner = UserA,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 210,
                HomeID = hid,
                Name = "Virtual Account 210",
                Owner = UserB,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal
            });
            #endregion
            // Control centers
            #region Control Centers
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 201,
                HomeID = hid,
                Name = "CC201",
                Comment = "CC201",
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 202,
                HomeID = hid,
                Name = "CC202",
                Comment = "CC202",
                ParentID = 201,
                Owner = UserA,
            });
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 203,
                HomeID = hid,
                Name = "CC203",
                Comment = "CC203",
                ParentID = 201,
                Owner = UserB,
            });
            #endregion
            // Orders
            #region Orders
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 201,
                HomeID = hid,
                Name = "Order 201",
                ValidFrom = DateTime.Today.AddMonths(-12),
                ValidTo = DateTime.Today.AddMonths(12),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 201,
                        Precent = 100
                    },
                }
            });
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 202,
                HomeID = hid,
                Name = "Order 202",
                ValidFrom = DateTime.Today.AddMonths(-12),
                ValidTo = DateTime.Today.AddMonths(12),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 202,
                        Precent = 80
                    },
                    new FinanceOrderSRule
                    {
                        RuleID = 2,
                        ControlCenterID = 203,
                        Precent = 20
                    },
                }
            });
            #endregion
            // Documents
            #region Documents
            // -12 - 12
            var acnts = new int[]
            {
                201, 202, 203, 205, 206, 207, 209, 210,
                // 204, 208
            };
            var ccs = new int[]
            {
                201, 202, 203,
            };
            var ords = new int[]
            {
                201,
                202
            };
            for (int i = -12; i < 0; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 2);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = hid,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home3BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while(docitemcnt -- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = ords[new Random().Next(0, ords.Length - 1)]; ;
                            }
                            else
                            {
                                ndocitem.ControlCenterID = ccs[new Random().Next(0, ccs.Length - 1)]; ;
                            }
                            ndoc.Items.Add(ndocitem);
                        }

                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            #endregion

            // Save it
            db.SaveChanges();
        }

        /// <summary>
        /// Home 4
        ///     [Host] User C
        /// 
        ///  Accounts:
        ///     Cash Account 301, owned by User C
        ///     Deposit Account 302, owned by User C
        ///     Creditcard Account 303, owned by User C
        ///     Creditcard Account 304, owned by User C, Closed
        ///     Virtual Account 305, owned by User C
        ///     
        /// Control centers
        ///     CC 301, owned by user C
        ///        
        /// Orders
        ///     Order 301, valid from (-12 months to 12 months)
        ///         Rule: CC301 100%
        ///         
        /// </summary>
        public static void CreateTestingData_Home4(hihDataContext db, int hid = Home4ID)
        {
            // Accounts
            #region Accounts
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 301,
                HomeID = hid,
                Name = "Cash Account 301",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 302,
                HomeID = hid,
                Name = "Deposit Account 302",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 303,
                HomeID = hid,
                Name = "Creditcard Account 303",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 304,
                HomeID = hid,
                Name = "Creditcard Account 304",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Closed
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 305,
                HomeID = hid,
                Name = "Virtual Account 305",
                Owner = UserC,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal
            });
            #endregion
            // Control centers
            #region Control Centers
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 301,
                HomeID = hid,
                Name = "CC301",
                Comment = "CC301",
                Owner = UserC
            });
            #endregion
            // Orders
            #region Orders
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 301,
                HomeID = hid,
                Name = "Order 301",
                ValidFrom = DateTime.Today.AddMonths(-12),
                ValidTo = DateTime.Today.AddMonths(12),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 301,
                        Precent = 100
                    },
                }
            });
            #endregion
            // Documents
            #region Documents
            // -12 - 0
            var acnts = new int[]
            {
                301, 302, 303, 305
                // 304
            };
            for (int i = -12; i < 0; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 2);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = hid,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home4BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while(docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = 301;
                            }
                            else
                            {
                                ndocitem.ControlCenterID = 301;
                            }
                            ndoc.Items.Add(ndocitem);
                        }
                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            #endregion

            // Save it
            db.SaveChanges();
        }

        /// <summary>
        /// Home 4
        ///     [Host] User D
        /// 
        ///  Accounts:
        ///     Cash Account 401, owned by User D
        ///     Deposit Account 402, owned by User D
        ///     Creditcard Account 403, owned by User D
        ///     Creditcard Account 404, owned by User D, Closed
        ///     Virtual Account 405, owned by User D
        ///     
        /// Control centers
        ///     CC 401, owned by user D
        ///        
        /// Orders
        ///     Order 401, valid from (-12 months to 12 months)
        ///         Rule: CC401 100%
        ///         
        /// </summary>
        public static void CreateTestingData_Home5(hihDataContext db, int hid = Home5ID)
        {
            // Accounts
            #region Accounts
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 401,
                HomeID = hid,
                Name = "Cash Account 401",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 402,
                HomeID = hid,
                Name = "Deposit Account 402",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Deposit,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 403,
                HomeID = hid,
                Name = "Creditcard Account 403",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Normal
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 404,
                HomeID = hid,
                Name = "Creditcard Account 404",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_Creditcard,
                Status = FinanceAccountStatus.Closed
            });
            db.FinanceAccount.Add(new FinanceAccount()
            {
                ID = 405,
                HomeID = hid,
                Name = "Virtual Account 405",
                Owner = UserD,
                CategoryID = FinanceAccountCategoriesController.AccountCategory_VirtualAccount,
                Status = FinanceAccountStatus.Normal
            });
            #endregion
            // Control centers
            #region Control Centers
            db.FinanceControlCenter.Add(new FinanceControlCenter
            {
                ID = 401,
                HomeID = hid,
                Name = "CC401",
                Comment = "CC401",
                Owner = UserD
            });
            #endregion
            // Orders
            #region Orders
            db.FinanceOrder.Add(new FinanceOrder
            {
                ID = 401,
                HomeID = hid,
                Name = "Order 401",
                ValidFrom = DateTime.Today.AddMonths(-12),
                ValidTo = DateTime.Today.AddMonths(12),
                SRule = new FinanceOrderSRule[]
                {
                    new FinanceOrderSRule
                    {
                        RuleID = 1,
                        ControlCenterID = 401,
                        Precent = 100
                    },
                }
            });
            #endregion
            // Documents
            #region Documents
            // -12 - 0
            var acnts = new int[]
            {
                401, 402, 403, 405
                // 404
            };
            for (int i = -12; i < 0; i++)
            {
                var dt1 = DateTime.Today.AddMonths(i);
                var dt2 = DateTime.Today.AddMonths(i + 1);
                while (dt1 <= dt2)
                {
                    dt1 = dt1.AddDays(1);
                    var docamt = new Random().Next(0, 2);
                    while (docamt-- > 0)
                    {
                        var ndoc = new FinanceDocument
                        {
                            TranDate = dt1,
                            HomeID = hid,
                            Desp = dt1.ToShortDateString(),
                            DocType = FinanceDocumentType.DocType_Normal,
                            TranCurr = Home5BaseCurrency,
                        };
                        var docitemcnt = new Random().Next(1, 3);
                        while (docitemcnt-- >= 0)
                        {
                            var ndocitem = new FinanceDocumentItem
                            {
                                ItemID = (docitemcnt + 2),
                                AccountID = acnts[new Random().Next(0, acnts.Length - 1)],
                                TranAmount = new decimal(new Random().NextDouble() * 100),
                            };
                            var ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            while (NotAllowedTranTypes.Exists(p => p == ttid))
                            {
                                ttid = new Random().Next(2, FinanceTransactionTypes.Count - 1);
                            };
                            ndocitem.TranType = ttid;
                            if (new Random().NextDouble() > 0.7)
                            {
                                ndocitem.OrderID = 301;
                            }
                            else
                            {
                                ndocitem.ControlCenterID = 301;
                            }
                            ndoc.Items.Add(ndocitem);
                        }
                        db.FinanceDocument.Add(ndoc);
                    }
                }
            }
            #endregion

            // Save it
            db.SaveChanges();
        }
        #endregion
    }
}
