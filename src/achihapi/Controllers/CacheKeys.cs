using System;

namespace achihapi.Controllers
{
    public static class CacheKeys
    {
        // Home def.
        public static String HomeDefList = "HomeDefList_{0}_{1}_{2}"; // UserName, Top, Skip
        public static String HomeDef = "HomeDef_{0}"; // Home ID

        // Finance part
        public static String FinCurrency = "Fin_Currency";
        public static String FinAccountCtgyList = "Fin_AcntCtgyList_{0}"; // Home ID
        public static String FinAssetCtgyList = "Fin_AssetCtgyList_{0}"; // Home ID
        public static String FinDocTypeList = "Fin_DocTypeList_{0}"; // Home ID
        public static String FinTranTypeList = "Fin_TranTypeList_{0}"; // Home ID
        public static String FinAccountList = "Fin_AccountList_{0}_{1}"; // Home ID, Status
        public static String FinAccount = "Fin_Account_{0}_{1}"; // Home ID, Account ID
        public static String FinOrderList = "Fin_OrderList_{0}_{1}"; // Home ID, Invalid flag
        public static String FinCCList = "Fin_CCList_{0}"; // Home Id
        public static String FinPlanList = "Fin_PlanList_{0}"; // Home Id

        public static String FinReportBS = "Fin_Report_BS_{0}"; // Home ID
        public static String FinReportCC = "Fin_Report_CC_{0}"; // Home ID
        public static String FinReportOrder = "Fin_Report_Order_{0}"; // Home ID

        // Learning part
        public static String LearnCtgyList = "Learn_CtgyList_{0}"; // Home ID
    }
}
