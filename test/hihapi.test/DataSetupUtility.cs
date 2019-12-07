using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using hihapi.Models;

namespace hihapi.test
{
    public static class DataSetupUtility
    {
        public static List<DBVersion> DBVersions
        {
            get { return listDBVersion; }
        }
        public static List<HomeDefine> HomeDefines
        {
            get { return listHomeDefine;  }
        }
        public static List<HomeMember> HomeMembers
        {
            get { return listHomeMember;  }
        }
        public static List<Currency> Currencies
        {
            get { return listCurrency;  }
        }
        public static List<Language> Languages
        {
            get { return listLanguage; }
        }
        public static List<FinanceAccountCategory> FinanceAccountCategories
        {
            get { return listFinAccountCategory;  }
        }
        public static List<FinanceAssetCategory> FinanceAssetCategories
        {
            get { return listFinAssetCategory; }
        }
        public static List<FinanceDocumentType> FinanceDocumentTypes
        {
            get { return listFinDocumentType; }
        }
        public static List<FinanceTransactionType> FinanceTransactionTypes
        {
            get { return listFinTransactionType; }
        }
        
        private static List<DBVersion> listDBVersion = new List<DBVersion>();
        private static List<HomeDefine> listHomeDefine = new List<HomeDefine>();
        private static List<HomeMember> listHomeMember = new List<HomeMember>();
        private static List<Currency> listCurrency = new List<Currency>();
        private static List<Language> listLanguage = new List<Language>();
        private static List<FinanceAccountCategory> listFinAccountCategory = new List<FinanceAccountCategory>();
        private static List<FinanceAssetCategory> listFinAssetCategory = new List<FinanceAssetCategory>();
        private static List<FinanceDocumentType> listFinDocumentType = new List<FinanceDocumentType>();
        private static List<FinanceTransactionType> listFinTransactionType = new List<FinanceTransactionType>();
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
            // Setup tables
            SetupTable_DBVersion();
            SetupTable_Currency();
            SetupTable_Language();
            SetupTable_FinAccountCategory();
            SetupTable_FinDocumentType();
            SetupTable_FinAssertCategory();
            SetupTable_FinTransactionType();

            SetupTable_HomeDefineAndMember();
        }

