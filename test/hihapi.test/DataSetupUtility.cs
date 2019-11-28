using System;
using System.Collections.Generic;
using hihapi.Models;

namespace hihapi.test 
{
    public static class DataSetupUtility
    {
        internal static List<DBVersion> listDBVersion = new List<DBVersion>();
        internal static List<HomeDefine> listHomeDefine = new List<HomeDefine>();
        internal static List<HomeMember> listHomeMember = new List<HomeMember>();
        internal static List<Currency> listCurrency = new List<Currency>();
        internal static List<Language> listLanguage = new List<Language>();

        static DataSetupUtility()
        {
            SetupTable_DBVersion();
            SetupTable_Currency();
            SetupTable_Language();
        }

        public static void InitializeDbForTests(hihDataContext db)
        {
            InitialTable_DBVersion(db);

            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(hihDataContext db)
        {
            // db.Messages.RemoveRange(db.Messages);
            InitializeDbForTests(db);
        }

        public static void InitialTable_DBVersion(hihDataContext db)
        {
            db.DBVersions.AddRange(listDBVersion);
        }
        public static void InitialTable_Currency(hihDataContext db)
        {
            db.Currencies.AddRange(listCurrency);
        }
        public static void InitialTable_Language(hihDataContext db)
        {
            db.Languages.AddRange(listLanguage);
        }

        public static void SetupTable_HomeDefineAndMember()
        {
            // Home 1
            // Member A (host)
            // Member B
            // Member C
            // Member D
            listHomeDefine.Add(new HomeDefine()
            {
                ID = 1,
                BaseCurrency = "CNY",
                Name = "Home 1",
                Host = "USERA"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 1,
                DisplayAs = "User A",
                Relation = 0,
                User = "USERA"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 1,
                DisplayAs = "User B",
                Relation = 1,
                User = "USERB"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 1,
                DisplayAs = "User C",
                Relation = 2,
                User = "USERC"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 1,
                DisplayAs = "User D",
                Relation = 2,
                User = "USERD"
            });

            // Home 2
            // Member B (Host)
            listHomeDefine.Add(new HomeDefine()
            {
                ID = 2,
                BaseCurrency = "CNY",
                Name = "Home 2",
                Host = "USERB"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 2,
                DisplayAs = "User B",
                Relation = 0,
                User = "USERB"
            });

            // Home 3
            // Member A (Host)
            // Member B
            listHomeDefine.Add(new HomeDefine()
            {
                ID = 3,
                BaseCurrency = "CNY",
                Name = "Home 3",
                Host = "USERA"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 3,
                DisplayAs = "User A",
                Relation = 0,
                User = "USERA"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 3,
                DisplayAs = "User B",
                Relation = 1,
                User = "USERB"
            });

            // Home 4
            // Member C (Host)
            listHomeDefine.Add(new HomeDefine()
            {
                ID = 4,
                BaseCurrency = "USD",
                Name = "Home 4",
                Host = "USERC"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 4,
                DisplayAs = "User C",
                Relation = 0,
                User = "USERC"
            });

            // Home 5
            // Member D (host)
            listHomeDefine.Add(new HomeDefine()
            {
                ID = 5,
                BaseCurrency = "EUR",
                Name = "Home 5",
                Host = "USERD"
            });
            listHomeMember.Add(new HomeMember()
            {
                HomeID = 5,
                DisplayAs = "User D",
                Relation = 0,
                User = "USERD"
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
    }
}
