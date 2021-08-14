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
            var acntBalRpt = modelBuilder.EntitySet<FinanceReportByAccount>("FinanceReportByAccounts");
            acntBalRpt.EntityType.HasKey(p => new {
                p.HomeID,
                p.AccountID,
            });
            var ccBalRpt = modelBuilder.EntitySet<FinanceReportByControlCenter>("FinanceReportByControlCenters");
            ccBalRpt.EntityType.HasKey(p => new
            {
                p.HomeID,
                p.ControlCenterID,
            });
            var ordBalRpt = modelBuilder.EntitySet<FinanceReportByOrder>("FinanceReportByOrders");
            ordBalRpt.EntityType.HasKey(p => new
            {
                p.HomeID,
                p.OrderID,
            });
            modelBuilder.ComplexType<RepeatedDates>();
            modelBuilder.ComplexType<RepeatDatesCalculationInput>();
            modelBuilder.ComplexType<RepeatedDatesWithAmount>();
            modelBuilder.ComplexType<RepeatDatesWithAmountCalculationInput>();

            // Function on Album - Get Photos
            //var functionWithOptional = builder.EntityType<Album>().Collection.Function("GetPhotos").ReturnsCollectionFromEntitySet<Photo>("Photos");
            //functionWithOptional.Parameter<int>("AlbumID");
            //functionWithOptional.Parameter<string>("AccessCode").Optional();

            //// Function on Album - GetAlbumPhotos
            //var funcOnEntity = builder.EntityType<Album>().Function("GetRelatedPhotos").ReturnsCollectionFromEntitySet<Photo>("Photos");
            //funcOnEntity.Parameter<string>("AccessCode");

            //// Action on Album - Change Access Code
            //var action = builder.EntityType<Album>().Action("ChangeAccessCode");
            //action.Parameter<string>("AccessCode"); // .Optional();
            //action.Returns<Boolean>();

            //// Function on Phto view
            //var functionWithOpt = builder.EntityType<PhotoView>().Collection.Function("SearchPhotoInAlbum").ReturnsCollectionFromEntitySet<PhotoView>("PhotoViews");
            //functionWithOpt.Parameter<int>("AlbumID");
            //functionWithOpt.Parameter<string>("AccessCode").Optional();


            // Function on root
            //// using attribute routing
            //var unboundFunction = builder.Function("CalculateSalary").Returns<string>();
            //unboundFunction.Parameter<int>("minSalary");
            //unboundFunction.Parameter<int>("maxSalary").Optional();
            //unboundFunction.Parameter<string>("wholeName").HasDefaultValue("abc");
            //return builder.GetEdmModel();

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

            // Actions on Documents
            //var docEntity = modelBuilder.EntityType<FinanceDocument>();
            //docEntity.Property(c => c.TranDate).AsDate();
            //docEntity.Collection
            //    .Action("PostDPDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //docEntity.Collection
            //    .Action("PostLoanDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //docEntity.Collection
            //    .Action("PostAssetBuyDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //docEntity.Collection
            //    .Action("PostAssetSellDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //docEntity.Collection
            //    .Action("PostAssetValueChangeDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //modelBuilder.Namespace = typeof(Currency).Namespace;
            //// Function on DP template documents
            //var tmpTpDocEntity = modelBuilder.EntityType<FinanceTmpDPDocument>();
            //var tmpTpDocPostFunc =
            //    tmpTpDocEntity.Collection.Action("PostDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //// Action on Loan template documents: Repay document
            //var tmpLoanDocEntity = modelBuilder.EntityType<FinanceTmpLoanDocument>();
            //tmpLoanDocEntity.Collection
            //    .Action("PostRepayDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");
            //tmpLoanDocEntity.Collection
            //    .Action("PostPrepaymentDocument")
            //    .ReturnsFromEntitySet<FinanceDocument>("FinanceDocuments");

            modelBuilder.EntitySet<BlogFormat>("BlogFormats");
            modelBuilder.EntitySet<BlogUserSetting>("BlogUserSettings");
            modelBuilder.EntitySet<BlogCollection>("BlogCollections");
            modelBuilder.EntitySet<BlogPost>("BlogPosts");
            modelBuilder.EntitySet<BlogPostCollection>("BlogPostCollections");
            modelBuilder.EntitySet<BlogPostTag>("BlogPostTags");
            // Functions
            var postentity = modelBuilder.EntityType<BlogPost>();
            postentity.Function("Deploy")
                    .Returns<string>()
                    ;
            postentity.Function("ClearDeploy")
                    .Returns<string>();
            var blogsetting = modelBuilder.EntityType<BlogUserSetting>();
            blogsetting.Function("Deploy")
                    .Returns<string>();

            return modelBuilder.GetEdmModel();
        }

    }
}
