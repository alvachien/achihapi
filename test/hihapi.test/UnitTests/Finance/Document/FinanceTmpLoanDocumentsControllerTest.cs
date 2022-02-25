using System;
using Xunit;
using System.Linq;
using hihapi.Models;
using hihapi.Controllers;
using hihapi.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.EntityFrameworkCore;
using hihapi.test.common;
using hihapi.Exceptions;

namespace hihapi.unittest.Finance
{
    [Collection("HIHAPI_UnitTests#1")]
    public class FinanceTmpLoanDocumentsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public FinanceTmpLoanDocumentsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase_GetWithInvalidUser()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinanceTmpLoanDocumentsController(context);

            try
            {
                control.Get();
            }
            catch (Exception ex)
            {
                Assert.IsType<UnauthorizedAccessException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostRepayDocumentWithInvalidModelState()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinanceTmpLoanDocumentsController(context);
            control.ModelState.AddModelError("Desp", "The Desp field is required.");

            try
            {
                await control.PostRepayDocument(new FinanceLoanRepayDocumentCreateContext());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostRepayDocumentWithInvalidInput()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinanceTmpLoanDocumentsController(context);

            try
            {
                await control.PostRepayDocument(new FinanceLoanRepayDocumentCreateContext());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostPrepaymentDocumenttWithInvalidModelState()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinanceTmpLoanDocumentsController(context);
            control.ModelState.AddModelError("Desp", "The Desp field is required.");

            try
            {
                await control.PostPrepaymentDocument(new FinanceLoanPrepayDocumentCreateContext());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }

        [Fact]
        public async Task TestCase_PostPrepaymentDocumenttWithInvalidInput()
        {
            var context = fixture.GetCurrentDataContext();

            var control = new FinanceTmpLoanDocumentsController(context);

            try
            {
                await control.PostPrepaymentDocument(new FinanceLoanPrepayDocumentCreateContext());
            }
            catch (Exception ex)
            {
                Assert.IsType<BadRequestException>(ex);
            }

            await context.DisposeAsync();
        }
    }
}
