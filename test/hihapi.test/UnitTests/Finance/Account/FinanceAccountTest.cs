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
    public class FinanceAccountTest
    {
        private SqliteDatabaseFixture fixture = null;

        public FinanceAccountTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        public static TheoryData<FinanceAccountTestData> AccountTestData =>
            new TheoryData<FinanceAccountTestData>
            {
                new FinanceAccountTestData()
                {
                    ExpectedValidResult = false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    ExpectedValidResult = false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    ExpectedValidResult= false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    Name = "Test 1",
                    ExpectedValidResult= true,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= true,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_AdvancePayment,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                },
                new FinanceAccountTestData()
                {
                    HomeID = DataSetupUtility.Home1ID,
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Name = "Test 1",
                    Comment = "Comment 1",
                    ExpectedValidResult= false,
                },
            };

        [Theory]
        [MemberData(nameof(AccountTestData))]
        public async Task TestCase_IsValid(FinanceAccountTestData testData)
        {
            FinanceAccount acnt = new FinanceAccount();
            acnt.HomeID = testData.HomeID;
            acnt.Name = testData.Name;
            acnt.Owner = testData.Owner;
            acnt.Comment = testData.Comment;
            acnt.CategoryID = testData.CategoryID;
            acnt.Status = testData.Status;

            acnt.ExtraAsset = testData.ExtraAsset;
            acnt.ExtraDP = testData.ExtraDP;
            acnt.ExtraLoan = testData.ExtraLoan;

            var context = this.fixture.GetCurrentDataContext();
            var isValid = acnt.IsValid(context);

            Assert.Equal(testData.ExpectedValidResult, isValid);

            await context.DisposeAsync();
        }

        public static TheoryData<FinanceAccountTestData> IsClosedAllowedTestData =>
            new TheoryData<FinanceAccountTestData>
            {
                new FinanceAccountTestData()
                {
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Creditcard,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Deposit,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_VirtualAccount,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = (byte)FinanceAccountStatus.Closed,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = (byte)FinanceAccountStatus.Frozen,
                    ExpectedIsCloseAllowedResult = false,
                },
                new FinanceAccountTestData()
                {
                    CategoryID = FinanceAccountCategory.AccountCategory_Asset,
                    Status = (byte)FinanceAccountStatus.Normal,
                    ExpectedIsCloseAllowedResult = true,
                },
            };

        [Theory]
        [MemberData(nameof(IsClosedAllowedTestData))]
        public async Task TestCase_IsCloseAllowed(FinanceAccountTestData testData)
        {
            FinanceAccount acnt = new FinanceAccount();
            acnt.HomeID = testData.HomeID;
            acnt.Name = testData.Name;
            acnt.Owner = testData.Owner;
            acnt.Comment = testData.Comment;
            acnt.CategoryID = testData.CategoryID;
            acnt.Status = testData.Status;

            acnt.ExtraAsset = testData.ExtraAsset;
            acnt.ExtraDP = testData.ExtraDP;
            acnt.ExtraLoan = testData.ExtraLoan;

            var context = this.fixture.GetCurrentDataContext();
            var isAllowed = acnt.IsCloseAllowed(context);

            Assert.Equal(testData.ExpectedIsCloseAllowedResult, isAllowed);

            await context.DisposeAsync();
        }
    }
}
