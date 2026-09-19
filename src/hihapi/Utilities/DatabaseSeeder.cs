using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Models;
using hihapi.Models.Library;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Utilities
{
    public static class DatabaseSeeder
    {
        // ── Version-based schema upgrade ──
        // Versions 1-21 belong to the old SQL Server delta era (Sqls/Delta/v1.sql
        // ... v21.sql, frozen 2022-09-30; never executed against SQLite). They are
        // frozen history: a database whose T_DBVERSION table is still empty is
        // baselined at LegacyBaselineVersion, after which every registered step
        // above the stored maximum runs exactly once and stamps its version row.
        // A schema change therefore means: add an idempotent step below and bump
        // CurrentVersion - no delta script, anywhere.
        // A throwing step fails startup before the API begins listening (fail
        // fast); steps must be idempotent because a crash between a step and its
        // version row makes the next start re-run it.
        internal const Int32 LegacyBaselineVersion = 21;
        public const Int32 CurrentVersion = 22;

        private static readonly DateTime LegacyBaselineReleasedDate = new(2022, 9, 30);

        internal readonly record struct SchemaUpgrade(Int32 Version, DateTime ReleasedDate, String Description, Action<hihDataContext> Apply);

        internal static readonly SchemaUpgrade[] SchemaUpgrades =
        {
            new(22, new DateTime(2026, 9, 2), "Book reading records: table + STATUS lifecycle column",
                EnsureReadingRecordSchema),
        };

        public static async Task SeedAsync(hihDataContext context)
        {
            var freshDatabase = await context.Database.EnsureCreatedAsync();

            ApplySchemaVersions(context, freshDatabase);
            SeedViews(context);
            SeedCurrencies(context);
            SeedLanguages(context);
            SeedFinanceAccountCategories(context);
            SeedFinanceAssetCategories(context);
            SeedFinanceDocumentTypes(context);
            SeedFinanceTransactionTypes(context);
            SeedLibraryPersonRoles(context);
            SeedLibraryOrganizationTypes(context);
            SeedLibraryBookCategories(context);

            await context.SaveChangesAsync();

            // Must run after SaveChanges: the seed inserts above are what bring
            // sqlite_sequence (and its per-table rows) into existence.
            ReserveCustomerIdRanges(context);
        }

        // Compare the stored version against the code's CurrentVersion and run every
        // missing upgrade step in order, one T_DBVERSION row stamped per version.
        private static void ApplySchemaVersions(hihDataContext context, Boolean freshDatabase)
        {
            var applied = context.DBVersions.Select(v => v.VersionID).ToList();
            if (applied.Count == 0)
            {
                // Fresh: EnsureCreatedAsync just built the CURRENT schema from the
                // model, so stamp it straight away - no historical step needs to run.
                // Existing but unversioned: a pre-upgrade install. Baseline it at the
                // last delta-era version and let the registered steps catch it up.
                var baseline = freshDatabase ? CurrentVersion : LegacyBaselineVersion;
                var baselineReleased = !freshDatabase ? LegacyBaselineReleasedDate
                    : SchemaUpgrades.Length > 0 ? SchemaUpgrades[^1].ReleasedDate
                    : DateTime.Today;
                context.DBVersions.Add(new DBVersion
                {
                    VersionID = baseline,
                    ReleasedDate = baselineReleased,
                    AppliedDate = DateTime.Today,
                });
                applied.Add(baseline);
                context.SaveChanges();
            }

            var maxApplied = applied.Max();
            foreach (var upgrade in SchemaUpgrades.Where(u => u.Version > maxApplied).OrderBy(u => u.Version))
            {
                upgrade.Apply(context);

                // Stamp right behind the step; a crash in between re-runs the
                // (idempotent) step on the next start instead of skipping it.
                context.DBVersions.Add(new DBVersion
                {
                    VersionID = upgrade.Version,
                    ReleasedDate = upgrade.ReleasedDate,
                    AppliedDate = DateTime.Today,
                });
                context.SaveChanges();
            }
        }

        // v22 - tables added after the first deployment. On a fresh database
        // EnsureCreatedAsync() has already created this table from the
        // [Table]/[Column] annotations and IF NOT EXISTS makes this a no-op
        // (SQLite matches object names case-insensitively).
        private static void EnsureReadingRecordSchema(hihDataContext context)
        {
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS T_LIB_BOOK_READING_RECORD (
                ID          INTEGER PRIMARY KEY AUTOINCREMENT,
                HID         INTEGER       NOT NULL,
                BOOK_ID     INTEGER       NOT NULL,
                USER        NVARCHAR(40)  NOT NULL,
                FROMDATE    DATE          NULL,
                TODATE      DATE          NULL,
                COMMENT     NVARCHAR(50)  NULL,
                STATUS      INTEGER       NOT NULL DEFAULT 0,
                CREATEDBY   NVARCHAR(40)  NULL,
                CREATEDAT   DATE          NULL DEFAULT CURRENT_DATE,
                UPDATEDBY   NVARCHAR(40)  NULL,
                UPDATEDAT   DATE          NULL DEFAULT CURRENT_DATE
            )");

            // STATUS added after the reading-lifecycle feature: EnsureCreatedAsync never
            // alters an existing table, so upgrade old hih.db files idempotently.
            // DEFAULT 0 is the enum's Reading value - the least-damaging state for any
            // row that forgets STATUS (an open record can still be finalized or aborted).
            if (!ColumnExists(context, "T_LIB_BOOK_READING_RECORD", "STATUS"))
            {
                context.Database.ExecuteSqlRaw(
                    @"ALTER TABLE T_LIB_BOOK_READING_RECORD ADD COLUMN STATUS INTEGER NOT NULL DEFAULT 0");

                // Backfill the lifecycle state of legacy rows: the former IsValid
                // required both dates, so TODATE IS NOT NULL -> Completed. The few
                // NULL-ToDate rows from the era before dates became required stay
                // open (Reading); they carry a NULL FromDate and are inert.
                context.Database.ExecuteSqlRaw(
                    @"UPDATE T_LIB_BOOK_READING_RECORD SET STATUS = 1 WHERE TODATE IS NOT NULL");
            }
        }

        // pragma_table_info() is SQLite's table-valued form of PRAGMA table_info and
        // accepts the table name as a bound parameter; column names compare
        // case-insensitively via UPPER() on both sides.
        private static bool ColumnExists(hihDataContext context, String table, String column)
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(1) FROM pragma_table_info($t) WHERE UPPER(name) = UPPER($c)";
            var pt = cmd.CreateParameter();
            pt.ParameterName = "$t";
            pt.Value = table;
            cmd.Parameters.Add(pt);
            var pc = cmd.CreateParameter();
            pc.ParameterName = "$c";
            pc.Value = column;
            cmd.Parameters.Add(pc);

            return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
        }

        // ── ID-range separation (system-delivered vs. customer-created) ──
        // System configuration rows carry explicit IDs below this boundary;
        // customer-created rows take IDs from the table's SQLite AUTOINCREMENT
        // sequence. Raising every configuration table's sequence to
        // CustomerIdRangeStart - 1 makes the next GENERATED row land AT the
        // boundary, so from this bump onward the two populations no longer share
        // the generated ID space (the race the historical v7/v10 account-category
        // deltas had to walk). Monotone (only ever raises) and idempotent, so
        // running it on every startup lets existing databases adopt the
        // separation without a delta script.
        //
        // CAUTION — the bump protects only ids generated AFTER the first run;
        // home rows created on an existing database before it keep their low
        // ids. So a NEW system row is never safe to append at a "free" low id
        // directly: it can collide with a pre-bump home row that already owns
        // that id. Delivering a new system row therefore requires a
        // SchemaUpgrade step that inserts per-row only where the id is still
        // unclaimed (check-then-insert against the live table) - the Any()-
        // guarded Seed* methods skip existing databases entirely and are not a
        // delivery path. The HID column (null = system) remains the
        // authoritative system/home marker everywhere.
        internal const int CustomerIdRangeStart = 1000;

        // [Table] names of the seeded configuration tables whose rows homes may
        // create (the Any()-guarded Seed* methods above — currencies and
        // languages are system-only and need no separation). Comparisons run
        // COLLATE NOCASE because the hand-written schemas created some tables
        // with different casing than the entity attributes.
        internal static readonly string[] CustomerBoundedConfigTables =
        {
            "T_FIN_ACCOUNT_CTGY",
            "T_FIN_ASSET_CTGY",
            "T_FIN_DOC_TYPE",
            "T_FIN_TRAN_TYPE",
            "t_lib_personrole_def",
            "T_LIB_ORGTYPE_DEF",
            "T_LIB_BOOKCTGY_DEF",
        };

        private static void ReserveCustomerIdRanges(hihDataContext context)
        {
            if (!TableExists(context, "sqlite_sequence"))
            {
                // sqlite_sequence is created lazily by SQLite on the first insert
                // into an AUTOINCREMENT table; with no sequence table there is
                // nothing to raise. Unreachable today (the seed inserts above always
                // run first) — purely defensive against future ordering changes.
                return;
            }

            var boundary = CustomerIdRangeStart - 1;
            foreach (var table in CustomerBoundedConfigTables)
            {
                // {0}/{1}/{2} tokens are turned into DbParameters by ExecuteSqlRaw —
                // no value is ever concatenated into the SQL text.
                var raised = context.Database.ExecuteSqlRaw(
                    @"UPDATE sqlite_sequence SET seq = CASE WHEN seq < {0} THEN {1} ELSE seq END
                      WHERE name = {2} COLLATE NOCASE",
                    boundary, boundary, table);
                if (raised == 0)
                {
                    // The sequence row can only be missing if this table was never
                    // inserted into; create it at the boundary so the first
                    // generated ID lands exactly on CustomerIdRangeStart.
                    context.Database.ExecuteSqlRaw(
                        "INSERT INTO sqlite_sequence (name, seq) VALUES ({0}, {1})",
                        table, boundary);
                }
            }
        }

        // sqlite_master lookup (same raw-ADO shape as ColumnExists above).
        private static bool TableExists(hihDataContext context, String name)
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = $n";
            var p = cmd.CreateParameter();
            p.ParameterName = "$n";
            p.Value = name;
            cmd.Parameters.Add(p);

            return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
        }

        private static void SeedViews(hihDataContext context)
        {
            // Views must be created after EnsureCreated (which only creates tables).
            // Each view is dropped then recreated for idempotency.
            var db = context.Database;

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_DOCUMENT_ITEM");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_DOCUMENT_ITEM AS
                SELECT DI.DOCID, DI.ITEMID, D.HID, D.TRANDATE, D.DESP AS DOCDESP,
                    DI.ACCOUNTID, DI.TRANTYPE, TT.NAME AS TRANTYPENAME, TT.EXPENSE AS TRANTYPE_EXP,
                    DI.USECURR2,
                    CASE WHEN (DI.USECURR2 IS NULL OR DI.USECURR2 = '') THEN D.TRANCURR ELSE D.TRANCURR2 END AS TRANCURR,
                    DI.TRANAMOUNT AS TRANAMOUNT_ORG,
                    CASE WHEN TT.EXPENSE = 1 THEN DI.TRANAMOUNT * -1 ELSE DI.TRANAMOUNT END AS TRANAMOUNT,
                    CASE WHEN (DI.USECURR2 IS NULL OR DI.USECURR2 = '')
                        THEN CASE WHEN D.EXGRATE IS NOT NULL
                            THEN CASE WHEN TT.EXPENSE = 1 THEN DI.TRANAMOUNT * D.EXGRATE / 100 * -1 ELSE DI.TRANAMOUNT * D.EXGRATE / 100 END
                            ELSE CASE WHEN TT.EXPENSE = 1 THEN DI.TRANAMOUNT * -1 ELSE DI.TRANAMOUNT END END
                        ELSE CASE WHEN D.EXGRATE2 IS NOT NULL
                            THEN CASE WHEN TT.EXPENSE = 1 THEN DI.TRANAMOUNT * D.EXGRATE2 / 100 * -1 ELSE DI.TRANAMOUNT * D.EXGRATE2 / 100 END
                            ELSE CASE WHEN TT.EXPENSE = 1 THEN DI.TRANAMOUNT * -1 ELSE DI.TRANAMOUNT END END
                    END AS TRANAMOUNT_LC,
                    DI.CONTROLCENTERID, DI.ORDERID, DI.DESP
                FROM T_FIN_DOCUMENT_ITEM DI
                JOIN T_FIN_TRAN_TYPE TT ON DI.TRANTYPE = TT.ID
                LEFT OUTER JOIN T_FIN_DOCUMENT D ON DI.DOCID = D.ID");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_GRP_ACNT");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_GRP_ACNT AS
                SELECT V.HID, V.ACCOUNTID, A.NAME AS ACCOUNTNAME,
                    CASE WHEN V.TRANTYPE_EXP = 1 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS OUTAMOUNT,
                    CASE WHEN V.TRANTYPE_EXP = 0 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS INAMOUNT
                FROM V_FIN_DOCUMENT_ITEM V
                JOIN T_FIN_ACCOUNT A ON V.ACCOUNTID = A.ID
                GROUP BY V.HID, V.ACCOUNTID, A.NAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_GRP_ACNT_TRANEXP");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_GRP_ACNT_TRANEXP AS
                SELECT V.HID, V.ACCOUNTID, A.NAME AS ACCOUNTNAME, V.TRANTYPE, V.TRANTYPENAME,
                    CASE WHEN V.TRANTYPE_EXP = 1 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS OUTAMOUNT,
                    CASE WHEN V.TRANTYPE_EXP = 0 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS INAMOUNT
                FROM V_FIN_DOCUMENT_ITEM V
                JOIN T_FIN_ACCOUNT A ON V.ACCOUNTID = A.ID
                GROUP BY V.HID, V.ACCOUNTID, A.NAME, V.TRANTYPE, V.TRANTYPENAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_REPORT_BS");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_REPORT_BS AS
                SELECT HID, ACCOUNTID, ACCOUNTNAME,
                    SUM(INAMOUNT) AS DEBITBALANCE, SUM(OUTAMOUNT) AS CREDITBALANCE,
                    SUM(INAMOUNT) + SUM(OUTAMOUNT) AS BALANCE
                FROM V_FIN_GRP_ACNT
                GROUP BY HID, ACCOUNTID, ACCOUNTNAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_GRP_CC");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_GRP_CC AS
                SELECT V.HID, V.CONTROLCENTERID, CC.NAME AS CCNAME,
                    CASE WHEN V.TRANTYPE_EXP = 1 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS OUTAMOUNT,
                    CASE WHEN V.TRANTYPE_EXP = 0 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS INAMOUNT
                FROM V_FIN_DOCUMENT_ITEM V
                JOIN T_FIN_CONTROLCENTER CC ON V.CONTROLCENTERID = CC.ID
                WHERE V.CONTROLCENTERID IS NOT NULL
                GROUP BY V.HID, V.CONTROLCENTERID, CC.NAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_GRP_CC_TRANEXP");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_GRP_CC_TRANEXP AS
                SELECT V.HID, V.CONTROLCENTERID, CC.NAME AS CCNAME, V.TRANTYPE, V.TRANTYPENAME,
                    CASE WHEN V.TRANTYPE_EXP = 1 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS OUTAMOUNT,
                    CASE WHEN V.TRANTYPE_EXP = 0 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS INAMOUNT
                FROM V_FIN_DOCUMENT_ITEM V
                JOIN T_FIN_CONTROLCENTER CC ON V.CONTROLCENTERID = CC.ID
                WHERE V.CONTROLCENTERID IS NOT NULL
                GROUP BY V.HID, V.CONTROLCENTERID, CC.NAME, V.TRANTYPE, V.TRANTYPENAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_REPORT_CC");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_REPORT_CC AS
                SELECT HID, CONTROLCENTERID, CCNAME,
                    SUM(INAMOUNT) AS DEBITBALANCE, SUM(OUTAMOUNT) AS CREDITBALANCE,
                    SUM(INAMOUNT) + SUM(OUTAMOUNT) AS BALANCE
                FROM V_FIN_GRP_CC
                GROUP BY HID, CONTROLCENTERID, CCNAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_GRP_ORD");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_GRP_ORD AS
                SELECT V.HID, V.ORDERID, O.NAME AS ORDERNAME,
                    CASE WHEN V.TRANTYPE_EXP = 1 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS OUTAMOUNT,
                    CASE WHEN V.TRANTYPE_EXP = 0 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS INAMOUNT
                FROM V_FIN_DOCUMENT_ITEM V
                JOIN T_FIN_ORDER O ON V.ORDERID = O.ID
                WHERE V.ORDERID IS NOT NULL
                GROUP BY V.HID, V.ORDERID, O.NAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_GRP_ORD_TRANEXP");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_GRP_ORD_TRANEXP AS
                SELECT V.HID, V.ORDERID, O.NAME AS ORDERNAME, V.TRANTYPE, V.TRANTYPENAME,
                    CASE WHEN V.TRANTYPE_EXP = 1 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS OUTAMOUNT,
                    CASE WHEN V.TRANTYPE_EXP = 0 THEN SUM(V.TRANAMOUNT_LC) ELSE 0 END AS INAMOUNT
                FROM V_FIN_DOCUMENT_ITEM V
                JOIN T_FIN_ORDER O ON V.ORDERID = O.ID
                WHERE V.ORDERID IS NOT NULL
                GROUP BY V.HID, V.ORDERID, O.NAME, V.TRANTYPE, V.TRANTYPENAME");

            db.ExecuteSqlRaw("DROP VIEW IF EXISTS V_FIN_REPORT_ORDER");
            db.ExecuteSqlRaw(@"CREATE VIEW V_FIN_REPORT_ORDER AS
                SELECT HID, ORDERID, ORDERNAME,
                    SUM(INAMOUNT) AS DEBITBALANCE, SUM(OUTAMOUNT) AS CREDITBALANCE,
                    SUM(INAMOUNT) + SUM(OUTAMOUNT) AS BALANCE
                FROM V_FIN_GRP_ORD
                GROUP BY HID, ORDERID, ORDERNAME");
        }

        private static void SeedCurrencies(hihDataContext context)
        {
            if (context.Currencies.Any()) return;
            context.Currencies.AddRange(
                new Currency { Curr = "CNY", Name = "Sys.Currency.CNY", Symbol = "¥" },
                new Currency { Curr = "EUR", Name = "Sys.Currency.EUR", Symbol = "€" },
                new Currency { Curr = "HKD", Name = "Sys.Currency.HKD", Symbol = "HK$" },
                new Currency { Curr = "JPY", Name = "Sys.Currency.JPY", Symbol = "¥" },
                new Currency { Curr = "KRW", Name = "Sys.Currency.KRW", Symbol = "₩" },
                new Currency { Curr = "TWD", Name = "Sys.Currency.TWD", Symbol = "TW$" },
                new Currency { Curr = "USD", Name = "Sys.Currency.USD", Symbol = "$" }
            );
        }

        private static void SeedLanguages(hihDataContext context)
        {
            if (context.Languages.Any()) return;
            context.Languages.AddRange(
                new Language { Lcid = 4, ISOName = "zh-Hans", EnglishName = "Chinese (Simplified)", NativeName = "简体中文", AppFlag = true },
                new Language { Lcid = 9, ISOName = "en", EnglishName = "English", NativeName = "English", AppFlag = true },
                new Language { Lcid = 17, ISOName = "ja", EnglishName = "Japanese", NativeName = "日本语", AppFlag = false },
                new Language { Lcid = 31748, ISOName = "zh-Hant", EnglishName = "Chinese (Traditional)", NativeName = "繁體中文", AppFlag = false }
            );
        }

        private static void SeedFinanceAccountCategories(hihDataContext context)
        {
            if (context.FinAccountCategories.Any()) return;
            context.FinAccountCategories.AddRange(
                new FinanceAccountCategory { ID = 1, Name = "Sys.AcntCty.Cash", AssetFlag = true },
                new FinanceAccountCategory { ID = 2, Name = "Sys.AcntCty.DepositAccount", AssetFlag = true },
                new FinanceAccountCategory { ID = 3, Name = "Sys.AcntCty.CreditCard", AssetFlag = false },
                new FinanceAccountCategory { ID = 4, Name = "Sys.AcntCty.AccountPayable", AssetFlag = false },
                new FinanceAccountCategory { ID = 5, Name = "Sys.AcntCty.AccountReceviable", AssetFlag = true },
                new FinanceAccountCategory { ID = 6, Name = "Sys.AcntCty.VirtualAccount", AssetFlag = true, Comment = "如支付宝等" },
                new FinanceAccountCategory { ID = 7, Name = "Sys.AcntCty.AssetAccount", AssetFlag = true },
                new FinanceAccountCategory { ID = 8, Name = "Sys.AcntCty.AdvancedPayment", AssetFlag = true },
                new FinanceAccountCategory { ID = 9, Name = "Sys.AcntCty.BorrowFrom", AssetFlag = false, Comment = "借入款、贷款" },
                new FinanceAccountCategory { ID = 10, Name = "Sys.AcntCty.LendTo", AssetFlag = true, Comment = "借出款" },
                new FinanceAccountCategory { ID = 11, Name = "Sys.AcntCty.AdvancedRecv", AssetFlag = false, Comment = "预收款" },
                new FinanceAccountCategory { ID = 12, Name = "Sys.AcntCty.Insurance", AssetFlag = true, Comment = "保险" }
            );
        }

        private static void SeedFinanceAssetCategories(hihDataContext context)
        {
            if (context.FinAssetCategories.Any()) return;
            context.FinAssetCategories.AddRange(
                new FinanceAssetCategory { ID = 1, Name = "Sys.AssCtgy.Apartment", Desp = "公寓" },
                new FinanceAssetCategory { ID = 2, Name = "Sys.AssCtgy.Automobile", Desp = "机动车" },
                new FinanceAssetCategory { ID = 3, Name = "Sys.AssCtgy.Furniture", Desp = "家具" },
                new FinanceAssetCategory { ID = 4, Name = "Sys.AssCtgy.HouseAppliances", Desp = "家用电器" },
                new FinanceAssetCategory { ID = 5, Name = "Sys.AssCtgy.Camera", Desp = "相机" },
                new FinanceAssetCategory { ID = 6, Name = "Sys.AssCtgy.Computer", Desp = "计算机" },
                new FinanceAssetCategory { ID = 7, Name = "Sys.AssCtgy.MobileDevice", Desp = "移动设备" }
            );
        }

        private static void SeedFinanceDocumentTypes(hihDataContext context)
        {
            if (context.FinDocumentTypes.Any()) return;
            context.FinDocumentTypes.AddRange(
                new FinanceDocumentType { ID = 1, Name = "Sys.DocTy.Normal", Comment = "普通" },
                new FinanceDocumentType { ID = 2, Name = "Sys.DocTy.Transfer", Comment = "转账" },
                new FinanceDocumentType { ID = 3, Name = "Sys.DocTy.CurrExg", Comment = "兑换不同的货币" },
                new FinanceDocumentType { ID = 4, Name = "Sys.DocTy.Installment", Comment = "分期付款" },
                new FinanceDocumentType { ID = 5, Name = "Sys.DocTy.AdvancedPayment", Comment = "预付款" },
                new FinanceDocumentType { ID = 6, Name = "Sys.DocTy.CreditCardRepay", Comment = "信用卡还款" },
                new FinanceDocumentType { ID = 7, Name = "Sys.DocTy.AssetBuyIn", Comment = "购入资产或大件家用器具" },
                new FinanceDocumentType { ID = 8, Name = "Sys.DocTy.AssetSoldOut", Comment = "出售资产或大件家用器具" },
                new FinanceDocumentType { ID = 9, Name = "Sys.DocTy.BorrowFrom", Comment = "借款、贷款等" },
                new FinanceDocumentType { ID = 10, Name = "Sys.DocTy.LendTo", Comment = "借出款" },
                new FinanceDocumentType { ID = 11, Name = "Sys.DocTy.Repay", Comment = "借款、贷款等" },
                new FinanceDocumentType { ID = 12, Name = "Sys.DocTy.AdvancedRecv", Comment = "预收款" },
                new FinanceDocumentType { ID = 13, Name = "Sys.DocTy.AssetValChg", Comment = "资产净值变动" },
                new FinanceDocumentType { ID = 14, Name = "Sys.DocTy.Insurance", Comment = "保险" },
                new FinanceDocumentType { ID = 15, Name = "Sys.DocTy.AssetDeprec", Comment = "资产折旧" }
            );
        }

        private static void SeedFinanceTransactionTypes(hihDataContext context)
        {
            if (context.FinTransactionType.Any()) return;
            context.FinTransactionType.AddRange(
                // Income: 主业收入
                new FinanceTransactionType { ID = 2, Name = "主业收入", Expense = false, ParID = null, Comment = "主业收入" },
                new FinanceTransactionType { ID = 3, Name = "工资", Expense = false, ParID = 2, Comment = "工资" },
                new FinanceTransactionType { ID = 4, Name = "奖金", Expense = false, ParID = 2, Comment = "奖金" },
                new FinanceTransactionType { ID = 35, Name = "津贴", Expense = false, ParID = 2, Comment = "津贴类，如加班等" },
                // Income: 投资、保险、博彩类收入
                new FinanceTransactionType { ID = 5, Name = "投资、保险、博彩类收入", Expense = false, ParID = null, Comment = "投资、保险、博彩类收入" },
                new FinanceTransactionType { ID = 6, Name = "股票收益", Expense = false, ParID = 5, Comment = "股票收益" },
                new FinanceTransactionType { ID = 7, Name = "基金收益", Expense = false, ParID = 5, Comment = "基金收益" },
                new FinanceTransactionType { ID = 8, Name = "利息收入", Expense = false, ParID = 5, Comment = "银行利息收入" },
                new FinanceTransactionType { ID = 13, Name = "彩票收益", Expense = false, ParID = 5, Comment = "彩票中奖类收益" },
                new FinanceTransactionType { ID = 36, Name = "保险报销收入", Expense = false, ParID = 5, Comment = "保险报销收入" },
                new FinanceTransactionType { ID = 84, Name = "房租收入", Expense = false, ParID = 5, Comment = "房租收入" },
                new FinanceTransactionType { ID = 87, Name = "借贷还款收入", Expense = false, ParID = 5, Comment = "借贷还款收入" },
                new FinanceTransactionType { ID = 90, Name = "资产增值", Expense = false, ParID = 5, Comment = "资产增值" },
                new FinanceTransactionType { ID = 93, Name = "资产出售收益", Expense = false, ParID = 5, Comment = "资产出售收益" },
                // Income: 其它收入
                new FinanceTransactionType { ID = 10, Name = "其它收入", Expense = false, ParID = null, Comment = "其它收入" },
                new FinanceTransactionType { ID = 1, Name = "起始资金", Expense = false, ParID = 10, Comment = "起始资金" },
                new FinanceTransactionType { ID = 37, Name = "转账收入", Expense = false, ParID = 10, Comment = "转账收入" },
                new FinanceTransactionType { ID = 80, Name = "贷款入账", Expense = false, ParID = 10, Comment = "贷款入账" },
                new FinanceTransactionType { ID = 91, Name = "预收款收入", Expense = false, ParID = 10, Comment = "预收款收入" },
                // Income: 人情交往类
                new FinanceTransactionType { ID = 30, Name = "人情交往类", Expense = false, ParID = null, Comment = "人情交往类" },
                new FinanceTransactionType { ID = 33, Name = "红包收入", Expense = false, ParID = 30, Comment = "红包收入" },
                // Expense: 生活类开支
                new FinanceTransactionType { ID = 9, Name = "生活类开支", Expense = true, ParID = null, Comment = "生活类开支" },
                new FinanceTransactionType { ID = 11, Name = "物业类支出", Expense = true, ParID = 9, Comment = "物业类支出" },
                new FinanceTransactionType { ID = 14, Name = "小区物业费", Expense = true, ParID = 11, Comment = "小区物业费" },
                new FinanceTransactionType { ID = 15, Name = "水费", Expense = true, ParID = 11, Comment = "水费" },
                new FinanceTransactionType { ID = 16, Name = "电费", Expense = true, ParID = 11, Comment = "电费" },
                new FinanceTransactionType { ID = 17, Name = "天然气费", Expense = true, ParID = 11, Comment = "天然气费" },
                new FinanceTransactionType { ID = 18, Name = "物业维修费", Expense = true, ParID = 11, Comment = "物业维修费" },
                new FinanceTransactionType { ID = 26, Name = "通讯费", Expense = true, ParID = 9, Comment = "通讯费" },
                new FinanceTransactionType { ID = 27, Name = "固定电话/宽带", Expense = true, ParID = 26, Comment = "固定电话/宽带" },
                new FinanceTransactionType { ID = 28, Name = "手机费", Expense = true, ParID = 26, Comment = "手机费" },
                new FinanceTransactionType { ID = 38, Name = "衣服饰品", Expense = true, ParID = 9, Comment = "衣服饰品" },
                new FinanceTransactionType { ID = 39, Name = "食品酒水", Expense = true, ParID = 9, Comment = "食品酒水" },
                new FinanceTransactionType { ID = 40, Name = "衣服鞋帽", Expense = true, ParID = 38, Comment = "衣服鞋帽" },
                new FinanceTransactionType { ID = 41, Name = "化妆饰品", Expense = true, ParID = 38, Comment = "化妆饰品" },
                new FinanceTransactionType { ID = 42, Name = "水果类", Expense = true, ParID = 39, Comment = "水果类" },
                new FinanceTransactionType { ID = 43, Name = "零食类", Expense = true, ParID = 39, Comment = "零食类" },
                new FinanceTransactionType { ID = 44, Name = "烟酒茶类", Expense = true, ParID = 39, Comment = "烟酒茶类" },
                new FinanceTransactionType { ID = 45, Name = "咖啡外卖类", Expense = true, ParID = 39, Comment = "咖啡外卖类" },
                new FinanceTransactionType { ID = 46, Name = "早中晚餐", Expense = true, ParID = 39, Comment = "早中晚餐" },
                new FinanceTransactionType { ID = 49, Name = "休闲娱乐", Expense = true, ParID = 9, Comment = "休闲娱乐" },
                new FinanceTransactionType { ID = 50, Name = "旅游度假", Expense = true, ParID = 49, Comment = "旅游度假" },
                new FinanceTransactionType { ID = 51, Name = "电影演出", Expense = true, ParID = 49, Comment = "电影演出" },
                new FinanceTransactionType { ID = 52, Name = "摄影外拍类", Expense = true, ParID = 49, Comment = "摄影外拍类" },
                new FinanceTransactionType { ID = 53, Name = "腐败聚会类", Expense = true, ParID = 49, Comment = "腐败聚会类" },
                new FinanceTransactionType { ID = 54, Name = "学习进修", Expense = true, ParID = 9, Comment = "学习进修" },
                new FinanceTransactionType { ID = 58, Name = "书刊杂志", Expense = true, ParID = 54, Comment = "书刊杂志" },
                new FinanceTransactionType { ID = 59, Name = "培训进修", Expense = true, ParID = 54, Comment = "培训进修" },
                new FinanceTransactionType { ID = 61, Name = "日常用品", Expense = true, ParID = 9, Comment = "日常用品" },
                new FinanceTransactionType { ID = 62, Name = "日用品", Expense = true, ParID = 61, Comment = "日用品" },
                new FinanceTransactionType { ID = 63, Name = "电子产品类", Expense = true, ParID = 61, Comment = "电子产品类" },
                new FinanceTransactionType { ID = 64, Name = "厨房用具", Expense = true, ParID = 61, Comment = "厨房用具" },
                new FinanceTransactionType { ID = 65, Name = "洗涤用品", Expense = true, ParID = 61, Comment = "洗涤用品" },
                new FinanceTransactionType { ID = 66, Name = "大家电类", Expense = true, ParID = 61, Comment = "大家电类" },
                new FinanceTransactionType { ID = 67, Name = "保健护理用品", Expense = true, ParID = 61, Comment = "保健护理用品" },
                new FinanceTransactionType { ID = 68, Name = "喂哺用品", Expense = true, ParID = 61, Comment = "喂哺用品" },
                new FinanceTransactionType { ID = 79, Name = "有线电视费", Expense = true, ParID = 11, Comment = "有线电视费" },
                new FinanceTransactionType { ID = 85, Name = "房租支出", Expense = true, ParID = 11, Comment = "房租支出" },
                // Expense: 私家车支出
                new FinanceTransactionType { ID = 12, Name = "私家车支出", Expense = true, ParID = null, Comment = "私家车支出" },
                new FinanceTransactionType { ID = 19, Name = "车辆保养", Expense = true, ParID = 12, Comment = "车辆保养" },
                new FinanceTransactionType { ID = 20, Name = "汽油费", Expense = true, ParID = 12, Comment = "汽油费" },
                new FinanceTransactionType { ID = 21, Name = "车辆保险费", Expense = true, ParID = 12, Comment = "车辆保险费" },
                new FinanceTransactionType { ID = 22, Name = "停车费", Expense = true, ParID = 12, Comment = "停车费" },
                new FinanceTransactionType { ID = 23, Name = "车辆维修", Expense = true, ParID = 12, Comment = "车辆维修" },
                new FinanceTransactionType { ID = 57, Name = "违章付款类", Expense = true, ParID = 12, Comment = "违章付款类" },
                // Expense: 其它支出
                new FinanceTransactionType { ID = 24, Name = "其它支出", Expense = true, ParID = null, Comment = "其它支出" },
                new FinanceTransactionType { ID = 82, Name = "起始负债", Expense = true, ParID = 24, Comment = "起始负债" },
                new FinanceTransactionType { ID = 60, Name = "转账支出", Expense = true, ParID = 24, Comment = "转账支出" },
                new FinanceTransactionType { ID = 81, Name = "借出款项", Expense = true, ParID = 24, Comment = "借出款项" },
                new FinanceTransactionType { ID = 88, Name = "预付款支出", Expense = true, ParID = 24, Comment = "预付款支出" },
                // Expense: 投资、保险、博彩类支出
                new FinanceTransactionType { ID = 25, Name = "投资、保险、博彩类支出", Expense = true, ParID = null, Comment = "投资、保险、博彩类支出" },
                new FinanceTransactionType { ID = 29, Name = "彩票支出", Expense = true, ParID = 25, Comment = "彩票投注等支出" },
                new FinanceTransactionType { ID = 34, Name = "保单投保、续保支出", Expense = true, ParID = 25, Comment = "保单投保、续保支出" },
                new FinanceTransactionType { ID = 55, Name = "银行利息支出", Expense = true, ParID = 25, Comment = "银行利息支出" },
                new FinanceTransactionType { ID = 56, Name = "银行手续费支出", Expense = true, ParID = 25, Comment = "银行手续费支出" },
                new FinanceTransactionType { ID = 83, Name = "投资手续费支出", Expense = true, ParID = 25, Comment = "投资手续费支出" },
                new FinanceTransactionType { ID = 86, Name = "偿还借贷款", Expense = true, ParID = 25, Comment = "偿还借贷款" },
                new FinanceTransactionType { ID = 89, Name = "资产减值", Expense = true, ParID = 25, Comment = "资产减值" },
                new FinanceTransactionType { ID = 92, Name = "资产出售费用", Expense = true, ParID = 25, Comment = "资产出售费用" },
                // Expense: 人际交往
                new FinanceTransactionType { ID = 31, Name = "人际交往", Expense = true, ParID = null, Comment = "人际交往" },
                new FinanceTransactionType { ID = 32, Name = "红包支出", Expense = true, ParID = 31, Comment = "红包支出" },
                new FinanceTransactionType { ID = 47, Name = "请客送礼", Expense = true, ParID = 31, Comment = "请客送礼" },
                new FinanceTransactionType { ID = 48, Name = "孝敬家长", Expense = true, ParID = 31, Comment = "孝敬家长" },
                // Expense: 公共交通类
                new FinanceTransactionType { ID = 69, Name = "公共交通类", Expense = true, ParID = null, Comment = "公共交通类" },
                new FinanceTransactionType { ID = 70, Name = "公交地铁等", Expense = true, ParID = 69, Comment = "公交地铁等" },
                new FinanceTransactionType { ID = 71, Name = "长途客车等", Expense = true, ParID = 69, Comment = "长途客车等" },
                new FinanceTransactionType { ID = 72, Name = "火车动车等", Expense = true, ParID = 69, Comment = "火车动车等" },
                new FinanceTransactionType { ID = 73, Name = "飞机等", Expense = true, ParID = 69, Comment = "飞机等" },
                new FinanceTransactionType { ID = 74, Name = "出租车等", Expense = true, ParID = 69, Comment = "出租车等" },
                // Expense: 医疗保健
                new FinanceTransactionType { ID = 75, Name = "医疗保健", Expense = true, ParID = null, Comment = "医疗保健" },
                new FinanceTransactionType { ID = 76, Name = "诊疗费", Expense = true, ParID = 75, Comment = "诊疗费" },
                new FinanceTransactionType { ID = 77, Name = "医药费", Expense = true, ParID = 75, Comment = "医药费" },
                new FinanceTransactionType { ID = 78, Name = "保健品费", Expense = true, ParID = 75, Comment = "保健品费" }
            );
        }

        private static void SeedLibraryPersonRoles(hihDataContext context)
        {
            if (context.PersonRoles.Any()) return;
            context.PersonRoles.AddRange(
                new LibraryPersonRole { Id = 1, Name = "Library.Author", Comment = "Author" },
                new LibraryPersonRole { Id = 2, Name = "Library.Translator", Comment = "译者" }
            );
        }

        private static void SeedLibraryOrganizationTypes(hihDataContext context)
        {
            if (context.OrganizationTypes.Any()) return;
            context.OrganizationTypes.AddRange(
                new LibraryOrganizationType { Id = 1, Name = "Library.Press", Comment = "出版社" },
                new LibraryOrganizationType { Id = 2, Name = "Library.Library", Comment = "图书馆" }
            );
        }

        private static void SeedLibraryBookCategories(hihDataContext context)
        {
            if (context.BookCategories.Any()) return;

            // System-level book categories (HomeID = null). `Name` is the i18n key the UI
            // resolves via transloco (Sys.BkCtgy.* in assets/i18n/{en,zh}.json), so every
            // row here is already bilingual. `ParentID` forms the hierarchy (null = root).
            // IDs are stable and referenced by t_lib_book_ctgy, so do not renumber existing
            // ones (1-9, 21, 41, 51, 61); new rows use free slots in the same ranges.
            context.BookCategories.AddRange(
                // --- Roots (ParentID = null) ---
                new LibraryBookCategory { Id = 1, Name = "Sys.BkCtgy.Novel", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 21, Name = "Sys.BkCtgy.Computer", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 41, Name = "Sys.BkCtgy.Education", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 51, Name = "Sys.BkCtgy.ChildBk", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 61, Name = "Sys.BkCtgy.Finance", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 71, Name = "Sys.BkCtgy.History", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 81, Name = "Sys.BkCtgy.ArtPt", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 91, Name = "Sys.BkCtgy.Health", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 101, Name = "Sys.BkCtgy.Cookbook", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 111, Name = "Sys.BkCtgy.Reference", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 121, Name = "Sys.BkCtgy.Comics", ParentID = null, Comment = null },
                new LibraryBookCategory { Id = 131, Name = "Sys.BkCtgy.Travel", ParentID = null, Comment = null },
                // --- Children of Novel (1) ---
                new LibraryBookCategory { Id = 2, Name = "Sys.BkCtgy.SciFiction", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 3, Name = "Sys.BkCtgy.Romance", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 4, Name = "Sys.BkCtgy.Thriller", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 5, Name = "Sys.BkCtgy.DetectiveStory", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 6, Name = "Sys.BkCtgy.KungfuNovels", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 7, Name = "Sys.BkCtgy.FantasyNovel", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 8, Name = "Sys.BkCtgy.ChineseClassical", ParentID = 1, Comment = null },
                new LibraryBookCategory { Id = 9, Name = "Sys.BkCtgy.WorldFamousBook", ParentID = 1, Comment = null },
                // --- Other children ---
                new LibraryBookCategory { Id = 62, Name = "Sys.BkCtgy.Accounting", ParentID = 61, Comment = null },
                new LibraryBookCategory { Id = 72, Name = "Sys.BkCtgy.Bio", ParentID = 71, Comment = null },
                new LibraryBookCategory { Id = 82, Name = "Sys.BkCtgy.CraftAndHobby", ParentID = 81, Comment = null }
            );
        }
    }
}
