﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Deltas;

namespace hihapi.Controllers
{
    [Authorize]
    public sealed class FinanceAccountsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceAccountsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceAccounts
        [EnableQuery]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinanceAccount> option)
        public IActionResult Get()
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

            // Check whether User assigned with specified Home ID
            return Ok(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { hmem.HomeID, hmem.User, hmem.IsChild } into hmems
                      join acnts in _context.FinanceAccount
                        on hmems.HomeID equals acnts.HomeID
                      where (hmems.IsChild == true && hmems.User == acnts.Owner)
                          || !hmems.IsChild.HasValue
                          || hmems.IsChild == false
                      select acnts);
        }

        [EnableQuery]
        [HttpGet]
        public FinanceAccount Get([FromODataUri] Int32 key)
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

            var hidquery = from hmem in _context.HomeMembers
                           where hmem.User == usrName
                           select new { HomeID = hmem.HomeID };
            var acntquery = from acnt in _context.FinanceAccount
                            where acnt.ID == key
                            select acnt;
            return (from acnt in acntquery
                    join hid in hidquery
                    on acnt.HomeID equals hid.HomeID
                    select acnt).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FinanceAccount account)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // User
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

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == account.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!account.IsValid(this._context))
                return BadRequest();

            account.CreatedAt = DateTime.Now;
            account.Createdby = usrName;
            _context.FinanceAccount.Add(account);
            await _context.SaveChangesAsync();

            return Created(account);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceAccount update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("ID mismatched");
            }

            // User
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

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == update.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }
            
            var account = await _context.FinanceAccount.FindAsync(key);
            if (account == null)
                return NotFound();


            if (!update.IsValid(this._context))
                return BadRequest();

            // Checks.
            // 1. Not changes to account category            

            update.Updatedby = usrName;
            update.UpdatedAt = DateTime.Now;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinanceAccount.Any(p => p.ID == key))
                {
                    return NotFound();
                }
                else
                {
                    throw new DBOperationException(exp.Message);
                }
            }

            return Updated(update);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<FinanceAccount> coll)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            var entity = await _context.FinanceAccount.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            // User
            string usrName;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
                if (String.CompareOrdinal(entity.Owner, usrName) != 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Patch it
            coll.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FinanceAccount.Any(p => p.ID == key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(entity);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinanceAccount.FindAsync(key);
            if (cc == null)
            {
                return NotFound();
            }

            // User
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

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == cc.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!cc.IsDeleteAllowed(this._context))
                return BadRequest();

            _context.FinanceAccount.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        /// <summary>
        /// Create a legacy loan account
        /// </summary>
        /// <param name="parameters">
        ///     AccountInfo: Account INfo
        ///     LoanDate: Transaction date
        ///     Amount: Amount on transaction date
        ///     ControlCenterID (?): Control Center ID
        ///     OrderID (?): Order ID
        /// </param>
        /// <returns>New created account</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        [HttpPost]
        public async Task<IActionResult> CreateLegacyLoanAccount([FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }

            // 0. Get inputted parameter
            FinanceAccount accountInfo = (FinanceAccount)parameters["AccountInfo"];
            DateTime tranDate = (DateTime)parameters["LoanDate"];
            Decimal tranAmount = (Decimal)parameters["Amount"];
            Int32? ccid = (Int32?)parameters["ControlCenterID"];
            Int32? orderid = (Int32?)parameters["OrderID"];

            // 1. Check User
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
            // 2a. Check data
            if (accountInfo.CategoryID != FinanceAccountCategory.AccountCategory_BorrowFrom
                || String.IsNullOrEmpty(accountInfo.Name)
                || accountInfo.HomeID == 0)
            {
                return BadRequest();
            }
            // 2b. Check the Home ID
            var homedef = (from dbhomedef in _context.HomeDefines
                           where dbhomedef.ID == accountInfo.HomeID
                           select dbhomedef).SingleOrDefault();
            if (homedef == null)
            {
                throw new UnauthorizedAccessException();
            }
            var hms = _context.HomeMembers.Where(p => p.HomeID == accountInfo.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var loanAccountID = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Create account
                    accountInfo.CreatedAt = DateTime.Now;
                    accountInfo.Createdby = usrName;
                    accountInfo.Status = FinanceAccountStatus.Normal;
                    accountInfo.CategoryID = FinanceAccountCategory.AccountCategory_BorrowFrom;
                    accountInfo.ExtraLoan = new FinanceAccountExtraLoan();
                    accountInfo.ExtraLoan.InterestFree = true;
                    accountInfo.ExtraLoan.StartDate = tranDate;
                    accountInfo.ExtraLoan.RepaymentMethod = LoanRepaymentMethod.InformalPayment;
                    accountInfo.ExtraLoan.AccountHeader = accountInfo;

                    var acntEntity = _context.FinanceAccount.Add(accountInfo);
                    await _context.SaveChangesAsync();
                    loanAccountID = acntEntity.Entity.ID;

                    // Create the document
                    var vmFIDoc = new FinanceDocument();
                    vmFIDoc.HomeID = accountInfo.HomeID;
                    vmFIDoc.Createdby = usrName;
                    vmFIDoc.TranDate = tranDate;
                    vmFIDoc.TranCurr = homedef.BaseCurrency;
                    vmFIDoc.DocType = FinanceDocumentType.DocType_BorrowFrom;
                    vmFIDoc.Desp = accountInfo.Name;
                    vmFIDoc.CreatedAt = DateTime.Now;
                    vmFIDoc.Createdby = usrName;
                    var nitem = new FinanceDocumentItem();
                    nitem.ItemID = 1;
                    nitem.AccountID = loanAccountID;
                    nitem.TranAmount = tranAmount;
                    nitem.Desp = accountInfo.Name;
                    nitem.TranType = FinanceTransactionType.TranType_OpeningAsset;
                    if (ccid.HasValue)
                        nitem.ControlCenterID = ccid.Value;
                    if (orderid.HasValue)
                        nitem.OrderID = orderid.Value;
                    nitem.DocumentHeader = vmFIDoc;

                    var docEntity = _context.FinanceDocument.Add(vmFIDoc);
                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    // Update the account
                    var extraEntry = _context.FinanceAccountExtraLoan.Find(loanAccountID);
                    if (extraEntry != null)
                    {
                        extraEntry.RefDocID = origdocid;                        
                    }

                    await _context.SaveChangesAsync();

                    transaction.Commit();
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
                throw new BadRequestException(errorString);
            }

            return Created(accountInfo);
        }

        /// <summary>
        /// Close an account
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        [HttpPost]
        public async Task<IActionResult> CloseAccount([FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }

            // 0. Get inputted parameter
            Int32 hid = (Int32)parameters["HomeID"];
            Int32 accountID = (Int32)parameters["AccountID"];

            // 1. Check User
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
            // 2. Check the Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }
            // 3. Check the account
            var acntDB = await _context.FinanceAccount.FindAsync(accountID);
            bool ret = false;
            if (acntDB != null)
            {
                ret = acntDB.IsCloseAllowed(this._context);

                if (ret)
                {
                    acntDB.Status = FinanceAccountStatus.Closed;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(ret);
        }

        [HttpPost]
        public async Task<IActionResult> SettleAccount([FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }

            // 0. Get inputted parameter
            Int32 hid = (Int32)parameters["HomeID"];
            Int32 accountID = (Int32)parameters["AccountID"];
            Int32 ccID = (Int32)parameters["ControlCenterID"];
            DateTime dateSettle = DateTime.Parse((String)parameters["SettledDate"]);
            Decimal amt = (Decimal)parameters["InitialAmount"];
            String curr = (String)parameters["Currency"];

            // 1. Check User
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
            // 2. Check the Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }
            // 3. Check the account
            var acntDB = await _context.FinanceAccount.FindAsync(accountID);
            bool ret = true;
            if (acntDB != null)
            {
                // 4. Check account status
                if (acntDB.Status != FinanceAccountStatus.Normal)
                {
                    ret = false;
                }

                // 5. Find documents which are earlier than the specified date
                int docCounts = 0;
                if (ret)
                {
                    docCounts = (from docitem in _context.FinanceDocumentItem
                                     join docheader in _context.FinanceDocument
                                         on docitem.DocID equals docheader.ID
                                     where docitem.AccountID == accountID
                                         && docheader.TranDate < dateSettle
                                         && docheader.HomeID == hid
                                     select docheader.ID).Count();
                    if (docCounts > 0)
                        ret = false;
                }

                // 6. Check there is already an item with same tran. type.
                if (ret)
                {
                    docCounts = (from docitem in _context.FinanceDocumentItem
                                 join docheader in _context.FinanceDocument
                                     on docitem.DocID equals docheader.ID
                                 where docitem.AccountID == accountID
                                    && docheader.HomeID == hid
                                    && ( docitem.TranType == FinanceTransactionType.TranType_OpeningAsset
                                    || docitem.TranType == FinanceTransactionType.TranType_OpeningLiability )
                                 select docheader.ID).Count();
                    if (docCounts > 0)
                        ret = false;
                }

                // 7. Create a document
                if (ret)
                {
                    try
                    {
                        if (acntDB.CategoryID == FinanceAccountCategory.AccountCategory_Cash
                            || acntDB.CategoryID == FinanceAccountCategory.AccountCategory_Creditcard
                            || acntDB.CategoryID == FinanceAccountCategory.AccountCategory_VirtualAccount)
                        {
                            FinanceDocument doc = new FinanceDocument();
                            doc.TranCurr = curr;
                            doc.HomeID = hid;
                            doc.CreatedAt = DateTime.Now;
                            doc.Createdby = usrName;
                            doc.Desp = "Settle count for account";
                            doc.DocType = FinanceDocumentType.DocType_Normal;
                            doc.TranDate = dateSettle;
                            FinanceDocumentItem docitem = new FinanceDocumentItem();
                            docitem.DocumentHeader = doc;
                            docitem.AccountID = accountID;
                            docitem.ControlCenterID = ccID;
                            docitem.Desp = doc.Desp;
                            docitem.ItemID = 1;
                            docitem.TranAmount = amt;
                            if (amt > 0)
                                docitem.TranType = FinanceTransactionType.TranType_OpeningAsset;
                            else
                                docitem.TranType = FinanceTransactionType.TranType_OpeningLiability;
                            docitem.DocumentHeader = doc;
                            doc.Items.Add(docitem);

                            // Save it to DB
                            _context.FinanceDocument.Add(doc);

                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            else
            {
                return NotFound();
            }

            return Ok(ret);
        }
    }
}

