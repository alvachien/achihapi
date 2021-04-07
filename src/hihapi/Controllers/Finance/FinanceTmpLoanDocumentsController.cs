using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public sealed class FinanceTmpLoanDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceTmpLoanDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceTmpLoanDocument> Get()
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            return _context.FinanceTmpLoanDocument;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostRepayDocument([FromBody]FinanceLoanRepayDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Checks
            // Check 0: Input data check
            if (createContext.HomeID <= 0
                || createContext.LoanTemplateDocumentID <= 0
                || createContext.DocumentInfo == null
                || createContext.DocumentInfo.DocType != FinanceDocumentType.DocType_Repay)
            {
                throw new BadRequestException("Invalid inputted data");
            }

            // Check 1: User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check 1: Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Check 2: Template doc
            var docLoanTmp = _context.FinanceTmpLoanDocument
                .Where(p => p.DocumentID == createContext.LoanTemplateDocumentID)
                .FirstOrDefault();
            if (docLoanTmp == null
                || docLoanTmp.ReferenceDocumentID > 0)
            {
                throw new BadRequestException("Invalid loan template ID or doc has been posted");
            }
            if (!docLoanTmp.ControlCenterID.HasValue && !docLoanTmp.OrderID.HasValue)
            {
                throw new BadRequestException("Tmp doc lack of control center or order");
            }
            if (docLoanTmp.TransactionAmount == 0)
            {
                throw new BadRequestException("Tmp doc lack of amount");
            }

            // Check 3: Account
            var loanAccountHeader = _context.FinanceAccount.Where(p => p.ID == docLoanTmp.AccountID).FirstOrDefault();
            if (loanAccountHeader == null
                || loanAccountHeader.Status != (Byte)FinanceAccountStatus.Normal
                || !(loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_BorrowFrom
                    || loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_LendTo)
                    )
            {
                throw new BadRequestException("Account not exist or account has wrong status or account has wrong category");
            }
            var loanAccountExtra = _context.FinanceAccountExtraLoan.Where(p => p.AccountID == loanAccountHeader.ID).FirstOrDefault();
            if (loanAccountExtra == null)
            {
                throw new BadRequestException("Account is not a valid loan account");
            }

            // Check 4: Check amounts
            int ninvaliditems = 0;
            Decimal acntBalance = 0M;
            // Balance
            var acntBalInfo = _context.FinanceReporAccountGroupAndExpenseView.Where(p => p.AccountID == loanAccountHeader.ID).ToList();
            if (acntBalInfo.Count > 0)
            {
                acntBalance = 0;
                acntBalInfo.ForEach(action =>
                {
                    if (action.IsExpense)
                    {
                        acntBalance += action.Balance;
                    }
                    else
                    {
                        acntBalance += action.Balance;
                    }
                });
            }
            else
            {
                throw new BadRequestException("No balance");
            }

            // Only four tran. types are allowed
            if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_BorrowFrom)
            {
                ninvaliditems = createContext.DocumentInfo.Items.Where(item => item.TranType != FinanceTransactionType.TranType_InterestOut
                    && item.TranType != FinanceTransactionType.TranType_RepaymentOut
                    && item.TranType != FinanceTransactionType.TranType_RepaymentIn)
                    .Count();
            }
            else if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_LendTo)
            {
                ninvaliditems = createContext.DocumentInfo.Items.Where(item => item.TranType != FinanceTransactionType.TranType_InterestIn
                    && item.TranType != FinanceTransactionType.TranType_RepaymentOut
                    && item.TranType != FinanceTransactionType.TranType_RepaymentIn)
                    .Count();
            }
            if (ninvaliditems > 0)
            {
                throw new BadRequestException("Items with invalid tran type");
            }

            // Check the amount
            decimal totalOut = createContext.DocumentInfo.Items
                .Where(item => item.TranType == FinanceTransactionType.TranType_RepaymentOut)
                .Sum(item2 => item2.TranAmount);
            decimal totalIn = createContext.DocumentInfo.Items
                .Where(item => item.TranType == FinanceTransactionType.TranType_RepaymentIn)
                .Sum(item2 => item2.TranAmount);
            //decimal totalintOut = repaydoc.Items.Where(item => (item.TranType == FinanceTranTypeViewModel.TranType_InterestOut)).Sum(item2 => item2.TranAmount);
            // New account balance
            if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_LendTo)
            {
                acntBalance -= totalOut;
            }
            else if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_BorrowFrom)
            {
                acntBalance += totalIn;
            }
            if (totalOut != totalIn)
            {
                throw new BadRequestException("Amount is not equal!");
            }

            // The post here is:
            // 1. Post a repayment document with the content from this template doc
            // 2. Update the template doc with REFDOCID
            // 3. If the account balance is zero, close the account;

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            FinanceDocument findoc = null;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    createContext.DocumentInfo.Createdby = usrName;
                    createContext.DocumentInfo.CreatedAt = DateTime.Now;
                    var docEntity = _context.FinanceDocument.Add(createContext.DocumentInfo);
                    _context.SaveChanges();
                    origdocid = docEntity.Entity.ID;

                    // 2. Update the tmp doc
                    docLoanTmp.ReferenceDocumentID = origdocid;
                    docLoanTmp.UpdatedAt = DateTime.Now;
                    docLoanTmp.Updatedby = usrName;
                    _context.FinanceTmpLoanDocument.Update(docLoanTmp);
                    _context.SaveChanges();

                    // 3. In case balance is zero, update the account status
                    if (Decimal.Compare(acntBalance, 0) == 0)
                    {
                        loanAccountHeader.Status = (Byte)FinanceAccountStatus.Closed;
                        loanAccountHeader.Updatedby = usrName;
                        loanAccountHeader.UpdatedAt = DateTime.Now;
                        _context.FinanceAccount.Update(loanAccountHeader);
                        _context.SaveChanges();
                    }

                    findoc = docEntity.Entity;

                    await transaction.CommitAsync();
                }
                catch (Exception exp)
                {
                    errorOccur = true;
                    errorString = exp.Message;
                    transaction.Rollback();
                }
            }

            if (errorOccur)
            {
                throw new DBOperationException(errorString);
            }

            return Created(findoc);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostPrepaymentDocument([FromBody]FinanceLoanPrepayDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Checks
            // Check 0: Input data check
            if (createContext.HomeID <= 0
                || createContext.LoanAccountID <= 0
                || createContext.DocumentInfo == null
                || createContext.DocumentInfo.DocType != FinanceDocumentType.DocType_Repay)
            {
                throw new BadRequestException("Invalid inputted data");
            }

            // Check 1: User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check 2: Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Check 2: Account
            var loanAccountHeader = _context.FinanceAccount.Where(p => p.ID == createContext.LoanAccountID).FirstOrDefault();
            if (loanAccountHeader == null
                || loanAccountHeader.Status != (Byte)FinanceAccountStatus.Normal
                || !(loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_BorrowFrom
                    || loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_LendTo)
                    )
            {
                throw new BadRequestException("Account not exist or account has wrong status or account has wrong category");
            }
            var loanAccountExtra = _context.FinanceAccountExtraLoan.Where(p => p.AccountID == loanAccountHeader.ID).FirstOrDefault();
            if (loanAccountExtra == null)
            {
                throw new BadRequestException("Account is not a valid loan account");
            }

            // Check 3: Check amounts
            int ninvaliditems = 0;
            Decimal acntBalance = 0M;
            // Balance
            var acntBalInfo = _context.FinanceReporAccountGroupAndExpenseView.Where(p => p.AccountID == loanAccountHeader.ID).ToList();
            if (acntBalInfo.Count > 0)
            {
                acntBalance = 0;
                acntBalInfo.ForEach(action =>
                {
                    if (action.IsExpense)
                    {
                        acntBalance += action.Balance;
                    }
                    else
                    {
                        acntBalance += action.Balance;
                    }
                });
            }
            else
            {
                throw new BadRequestException("No balance");
            }

            // Only four tran. types are allowed
            if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_BorrowFrom)
            {
                ninvaliditems = createContext.DocumentInfo.Items.Where(item => item.TranType != FinanceTransactionType.TranType_InterestOut
                    && item.TranType != FinanceTransactionType.TranType_RepaymentOut
                    && item.TranType != FinanceTransactionType.TranType_RepaymentIn)
                    .Count();
            }
            else if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_LendTo)
            {
                ninvaliditems = createContext.DocumentInfo.Items.Where(item => item.TranType != FinanceTransactionType.TranType_InterestIn
                    && item.TranType != FinanceTransactionType.TranType_RepaymentOut
                    && item.TranType != FinanceTransactionType.TranType_RepaymentIn)
                    .Count();
            }
            if (ninvaliditems > 0)
            {
                throw new BadRequestException("Items with invalid tran type");
            }

            // Check the amount
            decimal totalOut = createContext.DocumentInfo.Items
                .Where(item => item.TranType == FinanceTransactionType.TranType_RepaymentOut)
                .Sum(item2 => item2.TranAmount);
            decimal totalIn = createContext.DocumentInfo.Items
                .Where(item => item.TranType == FinanceTransactionType.TranType_RepaymentIn)
                .Sum(item2 => item2.TranAmount);
            //decimal totalintOut = repaydoc.Items.Where(item => (item.TranType == FinanceTranTypeViewModel.TranType_InterestOut)).Sum(item2 => item2.TranAmount);
            // New account balance
            if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_LendTo)
            {
                acntBalance -= totalOut;
            }
            else if (loanAccountHeader.CategoryID == FinanceAccountCategory.AccountCategory_BorrowFrom)
            {
                acntBalance += totalIn;
            }
            if (totalOut != totalIn)
            {
                throw new BadRequestException("Amount is not equal!");
            }

            // The post here is:
            // 1. Post a repayment document with the content from this template doc
            // 2. Update the template doc with REFDOCID
            // 3. If the account balance is zero, close the account;

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            FinanceDocument findoc = null;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    createContext.DocumentInfo.Createdby = usrName;
                    createContext.DocumentInfo.CreatedAt = DateTime.Now;
                    var docEntity = _context.FinanceDocument.Add(createContext.DocumentInfo);
                    _context.SaveChanges();
                    origdocid = docEntity.Entity.ID;
                    _context.SaveChanges();

                    // 2. In case balance is zero, update the account status
                    if (Decimal.Compare(acntBalance, 0) == 0)
                    {
                        loanAccountHeader.Status = (Byte)FinanceAccountStatus.Closed;
                        loanAccountHeader.Updatedby = usrName;
                        loanAccountHeader.UpdatedAt = DateTime.Now;
                        _context.FinanceAccount.Update(loanAccountHeader);
                        _context.SaveChanges();
                    }

                    findoc = docEntity.Entity;

                    await transaction.CommitAsync();
                }
                catch (Exception exp)
                {
                    errorOccur = true;
                    errorString = exp.Message;
                    transaction.Rollback();
                }
            }

            if (errorOccur)
            {
                throw new DBOperationException(errorString);
            }

            return Created(findoc);
        }
    }
}
