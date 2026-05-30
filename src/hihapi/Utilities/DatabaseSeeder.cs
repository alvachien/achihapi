using System.Linq;
using System.Threading.Tasks;
using hihapi.Models;
using hihapi.Models.Library;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Utilities
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(hihDataContext context)
        {
            await context.Database.EnsureCreatedAsync();

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
            context.BookCategories.AddRange(
                new LibraryBookCategory { Id = 1, Name = "Sys.BkCtgy.Novel", Comment = "Novel" },
                new LibraryBookCategory { Id = 2, Name = "Sys.BkCtgy.SciFiction", Comment = "Sci Fiction", ParentID = 1 },
                new LibraryBookCategory { Id = 3, Name = "Sys.BkCtgy.Romance", Comment = "Romance", ParentID = 1 },
                new LibraryBookCategory { Id = 4, Name = "Sys.BkCtgy.Thriller", Comment = "悬疑类", ParentID = 1 },
                new LibraryBookCategory { Id = 5, Name = "Sys.BkCtgy.DetectiveStory", Comment = "侦探、推理类", ParentID = 1 },
                new LibraryBookCategory { Id = 6, Name = "Sys.BkCtgy.KungfuNovels", Comment = "武侠小说类", ParentID = 1 },
                new LibraryBookCategory { Id = 7, Name = "Sys.BkCtgy.FantasyNovel", Comment = "玄幻小说类", ParentID = 1 }
            );
        }
    }
}