        public static ClaimsPrincipal GetClaimForUser(String usr)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, usr),
                new Claim(ClaimTypes.NameIdentifier, usr),
            }, "mock"));
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

        private static void InitialTable_DBVersion(hihDataContext db)
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
            listHomeDefine.Add(new HomeDefine()
            {
                ID = Home1ID,
                BaseCurrency = Home1BaseCurrency,
                Name = "Home 1",
                Host = UserA
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User A",
                Relation = 0,
                User = UserA
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User B",
                Relation = 1,
                User = UserB
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User C",
                Relation = 2,
                User = UserC
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home1ID,
                DisplayAs = "User D",
                Relation = 2,
                User = UserD
            });

            // Home 2
            // Member B (Host)
            listHomeDefine.Add(new HomeDefine()
            {
                ID = Home2ID,
                BaseCurrency = Home2BaseCurrency,
                Name = "Home 2",
                Host = UserB
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home2ID,
                DisplayAs = "User B",
                Relation = 0,
                User = UserB
            });

            // Home 3
            // Member A (Host)
            // Member B
            listHomeDefine.Add(new HomeDefine()
            {
                ID = Home3ID,
                BaseCurrency = Home3BaseCurrency,
                Name = "Home 3",
                Host = UserA
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home3ID,
                DisplayAs = "User A",
                Relation = 0,
                User = UserA
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home3ID,
                DisplayAs = "User B",
                Relation = 1,
                User = UserB
            });

            // Home 4
            // Member C (Host)
            listHomeDefine.Add(new HomeDefine()
            {
                ID = Home4ID,
                BaseCurrency = Home4BaseCurrency,
                Name = "Home 4",
                Host = UserC
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home4ID,
                DisplayAs = "User C",
                Relation = 0,
                User = UserC
            });

            // Home 5
            // Member D (host)
            listHomeDefine.Add(new HomeDefine()
            {
                ID = Home5ID,
                BaseCurrency = Home5BaseCurrency,
                Name = "Home 5",
                Host = UserD
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = Home5ID,
                DisplayAs = "User D",
                Relation = 0,
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
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 1,
                ReleasedDate = new DateTime(2018, 7, 4)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 2,
                ReleasedDate = new DateTime(2018, 7, 5)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 3,
                ReleasedDate = new DateTime(2018, 7, 10)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 4,
                ReleasedDate = new DateTime(2018, 7, 11)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 5,
                ReleasedDate = new DateTime(2018, 8, 4)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 6,
                ReleasedDate = new DateTime(2018, 8, 5)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 7,
                ReleasedDate = new DateTime(2018, 10, 10)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 8,
                ReleasedDate = new DateTime(2018, 11, 1)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 9,
                ReleasedDate = new DateTime(2018, 11, 2)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 10,
                ReleasedDate = new DateTime(2018, 11, 3)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 11,
                ReleasedDate = new DateTime(2018, 12, 20)
            });
            listDBVersion.Add(new DBVersion()
            {
                VersionID = 12,
                ReleasedDate = new DateTime(2019, 4, 20)
            });
        }

        private static void SetupTable_Currency()
        {
            listCurrency.Add(new Currency() { Curr = "CNY", Name = "Sys.Currency.CNY", Symbol = "¥" });
            listCurrency.Add(new Currency() { Curr = "EUR", Name = "Sys.Currency.EUR", Symbol = "€" });
            listCurrency.Add(new Currency() { Curr = "HKD", Name = "Sys.Currency.HKD", Symbol = "HK$" });
            listCurrency.Add(new Currency() { Curr = "JPY", Name = "Sys.Currency.JPY", Symbol = "¥" });
            listCurrency.Add(new Currency() { Curr = "KRW", Name = "Sys.Currency.KRW", Symbol = "₩" });
            listCurrency.Add(new Currency() { Curr = "TWD", Name = "Sys.Currency.TWD", Symbol = "TW$" });
            listCurrency.Add(new Currency() { Curr = "USD", Name = "Sys.Currency.USD", Symbol = "$" });
        }

        private static void SetupTable_Language()
        {
            listLanguage.Add(new Language() { Lcid = 4, ISOName = "zh-Hans", EnglishName = "Chinese (Simplified)", NativeName = "简体中文", AppFlag = true });
            listLanguage.Add(new Language() { Lcid = 9, ISOName = "en", EnglishName = "English", NativeName = "English", AppFlag = true });
            listLanguage.Add(new Language() { Lcid = 17, ISOName = "ja", EnglishName = "Japanese", NativeName = "日本语", AppFlag = false });
            listLanguage.Add(new Language() { Lcid = 31748, ISOName = "zh-Hant", EnglishName = "Chinese (Traditional)", NativeName = "繁體中文", AppFlag = false });
        }

        private static void SetupTable_FinAccountCategory()
        {
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 1, Name = "Sys.AcntCty.Cash", AssetFlag = true, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 2, Name = "Sys.AcntCty.DepositAccount", AssetFlag = true, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 3, Name = "Sys.AcntCty.CreditCard", AssetFlag = false, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 4, Name = "Sys.AcntCty.AccountPayable", AssetFlag = false, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 5, Name = "Sys.AcntCty.AccountReceviable", AssetFlag = true, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 6, Name = "Sys.AcntCty.VirtualAccount", AssetFlag = true, Comment = "如支付宝等" });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 7, Name = "Sys.AcntCty.AssetAccount", AssetFlag = true, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 8, Name = "Sys.AcntCty.AdvancedPayment", AssetFlag = true, Comment = null });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 9, Name = "Sys.AcntCty.BorrowFrom", AssetFlag = false, Comment = "借入款、贷款" });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 10, Name = "Sys.AcntCty.LendTo", AssetFlag = true, Comment = "借出款" });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 11, Name = "Sys.AcntCty.AdvancedRecv", AssetFlag = false, Comment = "预收款" });
            listFinAccountCategory.Add(new FinanceAccountCategory() { ID = 12, Name = "Sys.AcntCty.Insurance", AssetFlag = true, Comment = "保险" });
        }

        private static void SetupTable_FinDocumentType()
        {
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 1, Name = "Sys.DocTy.Normal", Comment = "普通" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 2, Name = "Sys.DocTy.Transfer", Comment = "转账" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 3, Name = "Sys.DocTy.CurrExg", Comment = "兑换不同的货币" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 4, Name = "Sys.DocTy.Installment", Comment = "分期付款" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 5, Name = "Sys.DocTy.AdvancedPayment", Comment = "预付款" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 6, Name = "Sys.DocTy.CreditCardRepay", Comment = "信用卡还款" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 7, Name = "Sys.DocTy.AssetBuyIn", Comment = "购入资产或大件家用器具" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 8, Name = "Sys.DocTy.AssetSoldOut", Comment = "出售资产或大件家用器具" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 9, Name = "Sys.DocTy.BorrowFrom", Comment = "借款、贷款等" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 10, Name = "Sys.DocTy.LendTo", Comment = "借出款" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 11, Name = "Sys.DocTy.Repay", Comment = "借款、贷款等" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 12, Name = "Sys.DocTy.AdvancedRecv", Comment = "预收款" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 13, Name = "Sys.DocTy.AssetValChg", Comment = "资产净值变动" });
            listFinDocumentType.Add(new FinanceDocumentType() { ID = 14, Name = "Sys.DocTy.Insurance", Comment = "保险" });
        }

        private static void SetupTable_FinAssertCategory()
        {
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 1, Name = "Sys.AssCtgy.Apartment", Desp = "公寓" });
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 2, Name = "Sys.AssCtgy.Automobile", Desp = "机动车" });
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 3, Name = "Sys.AssCtgy.Furniture", Desp = "家具" });
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 4, Name = "Sys.AssCtgy.HouseAppliances", Desp = "家用电器" });
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 5, Name = "Sys.AssCtgy.Camera", Desp = "相机" });
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 6, Name = "Sys.AssCtgy.Computer", Desp = "计算机" });
            listFinAssetCategory.Add(new FinanceAssetCategory() { ID = 7, Name = "Sys.AssCtgy.MobileDevice", Desp = "移动设备" });
        }

        private static void SetupTable_FinTransactionType()
        {
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 2, Name = "主业收入", Expense = false, ParID = null, Comment = "主业收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 3, Name = "工资", Expense = false, ParID = 2, Comment = "工资" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 4, Name = "奖金", Expense = false, ParID = 2, Comment = "奖金" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 35, Name = "津贴", Expense = false, ParID = 2, Comment = "津贴类，如加班等" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 5, Name = "投资、保险、博彩类收入", Expense = false, ParID = null, Comment = "投资、保险、博彩类收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 6, Name = "股票收益", Expense = false, ParID = 5, Comment = "股票收益" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 7, Name = "基金收益", Expense = false, ParID = 5, Comment = "基金收益" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 8, Name = "利息收入", Expense = false, ParID = 5, Comment = "银行利息收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 13, Name = "彩票收益", Expense = false, ParID = 5, Comment = "彩票中奖类收益" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 36, Name = "保险报销收入", Expense = false, ParID = 5, Comment = "保险报销收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 84, Name = "房租收入", Expense = false, ParID = 5, Comment = "房租收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 87, Name = "借贷还款收入", Expense = false, ParID = 5, Comment = "借贷还款收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 90, Name = "资产增值", Expense = false, ParID = 5, Comment = "资产增值" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 93, Name = "资产出售收益", Expense = false, ParID = 5, Comment = "资产出售收益" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 10, Name = "其它收入", Expense = false, ParID = null, Comment = "其它收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 1, Name = "起始资金", Expense = false, ParID = 10, Comment = "起始资金" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 37, Name = "转账收入", Expense = false, ParID = 10, Comment = "转账收入" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 80, Name = "贷款入账", Expense = false, ParID = 10, Comment = "贷款入账" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 91, Name = "预收款收入", Expense = false, ParID = 10, Comment = "预收款收入" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 30, Name = "人情交往类", Expense = false, ParID = null, Comment = "人情交往类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 33, Name = "红包收入", Expense = false, ParID = 30, Comment = "红包收入" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 9, Name = "生活类开支", Expense = true, ParID = null, Comment = "生活类开支" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 11, Name = "物业类支出", Expense = true, ParID = 9, Comment = "物业类支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 14, Name = "小区物业费", Expense = true, ParID = 11, Comment = "小区物业费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 15, Name = "水费", Expense = true, ParID = 11, Comment = "水费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 16, Name = "电费", Expense = true, ParID = 11, Comment = "电费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 17, Name = "天然气费", Expense = true, ParID = 11, Comment = "天然气费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 18, Name = "物业维修费", Expense = true, ParID = 11, Comment = "物业维修费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 26, Name = "通讯费", Expense = true, ParID = 9, Comment = "通讯费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 27, Name = "固定电话/宽带", Expense = true, ParID = 26, Comment = "固定电话/宽带" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 28, Name = "手机费", Expense = true, ParID = 26, Comment = "手机费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 38, Name = "衣服饰品", Expense = true, ParID = 9, Comment = "衣服饰品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 39, Name = "食品酒水", Expense = true, ParID = 9, Comment = "食品酒水" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 40, Name = "衣服鞋帽", Expense = true, ParID = 38, Comment = "衣服鞋帽" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 41, Name = "化妆饰品", Expense = true, ParID = 38, Comment = "化妆饰品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 42, Name = "水果类", Expense = true, ParID = 39, Comment = "水果类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 43, Name = "零食类", Expense = true, ParID = 39, Comment = "零食类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 44, Name = "烟酒茶类", Expense = true, ParID = 39, Comment = "烟酒茶类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 45, Name = "咖啡外卖类", Expense = true, ParID = 39, Comment = "咖啡外卖类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 46, Name = "早中晚餐", Expense = true, ParID = 39, Comment = "早中晚餐" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 49, Name = "休闲娱乐", Expense = true, ParID = 9, Comment = "休闲娱乐" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 50, Name = "旅游度假", Expense = true, ParID = 49, Comment = "旅游度假" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 51, Name = "电影演出", Expense = true, ParID = 49, Comment = "电影演出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 52, Name = "摄影外拍类", Expense = true, ParID = 49, Comment = "摄影外拍类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 53, Name = "腐败聚会类", Expense = true, ParID = 49, Comment = "腐败聚会类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 54, Name = "学习进修", Expense = true, ParID = 9, Comment = "学习进修" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 58, Name = "书刊杂志", Expense = true, ParID = 54, Comment = "书刊杂志" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 59, Name = "培训进修", Expense = true, ParID = 54, Comment = "培训进修" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 61, Name = "日常用品", Expense = true, ParID = 9, Comment = "日常用品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 62, Name = "日用品", Expense = true, ParID = 61, Comment = "日用品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 63, Name = "电子产品类", Expense = true, ParID = 61, Comment = "电子产品类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 64, Name = "厨房用具", Expense = true, ParID = 61, Comment = "厨房用具" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 65, Name = "洗涤用品", Expense = true, ParID = 61, Comment = "洗涤用品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 66, Name = "大家电类", Expense = true, ParID = 61, Comment = "大家电类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 67, Name = "保健护理用品", Expense = true, ParID = 61, Comment = "保健护理用品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 68, Name = "喂哺用品", Expense = true, ParID = 61, Comment = "喂哺用品" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 79, Name = "有线电视费", Expense = true, ParID = 11, Comment = "有线电视费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 85, Name = "房租支出", Expense = true, ParID = 11, Comment = "房租支出" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 12, Name = "私家车支出", Expense = true, ParID = null, Comment = "私家车支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 19, Name = "车辆保养", Expense = true, ParID = 12, Comment = "车辆保养" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 20, Name = "汽油费", Expense = true, ParID = 12, Comment = "汽油费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 21, Name = "车辆保险费", Expense = true, ParID = 12, Comment = "车辆保险费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 22, Name = "停车费", Expense = true, ParID = 12, Comment = "停车费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 23, Name = "车辆维修", Expense = true, ParID = 12, Comment = "车辆维修" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 57, Name = "违章付款类", Expense = true, ParID = 12, Comment = "违章付款类" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 24, Name = "其它支出", Expense = true, ParID = null, Comment = "其它支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 82, Name = "起始负债", Expense = true, ParID = 24, Comment = "起始负债" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 60, Name = "转账支出", Expense = true, ParID = 24, Comment = "转账支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 81, Name = "借出款项", Expense = true, ParID = 24, Comment = "借出款项" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 88, Name = "预付款支出", Expense = true, ParID = 24, Comment = "预付款支出" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 25, Name = "投资、保险、博彩类支出", Expense = true, ParID = null, Comment = "投资、保险、博彩类支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 29, Name = "彩票支出", Expense = true, ParID = 25, Comment = "彩票投注等支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 34, Name = "保单投保、续保支出", Expense = true, ParID = 25, Comment = "保单投保、续保支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 55, Name = "银行利息支出", Expense = true, ParID = 25, Comment = "银行利息支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 56, Name = "银行手续费支出", Expense = true, ParID = 25, Comment = "银行手续费支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 83, Name = "投资手续费支出", Expense = true, ParID = 25, Comment = "投资手续费支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 86, Name = "偿还借贷款", Expense = true, ParID = 25, Comment = "偿还借贷款" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 89, Name = "资产减值", Expense = true, ParID = 25, Comment = "资产减值" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 92, Name = "资产出售费用", Expense = true, ParID = 25, Comment = "资产出售费用" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 31, Name = "人际交往", Expense = true, ParID = null, Comment = "人际交往" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 32, Name = "红包支出", Expense = true, ParID = 31, Comment = "红包支出" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 47, Name = "请客送礼", Expense = true, ParID = 31, Comment = "请客送礼" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 48, Name = "孝敬家长", Expense = true, ParID = 31, Comment = "孝敬家长" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 69, Name = "公共交通类", Expense = true, ParID = null, Comment = "公共交通类" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 70, Name = "公交地铁等", Expense = true, ParID = 69, Comment = "公交地铁等" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 71, Name = "长途客车等", Expense = true, ParID = 69, Comment = "长途客车等" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 72, Name = "火车动车等", Expense = true, ParID = 69, Comment = "火车动车等" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 73, Name = "飞机等", Expense = true, ParID = 69, Comment = "飞机等" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 74, Name = "出租车等", Expense = true, ParID = 69, Comment = "出租车等" });

            listFinTransactionType.Add(new FinanceTransactionType() { ID = 75, Name = "医疗保健", Expense = true, ParID = null, Comment = "医疗保健" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 76, Name = "诊疗费", Expense = true, ParID = 75, Comment = "诊疗费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 77, Name = "医药费", Expense = true, ParID = 75, Comment = "医药费" });
            listFinTransactionType.Add(new FinanceTransactionType() { ID = 78, Name = "保健品费", Expense = true, ParID = 75, Comment = "保健品费" });
        }
    }
}
