using System;
using System.Collections.Generic;
using hihapi.Models.Event;
using hihapi.Models.Library;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace hihapi.Models
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.Namespace = typeof(Currency).Namespace;

            modelBuilder.EntitySet<Currency>("Currencies");
            modelBuilder.EntitySet<Language>("Languages");
            modelBuilder.EntitySet<DBVersion>("DBVersions");
            modelBuilder.EntitySet<CheckVersionResult>("CheckVersionResult");
            modelBuilder.EnumType<HomeMemberRelationType>();
            modelBuilder.EntitySet<HomeDefine>("HomeDefines");
            modelBuilder.EntitySet<HomeMember>("HomeMembers");
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
            var dprerst = modelBuilder.EntityType<FinanceAssetDepreicationResult>();
            dprerst.Property(c => c.TranDate).AsDate();
            modelBuilder.EntitySet<FinanceReport>("FinanceReports");
            modelBuilder.EntityType<FinanceReportByTransactionType>();
            modelBuilder.EntityType<FinanceReportByTransactionTypeMOM>();
            modelBuilder.EntityType<FinanceReportByAccountMOM>();
            modelBuilder.EntityType<FinanceReportByControlCenterMOM>();
            modelBuilder.EntityType<FinanceReportMOM>();
            modelBuilder.EntityType<FinanceReportPerDate>();
            modelBuilder.EntityType<FinanceAccountBalancePerDate>();
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
            // Function : Is Document Changable
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
            docEntity.Collection
                .Action("PostAssetDepreciationDocument")
                .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            // Function : Get asset depreciation
            var actionGetDepreResult = docEntity.Collection.Action("GetAssetDepreciationResult");
            actionGetDepreResult.Parameter<int>("HomeID");
            actionGetDepreResult.Parameter<int>("Year");
            actionGetDepreResult.Parameter<int?>("Month");
            actionGetDepreResult.ReturnsFromEntitySet<FinanceAssetDepreicationResult>("FinanceAssetDepreicationResults");

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
            // Action: GetAccountBalance
            var actionGetBalance = reportEntity.Collection.Action("GetAccountBalance");
            actionGetBalance.Parameter<int>("HomeID");
            actionGetBalance.Parameter<int>("AccountID");
            actionGetBalance.Returns<double>();
            // Action: GetAccountBalanceEx
            var actionGetBalanceEx = reportEntity.Collection.Action("GetAccountBalanceEx");
            actionGetBalanceEx.Parameter<int>("HomeID");
            actionGetBalanceEx.Parameter<int>("AccountID");
            actionGetBalanceEx.Parameter<List<DateTime>>("SelectedDates");
            actionGetBalanceEx.ReturnsFromEntitySet<FinanceAccountBalancePerDate>("FinanceAccountBalancePerDates");
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
            // Action: Get Cash Report
            var actionCashReport = reportEntity.Collection.Action("GetCashReport");
            actionCashReport.Parameter<int>("HomeID");
            actionCashReport.Parameter<string>("Period");
            actionCashReport.ReturnsFromEntitySet<FinanceReport>("FinanceReports");
            // Action: Get Cash Report MOM
            var actionCashReportMOM = reportEntity.Collection.Action("GetCashReportMOM");
            actionCashReportMOM.Parameter<int>("HomeID");
            actionCashReportMOM.Parameter<string>("Period");
            actionCashReportMOM.ReturnsFromEntitySet<FinanceReportMOM>("FinanceReportMOMs");
            // Action: Daily Cash Report
            var actionDailyCashReport = reportEntity.Collection.Action("GetDailyCashReport");
            actionDailyCashReport.Parameter<int>("HomeID");
            actionDailyCashReport.Parameter<int>("Year");
            actionDailyCashReport.Parameter<int>("Month");
            actionDailyCashReport.ReturnsFromEntitySet<FinanceReportPerDate>("FinanceReportPerDates");
            // Action: Statement of Income and ExpenseMOM
            var actionIEStatementMOM = reportEntity.Collection.Action("GetStatementOfIncomeAndExpenseMOM");
            actionIEStatementMOM.Parameter<int>("HomeID");
            actionIEStatementMOM.Parameter<string>("Period");
            actionIEStatementMOM.Parameter<bool>("ExcludeTransfer");
            actionIEStatementMOM.ReturnsFromEntitySet<FinanceReportMOM>("FinanceReportMOMs");
            // Action: Daily statement of Incomen and Expense
            var actionDailyIEStatement = reportEntity.Collection.Action("GetDailyStatementOfIncomeAndExpense");
            actionDailyIEStatement.Parameter<int>("HomeID");
            actionDailyIEStatement.Parameter<int>("Year");
            actionDailyIEStatement.Parameter<int>("Month");
            actionDailyIEStatement.Parameter<bool>("ExcludeTransfer");
            actionDailyIEStatement.ReturnsFromEntitySet<FinanceReportPerDate>("FinanceReportPerDates");
            // Action: Overview key figures
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

            // Library
            modelBuilder.EntitySet<LibraryPersonRole>("LibraryPersonRoles");
            modelBuilder.EntitySet<LibraryPerson>("LibraryPersons");
            modelBuilder.EntitySet<LibraryOrganizationType>("LibraryOrganizationTypes");
            modelBuilder.EntitySet<LibraryOrganization>("LibraryOrganizations");
            modelBuilder.EntitySet<LibraryBookCategory>("LibraryBookCategories");
            modelBuilder.EntitySet<LibraryBookLocation>("LibraryBookLocations");
            modelBuilder.EntitySet<LibraryBook>("LibraryBooks");
            modelBuilder.EntitySet<LibraryBookBorrowRecord>("LibraryBookBorrowRecords");

            // Event
            modelBuilder.EntitySet<NormalEvent>("NormalEvents");
            modelBuilder.EntitySet<RecurEvent>("RecurEvents");
            var normalEventEntity = modelBuilder.EntityType<NormalEvent>();
            normalEventEntity.Property(c => c.StartDate).AsDate();
            normalEventEntity.Property(c => c.EndDate).AsDate();
            // Action: Mark as completed
            var actionMarkAsCompleted = normalEventEntity.Collection.Action("MarkAsCompleted");
            actionMarkAsCompleted.Parameter<int>("HomeID");
            actionMarkAsCompleted.Parameter<int>("EventID");
            actionMarkAsCompleted.Returns<bool>();
            //// Action: Generate normal events
            //var recurEventEntity = modelBuilder.EntityType<RecurEvent>();
            //var actionGenerateNormalEvents = recurEventEntity.Collection.Action("GenerateNormalEvents");
            //actionGenerateNormalEvents.Parameter<int>("HomeID");
            //actionGenerateNormalEvents.ReturnsFromEntitySet<NormalEvent>("NormalEvents");

            return modelBuilder.GetEdmModel();
        }
    }
}
