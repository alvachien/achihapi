using System;
using Xunit;
using hihapi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using hihapi.test.common;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceDocumentTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceDocumentTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<FinanceDocumentTestData> DocumentTestData =>
            new TheoryData<FinanceDocumentTestData>
            {
                new FinanceDocumentTestData
                {
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 1,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 2,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 3,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 4,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 5,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 6,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = 1,
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 7,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 8,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = 9999
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 9,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Income1
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 10,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Income1,
                            TranAmount = 100,
                        },
                    },
                    ExpectedIsValidResult = true,
                    TestCaseIndex = 11,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Transfer,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Income1,
                            TranAmount = 100,
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 12,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Transfer,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = DataSetupUtility.TranType_Income1,
                            TranAmount = 100,
                        },
                        new FinanceDocumentItem
                        {
                            ItemID = 2,
                            AccountID = DataSetupUtility.Home1CashAccount2ID,
                            TranType = DataSetupUtility.TranType_Expense1,
                            TranAmount = 100,
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 13,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Transfer,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = FinanceTransactionType.TranType_TransferIn,
                            TranAmount = 100,
                        },
                        new FinanceDocumentItem
                        {
                            ItemID = 2,
                            AccountID = DataSetupUtility.Home1CashAccount2ID,
                            TranType = FinanceTransactionType.TranType_TransferOut,
                            TranAmount = 100,
                        },
                    },
                    ExpectedIsValidResult = true,
                    TestCaseIndex = 14,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Transfer,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    Items = new List<FinanceDocumentItem>
                    {
                        new FinanceDocumentItem
                        {
                            ItemID = 1,
                            AccountID = DataSetupUtility.Home1CashAccount1ID,
                            TranType = FinanceTransactionType.TranType_TransferIn,
                            TranAmount = 100,
                        },
                        new FinanceDocumentItem
                        {
                            ItemID = 2,
                            AccountID = DataSetupUtility.Home1CashAccount2ID,
                            TranType = FinanceTransactionType.TranType_TransferOut,
                            TranAmount = 10,
                        },
                    },
                    ExpectedIsValidResult = false,
                    TestCaseIndex = 15,
                },
            };

        [Theory]
        [MemberData(nameof(DocumentTestData))]
        public async Task TestCase_CheckValid(FinanceDocumentTestData testdata)
        {
            FinanceDocument doc = new FinanceDocument();
            doc.ID = testdata.ID;
            doc.HomeID = testdata.HomeID;
            doc.Desp = testdata.Desp;
            doc.DocType = testdata.DocType;
            doc.ExgRate = testdata.ExgRate;
            doc.ExgRate2 = testdata.ExgRate2;
            doc.ExgRate_Plan = testdata.ExgRate_Plan;
            doc.ExgRate_Plan2 = testdata.ExgRate_Plan2;
            doc.TranCurr = testdata.TranCurr;
            doc.TranCurr2 = testdata.TranCurr2;
            doc.TranDate = testdata.TranDate;
            testdata.Items.ForEach(item => doc.Items.Add(item));

            var context = this.fixture.GetCurrentDataContext();
            if (testdata.HomeID == DataSetupUtility.Home1ID)
            {
                fixture.InitHome1TestData(context);
            }
            else if (testdata.HomeID == DataSetupUtility.Home2ID)
            {
                fixture.InitHome2TestData(context);
            }

            // Valid.
            var isValid = doc.IsValid(context);

            Assert.Equal(testdata.ExpectedIsValidResult, isValid);

            await context.DisposeAsync();
        }
    }
}
