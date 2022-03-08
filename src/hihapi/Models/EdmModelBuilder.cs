using System;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace hihapi.Models
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Currency>("Currencies");
            modelBuilder.EntitySet<Language>("Languages");
            modelBuilder.EntitySet<DBVersion>("DBVersions");
            modelBuilder.EntitySet<CheckVersionResult>("CheckVersionResult");
            modelBuilder.EnumType<HomeMemberRelationType>();
            modelBuilder.EntitySet<HomeDefine>("HomeDefines");
            modelBuilder.EntitySet<HomeMember>("HomeMembers");
            modelBuilder.EntitySet<LearnCategory>("LearnCategories");
            modelBuilder.EntitySet<LearnObject>("LearnObjects");
            modelBuilder.EntitySet<FinanceAccountCategory>("FinanceAccountCategories");
            modelBuilder.EntitySet<FinanceAssetCategory>("FinanceAssetCategories");
            modelBuilder.EntitySet<FinanceDocumentType>("FinanceDocumentTypes");
            modelBuilder.EntitySet<FinanceTransactionType>("FinanceTransactionTypes");
            modelBuilder.EnumType<FinanceAccountStatus>();
            modelBuilder.EnumType<RepeatFrequency>();
            modelBuilder.EnumType<LoanRepaymentMethod>();
            modelBuilder.EnumType<FinancePlanTypeEnum>();
            modelBuilder.EntitySet<FinanceControlCenter>("FinanceControlCenters");
            modelBuilder.EntitySet<FinanceAccount>("FinanceAccounts");
            modelBuilder.EntitySet<FinanceAccountExtraDP>("FinanceAccountExtraDPs");
            modelBuilder.EntitySet<FinanceAccountExtraAS>("FinanceAccountExtraASs");
            modelBuilder.EntitySet<FinanceTmpDPDocument>("FinanceTmpDPDocuments");
            modelBuilder.EntitySet<FinanceTmpLoanDocument>("FinanceTmpLoanDocuments");
            modelBuilder.EntitySet<FinanceOrder>("FinanceOrders");
            modelBuilder.EntitySet<FinanceOrderSRule>("FinanceOrderSRules");
            modelBuilder.EntitySet<FinancePlan>("FinancePlans");
            modelBuilder.EntitySet<FinanceDocument>("FinanceDocuments");
            modelBuilder.EntitySet<FinanceDocumentItem>("FinanceDocumentItems");
            modelBuilder.EntitySet<FinanceDocumentItemView>("FinanceDocumentItemViews");
            //var acntBalRpt = modelBuilder.EntitySet<FinanceReportByAccount>("FinanceReportByAccounts");
            //acntBalRpt.EntityType.HasKey(p => new {
            //    p.HomeID,
            //    p.AccountID,
            //});
            modelBuilder.EntityType<FinanceReportByAccount>().HasKey(p => new {
                p.HomeID,
                p.AccountID,
            });
            //var ccBalRpt = modelBuilder.EntitySet<FinanceReportByControlCenter>("FinanceReportByControlCenters");
            //ccBalRpt.EntityType.HasKey(p => new
            //{
            //    p.HomeID,
            //    p.ControlCenterID,
            //});
            modelBuilder.EntityType<FinanceReportByControlCenter>().HasKey(p => new {
                p.HomeID,
                p.ControlCenterID,
            });
            //var ordBalRpt = modelBuilder.EntitySet<FinanceReportByOrder>("FinanceReportByOrders");
            //ordBalRpt.EntityType.HasKey(p => new
            //{
            //    p.HomeID,
            //    p.OrderID,
            //});
            modelBuilder.EntityType<FinanceReportByOrder>().HasKey(p => new {
                p.HomeID,
                p.OrderID,
            });
            modelBuilder.ComplexType<RepeatedDates>();
            modelBuilder.ComplexType<RepeatDatesCalculationInput>();
            modelBuilder.ComplexType<RepeatedDatesWithAmount>();
            modelBuilder.ComplexType<RepeatDatesWithAmountCalculationInput>();
            modelBuilder.ComplexType<RepeatedDatesWithAmountAndInterest>();
            modelBuilder.EntitySet<FinanceReport>("FinanceReports");
            modelBuilder.EntityType<FinanceReportByTransactionType>();
            modelBuilder.EntityType<FinanceReportByTransactionTypeMOM>();
            modelBuilder.EntityType<FinanceReportByAccountMOM>();
            modelBuilder.EntityType<FinanceReportByControlCenterMOM>();
            var rptAcntExpense = modelBuilder.EntityType<FinanceReporAccountGroupAndExpenseView>();
            rptAcntExpense.HasKey(p => new {
                p.HomeID,
                p.AccountID,
            });

            // Utilties Functions
            modelBuilder.Function("GetRepeatedDates")
                .ReturnsCollection<RepeatedDates>();
            var funcbuilder = modelBuilder.Function("GetRepeatedDates2")
                .ReturnsCollection<RepeatedDates>();
            funcbuilder.Parameter<string>("StartDate");
            funcbuilder.Parameter<string>("EndDate");
            funcbuilder.Parameter<int>("RepeatType");
            modelBuilder.Function("GetRepeatedDatesWithAmount")
                .ReturnsCollection<RepeatedDatesWithAmount>();
            modelBuilder.Function("GetRepeatedDatesWithAmountAndInterest")
                .ReturnsCollection<RepeatedDatesWithAmountAndInterest>();

            // Actions on Accounts
            var acntEntity = modelBuilder.EntityType<FinanceAccount>();
            // Action: Close account
            var closeAccountAction = acntEntity.Collection.Action("CloseAccount");
            closeAccountAction.Parameter<int>("HomeID");
            closeAccountAction.Parameter<int>("AccountID");
            closeAccountAction.Returns<bool>();
            // Action: Settle Account
            var settleAccountAction = acntEntity.Collection.Action("SettleAccount");
            settleAccountAction.Parameter<int>("HomeID");
            settleAccountAction.Parameter<int>("AccountID");
            settleAccountAction.Parameter<int>("ControlCenterID");
            settleAccountAction.Parameter<string>("SettledDate");
            settleAccountAction.Parameter<Decimal>("InitialAmount");
            settleAccountAction.Parameter<String>("Currency");
            settleAccountAction.Returns<bool>();

            // Functions/Actions on Documents
            var docEntity = modelBuilder.EntityType<FinanceDocument>();
            docEntity.Property(c => c.TranDate).AsDate();
            // Functions
            var functionIsDocChangable = docEntity.Function("IsChangable");
            functionIsDocChangable.Returns<bool>();

            // Actions
            docEntity.Collection
                .Action("PostDPDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            docEntity.Collection
                .Action("PostLoanDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            docEntity.Collection
                .Action("PostAssetBuyDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            docEntity.Collection
                .Action("PostAssetSellDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            docEntity.Collection
                .Action("PostAssetValueChangeDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            modelBuilder.Namespace = typeof(Currency).Namespace;
            // Function on DP template documents
            var tmpTpDocEntity = modelBuilder.EntityType<FinanceTmpDPDocument>();
            var tmpTpDocPostFunc =
                tmpTpDocEntity.Collection.Action("PostDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            // Action on Loan template documents: Repay document
            var tmpLoanDocEntity = modelBuilder.EntityType<FinanceTmpLoanDocument>();
            tmpLoanDocEntity.Collection
                .Action("PostRepayDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            tmpLoanDocEntity.Collection
                .Action("PostPrepaymentDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");

            // Actions on reports
            var reportEntity = modelBuilder.EntityType<FinanceReport>();
            // Action: GetMonthlyReportByTranType
            var actionReportCurrentMonthByTT = reportEntity.Collection.Action("GetReportByTranType");
                // .ReturnsCollection<FinanceReportByTransactionType>();
            actionReportCurrentMonthByTT.Parameter<int>("HomeID");
            actionReportCurrentMonthByTT.Parameter<int>("Year");
            actionReportCurrentMonthByTT.Parameter<int?>("Month");
            actionReportCurrentMonthByTT.ReturnsFromEntitySet<FinanceReportByTransactionType>("FinanceReportByTransactionTypes");
            // Action: Get Report By Transaction Type MOM
            var actionReportByTTMOM = reportEntity.Collection.Action("GetReportByTranTypeMOM");
            // .ReturnsCollection<FinanceReportByTransactionType>();
            actionReportByTTMOM.Parameter<int>("HomeID");
            actionReportByTTMOM.Parameter<int>("TransactionType");
            actionReportByTTMOM.Parameter<bool?>("IncludeChildren");
            actionReportByTTMOM.Parameter<string>("Period");
            actionReportByTTMOM.ReturnsFromEntitySet<FinanceReportByTransactionTypeMOM>("FinanceReportByTransactionTypeMOMs");
            // Action: Get Report by Account
            var actionReportAccount = reportEntity.Collection.Action("GetReportByAccount");
            actionReportAccount.Parameter<int>("HomeID");
            actionReportAccount.ReturnsFromEntitySet<FinanceReportByAccount>("FinanceReportByAccounts");
            // Action: Get Report By Account MOM
            var actionReportByAccountMOM = reportEntity.Collection.Action("GetReportByAccountMOM");
            // .ReturnsCollection<FinanceReportByTransactionType>();
            actionReportByAccountMOM.Parameter<int>("HomeID");
            actionReportByAccountMOM.Parameter<int>("AccountID");
            actionReportByAccountMOM.Parameter<string>("Period");
            actionReportByAccountMOM.ReturnsFromEntitySet<FinanceReportByAccountMOM>("FinanceReportByAccountMOMs");
            // Action: Get Report by ControlCenter
            var actionReportCC = reportEntity.Collection.Action("GetReportByControlCenter");
            actionReportCC.Parameter<int>("HomeID");
            actionReportCC.ReturnsFromEntitySet<FinanceReportByControlCenter>("FinanceReportByControlCenters");
            // Action: Get Report By ControlCenter MOM
            var actionReportByCCMOM = reportEntity.Collection.Action("GetReportByControlCenterMOM");
            // .ReturnsCollection<FinanceReportByTransactionType>();
            actionReportByCCMOM.Parameter<int>("HomeID");
            actionReportByCCMOM.Parameter<int>("ControlCenterID");
            actionReportByCCMOM.Parameter<bool?>("IncludeChildren");
            actionReportByCCMOM.Parameter<string>("Period");
            actionReportByCCMOM.ReturnsFromEntitySet<FinanceReportByControlCenterMOM>("FinanceReportByControlCenterMOMs");
            // Action: Get Report by Order
            var actionReportOrder = reportEntity.Collection.Action("GetReportByOrder");
            actionReportOrder.Parameter<int>("HomeID");
            actionReportOrder.Parameter<int?>("OrderID");
            actionReportOrder.ReturnsFromEntitySet<FinanceReportByOrder>("FinanceReportByOrders");
            // Overview key figures
            var entityFinOverviewKeyfigure = modelBuilder.EntityType<FinanceOverviewKeyFigure>();
            entityFinOverviewKeyfigure.HasKey(p => new {
                p.HomeID,
            });
            var actionFinanceOverviewKeyfigure = reportEntity.Collection.Action("GetFinanceOverviewKeyFigure");
            actionFinanceOverviewKeyfigure.Parameter<int>("HomeID");
            actionFinanceOverviewKeyfigure.Parameter<int>("Year");
            actionFinanceOverviewKeyfigure.Parameter<int>("Month");
            actionFinanceOverviewKeyfigure.Parameter<bool>("ExcludeTransfer");
            actionFinanceOverviewKeyfigure.ReturnsFromEntitySet<FinanceOverviewKeyFigure>("FinanceOverviewKeyFigure");

            modelBuilder.EntitySet<BlogFormat>("BlogFormats");
            modelBuilder.EntitySet<BlogUserSetting>("BlogUserSettings");
            modelBuilder.EntitySet<BlogCollection>("BlogCollections");
            modelBuilder.EntitySet<BlogPost>("BlogPosts");
            modelBuilder.EntitySet<BlogPostCollection>("BlogPostCollections");
            modelBuilder.EntitySet<BlogPostTag>("BlogPostTags");
            // Functions in Blog part
            var postentity = modelBuilder.EntityType<BlogPost>();
            postentity.Function("Deploy")
                    .Returns<string>();
            postentity.Function("ClearDeploy")
                    .Returns<string>();
            var blogsetting = modelBuilder.EntityType<BlogUserSetting>();
            blogsetting.Function("Deploy")
                    .Returns<string>();

            return modelBuilder.GetEdmModel();
        }
    }
}
