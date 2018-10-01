# Design

Due to the fact that Response Cache shall not be used in Authorized data, the Response Cache shall not be used.

## Without Response Cache enabled
Here comes the list of the Cache key which used in controller:
Fin_Currency, 1200 seconds;
Fin_AcntCtgyList_{0}, 1200 seconds; Where {0} is home id;
Fin_AssetCtgyList_{0}, 1200 seconds; Where {0} is home id;
Fin_DocTypeList_{0}, 1200 seconds, Where {0} is home id;
Fin_TranTypeList_{0}, 1200 seconds, Where {0} is home id;
Fin_Report_BS_{0}, 1200 seconds, Where {0} is home id;
HomeDefList_{0}_{1}_{2}, 600 seconds; Where {0} is User ID, {1} is Top, {2} is Skip;
HomeDef_{0}, 600 seconds; Where {0} is home id;

## With Response Cache enabled
### Controllers with Response Cache enabled
FinanceCurrencyController
LanguageController
FinanceAccountCategoryController
FinanceAssetCategoryController
FinanceDocTypeController
FinanceTranTypeController
LearnCategoryController

### Controlles with In-Memory Cache enabled
FinanceReportBSController


# Others
