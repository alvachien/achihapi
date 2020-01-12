using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;
using Microsoft.AspNet.OData.Query;

namespace hihapi.Controllers
{
    public class FinanceDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceDocuments
        [Authorize]
        public IQueryable Get(ODataQueryOptions<FinanceDocument> option)
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
            var query = from hmem in _context.HomeMembers
                        where hmem.User == usrName
                        select new { HomeID = hmem.HomeID } into hids
                        join docs in _context.FinanceDocument on hids.HomeID equals docs.HomeID
                        select docs;

            return option.ApplyTo(query);
        }

        [EnableQuery]
        [Authorize]
        public SingleResult<FinanceDocument> Get([FromODataUri]Int32 docid)
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
            var docquery = from doc in _context.FinanceDocument
                          where doc.ID == docid
                          select doc;
            var rstquery = from doc in docquery
                           join hid in hidquery
                           on doc.HomeID equals hid.HomeID
                           select doc;

            return SingleResult.Create(rstquery);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceDocument document)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest();
            }

            // Check document type, DP, Asset, loan document is not allowed
            if(document.DocType == FinanceDocumentType.DocType_AdvancePayment
                || document.DocType == FinanceDocumentType.DocType_AdvanceReceive
                || document.DocType == FinanceDocumentType.DocType_AssetBuyIn
                || document.DocType == FinanceDocumentType.DocType_AssetSoldOut
                || document.DocType == FinanceDocumentType.DocType_AssetValChg
                || document.DocType == FinanceDocumentType.DocType_BorrowFrom
                || document.DocType == FinanceDocumentType.DocType_LendTo)
            {
                return BadRequest("Document type is not allowed");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == document.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!document.IsValid(this._context))
                return BadRequest();

            _context.FinanceDocument.Add(document);
            await _context.SaveChangesAsync();

            return Created(document);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody]FinanceDocument update)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest();
            }
            if (key != update.ID)
            {
                return BadRequest();
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

            if (!update.IsValid(this._context))
                return BadRequest();

            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinanceDocument.Any(p => p.ID == key))
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

        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinanceDocument.FindAsync(key);
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
                {
                    throw new UnauthorizedAccessException();
                }
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

            _context.FinanceDocument.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostDPDocument(int HomeID, [FromBody]FinanceADPDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest("Model State is Invalid");
            }
            if (createContext == null || createContext.DocumentInfo == null || createContext.AccountInfo == null 
                || createContext.AccountInfo.ExtraDP == null
                || createContext.AccountInfo.ExtraDP.DPTmpDocs.Count <= 0)
            {
                return BadRequest("Invalid inputted data");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Verify the inputted parameters
            if (!(createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvancePayment
                || createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvanceReceive))
            {
                return BadRequest("Invalid document type");
            }
            if (createContext.DocumentInfo.Items.Count != 1)
            {
                return BadRequest("It shall be only one line item in DP docs");
            }
            // Check on the data
            if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvancePayment)
            {
                if (createContext.DocumentInfo.Items.ElementAt(0).TranType != FinanceTransactionType.TranType_AdvancePaymentOut)
                    return BadRequest("Invalid tran. type for advance payment");
            }
            else if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvanceReceive)
            {
                if (createContext.DocumentInfo.Items.ElementAt(0).TranType != FinanceTransactionType.TranType_AdvanceReceiveIn)
                    return BadRequest("Invalid tran. type for advance receive");
            }
            foreach (var tmpdocitem in createContext.AccountInfo.ExtraDP.DPTmpDocs)
            {
                if (!tmpdocitem.ControlCenterID.HasValue && !tmpdocitem.OrderID.HasValue)
                {
                    return BadRequest("Tmp Doc Item miss control center or order");
                }
                if (tmpdocitem.TranAmount == 0)
                {
                    return BadRequest("Tmp Doc Item miss amount");
                }
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var dpaccountid = 0;
            try
            {
                var docEntity = _context.FinanceDocument.Add(createContext.DocumentInfo);
                await _context.SaveChangesAsync();
                origdocid = docEntity.Entity.ID;

                // 2, Create the account
                createContext.AccountInfo.ExtraDP.RefenceDocumentID = origdocid;
                var acntEntity = _context.FinanceAccount.Add(createContext.AccountInfo);
                await _context.SaveChangesAsync();
                dpaccountid = acntEntity.Entity.ID;

                // 3, Update the document
                var itemid = docEntity.Entity.Items.ElementAt(0).ItemID;
                // _context.Attach(docEntity);
                
                var ndi = new FinanceDocumentItem();
                ndi.ItemID = ++itemid;
                ndi.AccountID = dpaccountid;
                ndi.ControlCenterID = docEntity.Entity.Items.ElementAt(0).ControlCenterID;
                ndi.OrderID = docEntity.Entity.Items.ElementAt(0).OrderID;
                ndi.Desp = docEntity.Entity.Items.ElementAt(0).Desp;
                ndi.TranAmount = docEntity.Entity.Items.ElementAt(0).TranAmount;
                ndi.UseCurr2 = docEntity.Entity.Items.ElementAt(0).UseCurr2;
                if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvancePayment)
                    ndi.TranType = FinanceTransactionType.TranType_OpeningAsset;
                else if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvanceReceive)
                    ndi.TranType = FinanceTransactionType.TranType_OpeningLiability;
                docEntity.Entity.Items.Add(ndi);

                docEntity.State = EntityState.Modified;
                
                await _context.SaveChangesAsync();
            }
            catch(Exception exp)
            {
                errorOccur = true;
                errorString = exp.Message;
            }
            finally
            {
                // Remove new created object
                if (errorOccur)
                {
                    try
                    {
                        if (origdocid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_document WHERE ID = " + origdocid.ToString());
                        }
                        if (dpaccountid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_account WHERE ID = " + dpaccountid.ToString());
                        }
                    }
                    catch(Exception exp2)
                    {
                        System.Diagnostics.Debug.WriteLine(exp2.Message);
                    }
                }
            }

            if (errorOccur)
            {
                return BadRequest(errorString);
            }

            return Created(createContext.DocumentInfo);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostLoanDocument(int HomeID, [FromBody]FinanceLoanDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest("Model State is Invalid");
            }
            if (createContext == null || createContext.DocumentInfo == null || createContext.AccountInfo == null
                || createContext.AccountInfo.ExtraLoan == null
                || createContext.AccountInfo.ExtraLoan.LoanTmpDocs.Count <= 0)
            {
                return BadRequest("Invalid inputted data");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!(createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_BorrowFrom
                || createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_LendTo))
            {
                return BadRequest("Invalid document type");
            }
            if (createContext.DocumentInfo.Items.Count != 1)
            {
                return BadRequest("Only one item doc is supported by far");
            }
            foreach (var tdoc in createContext.AccountInfo.ExtraLoan.LoanTmpDocs)
            {
                if (!tdoc.ControlCenterID.HasValue && !tdoc.OrderID.HasValue)
                {
                    return BadRequest("Either control center or order shall be specified in Loan Template doc");
                }
                if (tdoc.TransactionAmount <= 0)
                {
                    return BadRequest("Amount is zero!");
                }
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var dpaccountid = 0;
            try
            {
                var docEntity = _context.FinanceDocument.Add(createContext.DocumentInfo);
                await _context.SaveChangesAsync();
                origdocid = docEntity.Entity.ID;

                // 2, Create the account
                createContext.AccountInfo.ExtraLoan.RefDocID = origdocid;
                var acntEntity = _context.FinanceAccount.Add(createContext.AccountInfo);
                await _context.SaveChangesAsync();
                dpaccountid = acntEntity.Entity.ID;

                // 3, Update the document
                var itemid = docEntity.Entity.Items.ElementAt(0).ItemID;
                // _context.Attach(docEntity);

                var ndi = new FinanceDocumentItem();
                ndi.ItemID = ++itemid;
                ndi.AccountID = dpaccountid;
                ndi.ControlCenterID = docEntity.Entity.Items.ElementAt(0).ControlCenterID;
                ndi.OrderID = docEntity.Entity.Items.ElementAt(0).OrderID;
                ndi.Desp = docEntity.Entity.Items.ElementAt(0).Desp;
                ndi.TranAmount = docEntity.Entity.Items.ElementAt(0).TranAmount;
                ndi.UseCurr2 = docEntity.Entity.Items.ElementAt(0).UseCurr2;
                if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_BorrowFrom)
                    ndi.TranType = FinanceTransactionType.TranType_BorrowFrom;
                else if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_LendTo)
                    ndi.TranType = FinanceTransactionType.TranType_LendTo;
                docEntity.Entity.Items.Add(ndi);

                docEntity.State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (Exception exp)
            {
                errorOccur = true;
                errorString = exp.Message;
            }
            finally
            {
                // Remove new created object
                if (errorOccur)
                {
                    try
                    {
                        if (origdocid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_document WHERE ID = " + origdocid.ToString());
                        }
                        if (dpaccountid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_account WHERE ID = " + dpaccountid.ToString());
                        }
                    }
                    catch (Exception exp2)
                    {
                        System.Diagnostics.Debug.WriteLine(exp2.Message);
                    }
                }
            }

            if (errorOccur)
            {
                return BadRequest(errorString);
            }

            return Created(createContext.DocumentInfo);
        }
    
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostAssetBuyDocument(int HomeID, [FromBody]FinanceAssetBuyDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest("Model State is Invalid");
            }
            if (createContext == null || createContext.ExtraAsset == null)
                return BadRequest("No data is inputted");
            if (HomeID <= 0)
                return BadRequest("Not HID inputted");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Do basic checks
            if (String.IsNullOrEmpty(createContext.TranCurr) || String.IsNullOrEmpty(createContext.ExtraAsset.Name))
                return BadRequest("Invalid input data");
            if (createContext.IsLegacy.HasValue && createContext.IsLegacy.Value)
            {
                if (createContext.Items.Count > 0)
                    return BadRequest("Invalid input data");
            }
            else
            {
                if (createContext.Items.Count <= 0)
                    return BadRequest("Invalid input data");
            }

            foreach (var di in createContext.Items)
            {
                if (di.TranAmount == 0 || di.AccountID <= 0 || di.TranType <= 0 || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");
            }

            // Construct the Account
            var vmAccount = new FinanceAccount();
            vmAccount.HomeID = createContext.HID;
            vmAccount.Name = createContext.ExtraAsset.Name;
            vmAccount.Status = FinanceAccountStatus.Normal;
            vmAccount.CategoryID = FinanceAccountCategoriesController.AccountCategory_Asset;
            vmAccount.ExtraAsset = new FinanceAccountExtraAS();
            vmAccount.Owner = createContext.AccountOwner;
            vmAccount.Comment = createContext.ExtraAsset.Comment;
            vmAccount.ExtraAsset.Name = createContext.ExtraAsset.Name;
            vmAccount.ExtraAsset.Comment = createContext.ExtraAsset.Comment;
            vmAccount.ExtraAsset.CategoryID = createContext.ExtraAsset.CategoryID;
            vmAccount.ExtraAsset.AccountHeader = vmAccount;

            // Construct the Doc.
            var vmFIDoc = new FinanceDocument();
            vmFIDoc.DocType = FinanceDocumentType.DocType_AssetBuyIn;
            vmFIDoc.Desp = createContext.Desp;
            vmFIDoc.TranDate = createContext.TranDate;
            vmFIDoc.HomeID = HomeID;
            vmFIDoc.TranCurr = createContext.TranCurr;

            var maxItemID = 0;
            if (createContext.IsLegacy.HasValue && createContext.IsLegacy.Value)
            {
                // Legacy account...
            }
            else
            {
                Decimal totalAmt = 0;
                foreach (var di in createContext.Items)
                {
                    if (di.ItemID <= 0 || di.TranAmount == 0 || di.AccountID <= 0
                        || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                        return BadRequest("Invalid input data in items!");

                    // Todo: new check the tran. type is an expense!

                    totalAmt += di.TranAmount;
                    vmFIDoc.Items.Add(di);

                    if (maxItemID < di.ItemID)
                        maxItemID = di.ItemID;
                }

                if (totalAmt != createContext.TranAmount)
                    return BadRequest("Amount is not even");
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var assetaccountid = 0;
            try
            {
                // 1. Create the document
                var docEntity = _context.FinanceDocument.Add(vmFIDoc);
                await _context.SaveChangesAsync();
                origdocid = docEntity.Entity.ID;

                // 2, Create the account
                vmAccount.ExtraAsset.RefenceBuyDocumentID = origdocid;
                var acntEntity = _context.FinanceAccount.Add(vmAccount);
                await _context.SaveChangesAsync();
                assetaccountid = acntEntity.Entity.ID;

                // 3. Update the document by adding one more item
                var nitem = new FinanceDocumentItem();
                nitem.ItemID = ++maxItemID;
                nitem.AccountID = assetaccountid;
                nitem.TranAmount = createContext.TranAmount;
                nitem.Desp = vmFIDoc.Desp;
                nitem.TranType = FinanceTransactionType.TranType_OpeningAsset;
                if (createContext.ControlCenterID.HasValue)
                    nitem.ControlCenterID = createContext.ControlCenterID.Value;
                if (createContext.OrderID.HasValue)
                    nitem.OrderID = createContext.OrderID.Value;
                nitem.DocumentHeader = vmFIDoc;
                docEntity.Entity.Items.Add(nitem);

                docEntity.State = EntityState.Modified;

                await _context.SaveChangesAsync();

                vmFIDoc = docEntity.Entity;
            }
            catch (Exception exp)
            {
                errorOccur = true;
                errorString = exp.Message;
            }
            finally
            {
                // Remove new created object
                if (errorOccur)
                {
                    try
                    {
                        if (origdocid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_document WHERE ID = " + origdocid.ToString());
                        }
                        if (assetaccountid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_account WHERE ID = " + assetaccountid.ToString());
                        }
                    }
                    catch (Exception exp2)
                    {
                        System.Diagnostics.Debug.WriteLine(exp2.Message);
                    }
                }
            }

            if (errorOccur)
            {
                return BadRequest(errorString);
            }

            return Created(vmFIDoc);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostAssetSellDocument(int HomeID, [FromBody]FinanceAssetSellDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest("Model State is Invalid");
            }
            if (HomeID <= 0)
                return BadRequest("Not HID inputted");
            if (createContext.AssetAccountID <= 0)
                return BadRequest("Asset Account is invalid");
            if (createContext.TranAmount <= 0)
                return BadRequest("Amount is less than zero");
            if (createContext.Items.Count <= 0)
                return BadRequest("No items inputted");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Construct the Doc.
            var vmFIDoc = new FinanceDocument();
            vmFIDoc.DocType = FinanceDocumentType.DocType_AssetSoldOut;
            vmFIDoc.Desp = createContext.Desp;
            vmFIDoc.TranDate = createContext.TranDate;
            vmFIDoc.HomeID = HomeID;
            vmFIDoc.TranCurr = createContext.TranCurr;
            Decimal totalAmt = 0;
            var maxItemID = 0;
            foreach (var di in createContext.Items)
            {
                if (di.ItemID <= 0 || di.TranAmount == 0 || di.AccountID <= 0
                    || di.TranType != FinanceTransactionType.TranType_AssetSoldoutIncome
                    || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");

                totalAmt += di.TranAmount;
                vmFIDoc.Items.Add(di);

                if (maxItemID < di.ItemID)
                    maxItemID = di.ItemID;
            }
            if (Decimal.Compare(totalAmt, createContext.TranAmount) != 0)
                return BadRequest("Amount is not even");

            // Check 1: check account is a valid asset?
            var query = (from account in this._context.FinanceAccount
                        join assetaccount in this._context.FinanceAccountExtraAS on account.ID equals assetaccount.AccountID
                        where account.ID == createContext.AssetAccountID
                        select new { Status = account.Status, RefSellDoc = assetaccount.RefenceSoldDocumentID }).FirstOrDefault();
            if (query.Status != FinanceAccountStatus.Normal)
            {
                return BadRequest("Account status is not normal");
            }

            // Check 2: check the inputted date is valid > must be the later than all existing transactions;
            var query2 = (from docitem in this._context.FinanceDocumentItem
                         join docheader in this._context.FinanceDocument on docitem.DocID equals docheader.ID
                         where docitem.AccountID == createContext.AssetAccountID
                         orderby docheader.TranDate descending
                         select new { TranDate = docheader.TranDate }).FirstOrDefault();
            if (createContext.TranDate < query2.TranDate)
                return BadRequest("Tran. data is invalid");

            // Check 3. Fetch current balance
            var query3 = (from acntbal in this._context.FinanceReporAccountGroupView
                         where acntbal.AccountID == createContext.AssetAccountID
                         select acntbal.Balance).First();
            if (query3 <= 0)
                return BadRequest("Balance is less than zero");

            var nitem = new FinanceDocumentItem();
            nitem.ItemID = ++maxItemID;
            nitem.AccountID = createContext.AssetAccountID;
            nitem.TranAmount = createContext.TranAmount;
            nitem.Desp = vmFIDoc.Desp;
            nitem.TranType = FinanceTransactionType.TranType_AssetSoldout;
            if (createContext.ControlCenterID.HasValue)
                nitem.ControlCenterID = createContext.ControlCenterID.Value;
            if (createContext.OrderID.HasValue)
                nitem.OrderID = createContext.OrderID.Value;
            nitem.DocumentHeader = vmFIDoc;
            vmFIDoc.Items.Add(nitem);


            var ncmprst = Decimal.Compare(query3, createContext.TranAmount);
            if (ncmprst > 0)
            {
                var nitem2 = new FinanceDocumentItem();
                nitem2.ItemID = ++maxItemID;
                nitem2.AccountID = createContext.AssetAccountID;
                nitem2.TranAmount = Decimal.Subtract(query3, createContext.TranAmount);
                nitem2.Desp = vmFIDoc.Desp;
                nitem2.TranType = FinanceTransactionType.TranType_AssetValueDecrease;
                if (createContext.ControlCenterID.HasValue)
                    nitem2.ControlCenterID = createContext.ControlCenterID.Value;
                if (createContext.OrderID.HasValue)
                    nitem2.OrderID = createContext.OrderID.Value;
                nitem.DocumentHeader = vmFIDoc;
                vmFIDoc.Items.Add(nitem2);
            }
            else if (ncmprst < 0)
            {
                var nitem2 = new FinanceDocumentItem();
                nitem2.ItemID = ++maxItemID;
                nitem2.AccountID = createContext.AssetAccountID;
                nitem2.TranAmount = Decimal.Subtract(createContext.TranAmount, query3);
                nitem2.Desp = vmFIDoc.Desp;
                nitem2.TranType = FinanceTransactionType.TranType_AssetValueIncrease;
                if (createContext.ControlCenterID.HasValue)
                    nitem2.ControlCenterID = createContext.ControlCenterID.Value;
                if (createContext.OrderID.HasValue)
                    nitem2.OrderID = createContext.OrderID.Value;
                nitem.DocumentHeader = vmFIDoc;
                vmFIDoc.Items.Add(nitem2);
            }
            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            try
            {
                // 1. Create the document
                var docEntity = _context.FinanceDocument.Add(vmFIDoc);
                await _context.SaveChangesAsync();
                origdocid = docEntity.Entity.ID;

                vmFIDoc = docEntity.Entity;
            }
            catch (Exception exp)
            {
                errorOccur = true;
                errorString = exp.Message;
            }
            finally
            {
                // Remove new created object
                if (errorOccur)
                {
                    try
                    {
                        if (origdocid > 0)
                        {
                            _context.Database.ExecuteSqlRaw("DELETE FROM t_fin_document WHERE ID = " + origdocid.ToString());
                        }
                    }
                    catch (Exception exp2)
                    {
                        System.Diagnostics.Debug.WriteLine(exp2.Message);
                    }
                }
            }

            if (errorOccur)
            {
                return BadRequest(errorString);
            }

            return Created(vmFIDoc);
        }
    }
}
