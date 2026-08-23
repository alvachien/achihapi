using System;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Controllers;
using hihapi.Exceptions;
using hihapi.Models;
using hihapi.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
        public async Task Get_DoesNotLeakCrossTenantTmpLoanDocs()
        {
            var context = fixture.GetCurrentDataContext();
            fixture.InitHomeTestData(DataSetupUtility.Home1ID, context);

            // Build the minimal FK chain a T_FIN_TMPDOC_LOAN row requires:
            // a finance account -> its extra-loan row -> the tmp doc. All in Home 2,
            // which UserA (a Home 1 member only) must never see via Get().
            var loanAccount = new FinanceAccount
            {
                HomeID = DataSetupUtility.Home2ID,
                CategoryID = FinanceAccountCategory.AccountCategory_Cash,
                Status = FinanceAccountStatus.Normal,
                Owner = DataSetupUtility.UserB,
                Name = "CrossTenantLoanTest",
            };
            context.FinanceAccount.Add(loanAccount);
            context.SaveChanges(); // assign the auto-increment ID

            var extLoan = new FinanceAccountExtraLoan { AccountID = loanAccount.ID };
            var foreignDoc = new FinanceTmpLoanDocument
            {
                DocumentID = 90001,
                HomeID = DataSetupUtility.Home2ID,
                AccountID = loanAccount.ID,
                TransactionDate = DateTime.Today,
                TransactionAmount = 100m,
            };
            context.FinanceAccountExtraLoan.Add(extLoan);
            context.FinanceTmpLoanDocument.Add(foreignDoc);
            context.SaveChanges();

            var control = new FinanceTmpLoanDocumentsController(context);
            control.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = DataSetupUtility.GetClaimForUser(DataSetupUtility.UserA) }
            };

            try
            {
                var result = Assert.IsType<OkObjectResult>(control.Get());
                var docs = Assert.IsAssignableFrom<IQueryable<FinanceTmpLoanDocument>>(result.Value).ToList();
                // UserA is only a member of Home 1; the Home 2 tmp loan doc must be excluded.
                Assert.DoesNotContain(docs, d => d.DocumentID == foreignDoc.DocumentID);
                Assert.True(docs.All(d => d.HomeID != DataSetupUtility.Home2ID));
            }
            finally
            {
                // Cleanup the shared in-memory DB so other tests are unaffected.
                context.FinanceTmpLoanDocument.Remove(foreignDoc);
                context.FinanceAccountExtraLoan.Remove(extLoan);
                context.FinanceAccount.Remove(loanAccount);
                context.SaveChanges();
            }

            await context.DisposeAsync();
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
