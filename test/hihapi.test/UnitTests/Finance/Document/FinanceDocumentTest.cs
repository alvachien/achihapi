using System;
using Xunit;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using hihapi.Models;
using hihapi.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Results;

namespace hihapi.test.UnitTests.Finance
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
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    ExpectedIsValidResult = false,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    ExpectedIsValidResult = false,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    ExpectedIsValidResult = false,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    ExpectedIsValidResult = false,
                },
                new FinanceDocumentTestData
                {
                    HomeID = DataSetupUtility.Home1ID,
                    DocType = FinanceDocumentType.DocType_Normal,
                    Desp = "Test",
                    TranCurr = "CNY",
                    TranDate = DateTime.Today,
                    ExpectedIsValidResult = false,                    
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
