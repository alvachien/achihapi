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
using Microsoft.Extensions.Logging;

namespace hihapi.Controllers
{
    [Authorize]
    public class FinanceDocumentsController : ODataController
    {
        private readonly hihDataContext _context;
        private readonly ILogger<FinanceDocumentsController> _logger;

        private Dictionary<String, Object> changableProperites = new Dictionary<string, object>();

        public FinanceDocumentsController(hihDataContext context, ILogger<FinanceDocumentsController> logger)
        {
            _context = context;
            _logger = logger;

            // Changable properites
            changableProperites.Add("Desp", null);
            changableProperites.Add("TranDate", null);
        }

        /// GET: /FinanceDocuments
        [EnableQuery]
        [HttpGet]
        // public IActionResult Get(ODataQueryOptions<FinanceDocument> option)
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
                      select new { HomeID = hmem.HomeID, User = hmem.User, IsChild = hmem.IsChild } into hmems
                      join docs in _context.FinanceDocument
                        on hmems.HomeID equals docs.HomeID
                      where (hmems.IsChild == true && hmems.User == docs.Createdby)
                          || hmems.IsChild == null
                          || hmems.IsChild == false
                      select docs);
        }

        [EnableQuery]
        [HttpGet]
        public FinanceDocument Get([FromODataUri] Int32 key)
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
                           where doc.ID == key
                           select doc;
            return (from doc in docquery
                    join hid in hidquery
                    on doc.HomeID equals hid.HomeID
                    select doc).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FinanceDocument document)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Check document type, DP, Asset, loan document is not allowed
            if (document.DocType == FinanceDocumentType.DocType_AdvancePayment
                || document.DocType == FinanceDocumentType.DocType_AdvanceReceive
                || document.DocType == FinanceDocumentType.DocType_AssetBuyIn
                || document.DocType == FinanceDocumentType.DocType_AssetSoldOut
                || document.DocType == FinanceDocumentType.DocType_AssetValChg
                || document.DocType == FinanceDocumentType.DocType_BorrowFrom
                || document.DocType == FinanceDocumentType.DocType_LendTo)
            {
                throw new BadRequestException("Document type is not allowed");
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

            document.CreatedAt = DateTime.Now;
            document.Createdby = usrName;
            _context.FinanceDocument.Add(document);
            await _context.SaveChangesAsync();

            return Created(document);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceDocument update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("Inputted ID mismatched");
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

            // Only allow change normal document: TranDate, Desp
            if (!update.IsChangeAllowed(_context))
                return BadRequest("Not support for changing");

            if (!update.IsValid(this._context))
                return BadRequest("Document verify failed");

            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(update).State = EntityState.Modified;

            // Items
            var itemsInDB = _context.FinanceDocumentItem.Where(p => p.DocID == update.ID).AsNoTracking().ToList();
            foreach (var ditem in update.Items)
            {
                var itemindb = itemsInDB.Find(p => p.DocID == update.ID && p.ItemID == ditem.ItemID);
                if (itemindb == null)
                {
                    _context.FinanceDocumentItem.Add(ditem);
                }
                else
                {
                    // Update
                    _context.Entry(ditem).State = EntityState.Modified;
                }
            }
            foreach (var ditem in itemsInDB)
            {
                var nitem = update.Items.FirstOrDefault(p => p.DocID == update.ID && p.ItemID == ditem.ItemID);
                if (nitem == null)
                {
                    _context.FinanceDocumentItem.Remove(ditem);
                }
            }

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

            return Ok(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinanceDocument.FindAsync(key);
            if (cc == null)
                return NotFound();

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
                throw new UnauthorizedAccessException();

            // Is allow to delete
            if (!cc.IsDeleteAllowed(this._context))
                return BadRequest();

            _context.FinanceDocument.Remove(cc);

            // Further steps
            var bFurtherStep = false;
            // 1. DP tmplate docs
            var dpDoc = _context.FinanceTmpDPDocument.SingleOrDefault(p => p.ReferenceDocumentID == cc.ID && p.HomeID == cc.HomeID);
            if (dpDoc != null)
            {
                dpDoc.ReferenceDocumentID = null;
                bFurtherStep = true;
            }

            // 2. Loan template doc
            if (bFurtherStep == false)
            {
                var loanDoc = _context.FinanceTmpLoanDocument.SingleOrDefault(p => p.ReferenceDocumentID == cc.ID && p.HomeID == cc.HomeID);
                if (loanDoc != null)
                {
                    loanDoc.ReferenceDocumentID = null;
                    bFurtherStep = true;
                }
            }

            // 3. DP created doc
            if (bFurtherStep == false)
            {
                var crtedacnt = (from acntdp in _context.FinanceAccountExtraDP
                                 join acnt in _context.FinanceAccount
                             on acntdp.AccountID equals acnt.ID
                                 where acnt.HomeID == cc.HomeID
                                 && acntdp.RefenceDocumentID == cc.ID
                                 select acnt).SingleOrDefault();
                if (crtedacnt != null)
                {
                    // Delete the account
                    _context.FinanceAccount.Remove(crtedacnt);
                    bFurtherStep = true;
                }
            }

            // 4. Loan created doc
            if (bFurtherStep == false)
            {
                var crtedacnt = (from acntloan in _context.FinanceAccountExtraLoan
                                 join acnt in _context.FinanceAccount
                             on acntloan.AccountID equals acnt.ID
                                 where acnt.HomeID == cc.HomeID
                                 && acntloan.RefDocID == cc.ID
                                 select acnt).SingleOrDefault();
                if (crtedacnt != null)
                {
                    // Delete the account
                    _context.FinanceAccount.Remove(crtedacnt);
                    bFurtherStep = true;
                }
            }

            // 5. Asset sold doc
            if (bFurtherStep == false)
            {
                var crtedacnt = (from acntset in _context.FinanceAccountExtraAS
                                 join acnt in _context.FinanceAccount
                             on acntset.AccountID equals acnt.ID
                                 where acnt.HomeID == cc.HomeID
                                 && acntset.RefenceSoldDocumentID == cc.ID
                                 select acntset).SingleOrDefault();
                if (crtedacnt != null)
                {
                    // Clear the doc id.
                    crtedacnt.RefenceSoldDocumentID = null;
                    //_context.FinanceAccount.Remove(crtedacnt);
                    bFurtherStep = true;
                }
            }

            // 6. Asset buy doc
            if (bFurtherStep == false)
            {
                var crtedacnt = (from acntset in _context.FinanceAccountExtraAS
                                 join acnt in _context.FinanceAccount
                             on acntset.AccountID equals acnt.ID
                                 where acnt.HomeID == cc.HomeID
                                 && acntset.RefenceBuyDocumentID == cc.ID
                                 select acnt).SingleOrDefault();
                if (crtedacnt != null)
                {
                    // Delete the account.
                    _context.FinanceAccount.Remove(crtedacnt);
                    bFurtherStep = true;
                }
            }

            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<FinanceDocument> doc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            var entity = await _context.FinanceDocument.FindAsync(key);
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
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Do the validation.
            if (!(entity.DocType == FinanceDocumentType.DocType_Normal
                || entity.DocType == FinanceDocumentType.DocType_Transfer))
                return BadRequest("Not supported document type");

            foreach (var prop in doc.GetChangedPropertyNames())
            {
                if (!changableProperites.ContainsKey(prop))
                    return BadRequest("Property change not allow");
            }

            // Patch it
            doc.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FinanceDocument.Any(p => p.ID == key))
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

        [HttpPost]
        public async Task<IActionResult> PostDPDocument([FromBody] FinanceADPDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (createContext == null
                || createContext.DocumentInfo == null
                || createContext.AccountInfo == null
                || createContext.AccountInfo.HomeID <= 0
                || createContext.AccountInfo.Status != (Byte)FinanceAccountStatus.Normal
                || createContext.DocumentInfo.HomeID <= 0
                || createContext.DocumentInfo.HomeID != createContext.AccountInfo.HomeID
                || createContext.AccountInfo.ExtraDP == null
                || createContext.AccountInfo.ExtraDP.DPTmpDocs.Count <= 0)
            {
                throw new BadRequestException("Invalid inputted data");
            }
            // Check tmp doc ID
            var tmpdocids = new Dictionary<int, int>();
            foreach (var tmpdoc in createContext.AccountInfo.ExtraDP.DPTmpDocs)
            {
                if (tmpdoc.DocumentID <= 0)
                {
                    throw new BadRequestException("Invalid document ID");
                }
                if (tmpdocids.ContainsKey(tmpdoc.DocumentID))
                {
                    throw new BadRequestException("Duplicated Document ID found: " + tmpdoc.DocumentID);
                }
                tmpdocids.Add(tmpdoc.DocumentID, tmpdoc.DocumentID);
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.AccountInfo.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Verify the inputted parameters
            if (!(createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvancePayment
                || createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvanceReceive))
            {
                throw new BadRequestException("Invalid document type");
            }
            if (createContext.DocumentInfo.Items.Count != 1)
            {
                throw new BadRequestException("It shall be only one line item in DP docs");
            }
            // Check on the data
            if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvancePayment)
            {
                if (createContext.DocumentInfo.Items.ElementAt(0).TranType != FinanceTransactionType.TranType_AdvancePaymentOut)
                    throw new BadRequestException("Invalid tran. type for advance payment");
            }
            else if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_AdvanceReceive)
            {
                if (createContext.DocumentInfo.Items.ElementAt(0).TranType != FinanceTransactionType.TranType_AdvanceReceiveIn)
                    throw new BadRequestException("Invalid tran. type for advance receive");
            }
            foreach (var tmpdocitem in createContext.AccountInfo.ExtraDP.DPTmpDocs)
            {
                if (!tmpdocitem.ControlCenterID.HasValue && !tmpdocitem.OrderID.HasValue)
                {
                    throw new BadRequestException("Tmp Doc Item miss control center or order");
                }
                if (tmpdocitem.TranAmount == 0)
                {
                    throw new BadRequestException("Tmp Doc Item miss amount");
                }
                tmpdocitem.CreatedAt = DateTime.Now;
                tmpdocitem.Createdby = usrName;
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var dpaccountid = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    createContext.DocumentInfo.Createdby = usrName;
                    createContext.DocumentInfo.CreatedAt = DateTime.Now;
                    var docEntity = _context.FinanceDocument.Add(createContext.DocumentInfo);
                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    // 2, Create the account
                    createContext.AccountInfo.ExtraDP.RefenceDocumentID = origdocid;
                    createContext.AccountInfo.CreatedAt = DateTime.Now;
                    createContext.AccountInfo.Createdby = usrName;
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
                throw new BadRequestException(errorString);
            }

            return Ok(createContext.DocumentInfo);
        }

        [HttpPost]
        public async Task<IActionResult> PostLoanDocument([FromBody] FinanceLoanDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            Boolean isLegacyLoan = createContext.IsLegacy.GetValueOrDefault() ? true : false;
            if (isLegacyLoan)
            {
                if (createContext == null || createContext.DocumentInfo == null || createContext.AccountInfo == null
                    || createContext.AccountInfo.HomeID <= 0
                    || createContext.AccountInfo.Status != FinanceAccountStatus.Normal
                    || createContext.DocumentInfo.HomeID <= 0
                    || createContext.DocumentInfo.HomeID != createContext.AccountInfo.HomeID
                    || createContext.AccountInfo.ExtraLoan == null
                    || createContext.AccountInfo.ExtraLoan.LoanTmpDocs.Count > 0
                    || createContext.LegacyAmount.GetValueOrDefault() <= 0
                    || (createContext.ControlCenterID.HasValue && createContext.OrderID.HasValue)
                    || (!createContext.ControlCenterID.HasValue && !createContext.OrderID.HasValue)
                    )
                {
                    throw new BadRequestException("Invalid inputted data");
                }
            }
            else
            {
                if (createContext == null || createContext.DocumentInfo == null || createContext.AccountInfo == null
                    || createContext.AccountInfo.HomeID <= 0
                    || createContext.AccountInfo.Status != FinanceAccountStatus.Normal
                    || createContext.DocumentInfo.HomeID <= 0
                    || createContext.DocumentInfo.HomeID != createContext.AccountInfo.HomeID
                    || createContext.AccountInfo.ExtraLoan == null
                    || createContext.AccountInfo.ExtraLoan.LoanTmpDocs.Count <= 0)
                {
                    throw new BadRequestException("Invalid inputted data");
                }

                var tmpdocids = new Dictionary<int, int>();
                foreach (var tmpdoc in createContext.AccountInfo.ExtraLoan.LoanTmpDocs)
                {
                    if (tmpdoc.DocumentID <= 0)
                    {
                        throw new BadRequestException("Invalid document ID");
                    }
                    if (tmpdocids.ContainsKey(tmpdoc.DocumentID))
                    {
                        throw new BadRequestException("Duplicated Document ID found: " + tmpdoc.DocumentID);
                    }
                    tmpdocids.Add(tmpdoc.DocumentID, tmpdoc.DocumentID);
                }
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.AccountInfo.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!(createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_BorrowFrom
                || createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_LendTo))
            {
                throw new BadRequestException("Invalid document type");
            }
            if (!isLegacyLoan)
            {
                if (createContext.DocumentInfo.Items.Count != 1)
                {
                    throw new BadRequestException("Only one item doc is supported by far");
                }
                foreach (var tdoc in createContext.AccountInfo.ExtraLoan.LoanTmpDocs)
                {
                    if (!tdoc.ControlCenterID.HasValue && !tdoc.OrderID.HasValue)
                    {
                        throw new BadRequestException("Either control center or order shall be specified in Loan Template doc");
                    }
                    if (tdoc.TransactionAmount <= 0)
                    {
                        throw new BadRequestException("Amount is zero!");
                    }

                    tdoc.Createdby = usrName;
                    tdoc.CreatedAt = DateTime.Now;
                }
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var dpaccountid = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create document
                    createContext.DocumentInfo.Createdby = usrName;
                    createContext.DocumentInfo.CreatedAt = DateTime.Now;
                    var docEntity = _context.FinanceDocument.Add(createContext.DocumentInfo);
                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    // 2, Create the account
                    createContext.AccountInfo.ExtraLoan.RefDocID = origdocid;
                    createContext.AccountInfo.CreatedAt = DateTime.Now;
                    createContext.AccountInfo.Createdby = usrName;
                    var acntEntity = _context.FinanceAccount.Add(createContext.AccountInfo);
                    await _context.SaveChangesAsync();
                    dpaccountid = acntEntity.Entity.ID;

                    // 3, Update the document
                    if (isLegacyLoan)
                    {
                        var ndi = new FinanceDocumentItem();
                        ndi.ItemID = 1;
                        ndi.AccountID = dpaccountid;
                        ndi.ControlCenterID = createContext.ControlCenterID;
                        ndi.OrderID = createContext.OrderID;
                        ndi.Desp = docEntity.Entity.Desp;
                        ndi.TranAmount = createContext.LegacyAmount.Value;
                        ndi.UseCurr2 = false;
                        if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_BorrowFrom)
                            ndi.TranType = FinanceTransactionType.TranType_OpeningLiability;
                        else if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_LendTo)
                            ndi.TranType = FinanceTransactionType.TranType_OpeningAsset;
                        docEntity.Entity.Items.Add(ndi);
                    }
                    else
                    {
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
                            ndi.TranType = FinanceTransactionType.TranType_OpeningLiability;
                        else if (createContext.DocumentInfo.DocType == FinanceDocumentType.DocType_LendTo)
                            ndi.TranType = FinanceTransactionType.TranType_OpeningAsset;
                        docEntity.Entity.Items.Add(ndi);
                    }

                    docEntity.State = EntityState.Modified;

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

            return Ok(createContext.DocumentInfo);
        }

        [HttpPost]
        public async Task<IActionResult> PostAssetBuyDocument([FromBody] FinanceAssetBuyDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (createContext == null || createContext.ExtraAsset == null)
                throw new BadRequestException("No data is inputted");
            if (createContext.HID <= 0)
                throw new BadRequestException("Not HID inputted");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.HID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Do basic checks
            if (String.IsNullOrEmpty(createContext.TranCurr) || String.IsNullOrEmpty(createContext.ExtraAsset.Name))
                throw new BadRequestException("Invalid input data");
            if (createContext.IsLegacy.HasValue && createContext.IsLegacy.Value)
            {
                if (createContext.Items.Count > 0)
                    throw new BadRequestException("Invalid input data");
            }
            else
            {
                if (createContext.Items.Count <= 0)
                    throw new BadRequestException("Invalid input data");
            }

            foreach (var di in createContext.Items)
            {
                if (di.TranAmount == 0 || di.AccountID <= 0 || di.TranType <= 0 || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    throw new BadRequestException("Invalid input data in items!");
            }

            // Construct the Account
            var vmAccount = new FinanceAccount();
            vmAccount.HomeID = createContext.HID;
            vmAccount.Name = createContext.ExtraAsset.Name;
            vmAccount.Status = FinanceAccountStatus.Normal;
            vmAccount.CategoryID = FinanceAccountCategory.AccountCategory_Asset;
            vmAccount.ExtraAsset = new FinanceAccountExtraAS();
            vmAccount.Owner = createContext.AccountOwner;
            vmAccount.Comment = createContext.ExtraAsset.Comment;
            vmAccount.ExtraAsset.Name = createContext.ExtraAsset.Name;
            vmAccount.ExtraAsset.Comment = createContext.ExtraAsset.Comment;
            vmAccount.ExtraAsset.CategoryID = createContext.ExtraAsset.CategoryID;
            vmAccount.ExtraAsset.AccountHeader = vmAccount;
            if (createContext.ExtraAsset.BoughtDate.HasValue)
                vmAccount.ExtraAsset.BoughtDate = createContext.ExtraAsset.BoughtDate;
            if (createContext.ExtraAsset.ExpiredDate.HasValue)
                vmAccount.ExtraAsset.ExpiredDate = createContext.ExtraAsset.ExpiredDate;
            if (createContext.ExtraAsset.ResidualValue.HasValue)
                vmAccount.ExtraAsset.ResidualValue = createContext.ExtraAsset.ResidualValue;

            // Construct the Doc.
            var vmFIDoc = new FinanceDocument();
            vmFIDoc.DocType = FinanceDocumentType.DocType_AssetBuyIn;
            vmFIDoc.Desp = createContext.Desp;
            vmFIDoc.TranDate = createContext.TranDate;
            vmFIDoc.HomeID = createContext.HID;
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
                        throw new BadRequestException("Invalid input data in items!");

                    // Todo: new check the tran. type is an expense!

                    totalAmt += di.TranAmount;
                    vmFIDoc.Items.Add(di);

                    if (maxItemID < di.ItemID)
                        maxItemID = di.ItemID;
                }

                if (totalAmt != createContext.TranAmount)
                    throw new BadRequestException("Amount is not even");
            }

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;
            var assetaccountid = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    vmFIDoc.Createdby = usrName;
                    vmFIDoc.CreatedAt = DateTime.Now;
                    var docEntity = _context.FinanceDocument.Add(vmFIDoc);
                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    // 2, Create the account
                    vmAccount.ExtraAsset.RefenceBuyDocumentID = origdocid;
                    vmAccount.CreatedAt = DateTime.Now;
                    vmAccount.Createdby = usrName;
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

            return Ok(vmFIDoc);
        }

        [HttpPost]
        public async Task<IActionResult> PostAssetValueChangeDocument([FromBody] FinanceAssetRevaluationDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (createContext == null)
                throw new BadRequestException("No data is inputted");
            if (createContext.HID <= 0)
                throw new BadRequestException("Not HID inputted");
            if (createContext.AssetAccountID <= 0)
                throw new BadRequestException("Asset Account is invalid");
            if (createContext.Items.Count != 1)
                throw new BadRequestException("Items count is not correct");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.HID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Construct the Doc.
            var vmFIDoc = new FinanceDocument();
            vmFIDoc.DocType = FinanceDocumentType.DocType_AssetValChg;
            vmFIDoc.Desp = createContext.Desp;
            vmFIDoc.TranDate = createContext.TranDate;
            vmFIDoc.HomeID = createContext.HID;
            vmFIDoc.TranCurr = createContext.TranCurr;

            Decimal totalAmount = 0;
            Int32? rlTranType = null;
            foreach (var di in createContext.Items)
            {
                if (di.ItemID <= 0 || di.TranAmount == 0
                    || di.AccountID != createContext.AssetAccountID
                    || (di.TranType != FinanceTransactionType.TranType_AssetValueIncrease
                        && di.TranType != FinanceTransactionType.TranType_AssetValueDecrease)
                    || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");

                if (rlTranType == null)
                {
                    rlTranType = di.TranType;
                }
                else
                {
                    if (rlTranType.Value != di.TranType)
                    {
                        return BadRequest("Cannot support different trantype");
                    }
                }

                totalAmount += di.TranAmount;

                vmFIDoc.Items.Add(di);
            }

            // Basic check - TBD
            //try
            //{
            //    FinanceDocumentController.FinanceDocumentBasicCheck(vmFIDoc);
            //}
            //catch(Exception exp)
            //{
            //}
            // Perfrom the doc. validation
            //await FinanceDocumentsController.FinanceDocumentBasicValidationAsync(vmFIDoc, conn);

            // Additional check 1.account is a valid asset?
            var query = (from account in this._context.FinanceAccount
                         join assetaccount in this._context.FinanceAccountExtraAS on account.ID equals assetaccount.AccountID
                         where account.ID == createContext.AssetAccountID
                         select new { Status = account.Status, RefSellDoc = assetaccount.RefenceSoldDocumentID }).FirstOrDefault();
            if (query.Status != (Byte)FinanceAccountStatus.Normal)
            {
                throw new BadRequestException("Account status is not normal");
            }
            if (query.RefSellDoc != null)
            {
                throw new BadRequestException("Asset has soldout doc already");
            }
            // Additional check 2. the inputted date is valid > must be the later than all existing transactions;
            var query2 = (from docitem in this._context.FinanceDocumentItem
                          join docheader in this._context.FinanceDocument on docitem.DocID equals docheader.ID
                          where docitem.AccountID == createContext.AssetAccountID
                          orderby docheader.TranDate descending
                          select new { TranDate = docheader.TranDate }).FirstOrDefault();
            if (createContext.TranDate < query2.TranDate)
                throw new BadRequestException("Tran. data is invalid");
            // Additional check 3. Fetch current balance
            var query3 = (from acntbal in this._context.FinanceReporAccountGroupView
                          where acntbal.AccountID == createContext.AssetAccountID
                          select acntbal.Balance).First();
            if (query3 <= 0)
                throw new BadRequestException("Balance is less than zero");
            // Additional check 4: the balance with the inputted value
            var nCmpRst = Decimal.Compare(query3, totalAmount);
            if (nCmpRst > 0)
            {
                if (rlTranType.Value != FinanceTransactionType.TranType_AssetValueDecrease)
                {
                    throw new BadRequestException("Tran type is wrong");
                }
            }
            else if (nCmpRst < 0)
            {
                if (rlTranType.Value != FinanceTransactionType.TranType_AssetValueIncrease)
                {
                    throw new BadRequestException("Tran type is wrong");
                }
            }
            else if (nCmpRst == 0)
            {
                throw new BadRequestException("Current balance equals to new value");
            }

            // Update the database
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    vmFIDoc.Createdby = usrName;
                    vmFIDoc.CreatedAt = DateTime.Now;
                    var docEntity = _context.FinanceDocument.Add(vmFIDoc);
                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    vmFIDoc = docEntity.Entity;

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

            return Ok(vmFIDoc);
        }

        [HttpPost]
        public async Task<IActionResult> PostAssetSellDocument([FromBody] FinanceAssetSellDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (createContext.HID <= 0)
                throw new BadRequestException("Not HID inputted");
            if (createContext.AssetAccountID <= 0)
                throw new BadRequestException("Asset Account is invalid");
            if (createContext.TranAmount <= 0)
                throw new BadRequestException("Amount is less than zero");
            if (createContext.Items.Count <= 0)
                throw new BadRequestException("No items inputted");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.HID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Construct the Doc.
            var vmFIDoc = new FinanceDocument();
            vmFIDoc.DocType = FinanceDocumentType.DocType_AssetSoldOut;
            vmFIDoc.Desp = createContext.Desp;
            vmFIDoc.TranDate = createContext.TranDate;
            vmFIDoc.HomeID = createContext.HID;
            vmFIDoc.TranCurr = createContext.TranCurr;
            Decimal totalAmt = 0;
            var maxItemID = 0;
            foreach (var di in createContext.Items)
            {
                if (di.ItemID <= 0 || di.TranAmount == 0 || di.AccountID <= 0
                    || di.TranType != FinanceTransactionType.TranType_AssetSoldoutIncome
                    || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    throw new BadRequestException("Invalid input data in items!");

                totalAmt += di.TranAmount;
                vmFIDoc.Items.Add(di);

                if (maxItemID < di.ItemID)
                    maxItemID = di.ItemID;
            }
            if (Decimal.Compare(totalAmt, createContext.TranAmount) != 0)
                throw new BadRequestException("Amount is not even");

            // Check 1: check account is a valid asset?
            var query = (from account in this._context.FinanceAccount
                         join assetaccount in this._context.FinanceAccountExtraAS on account.ID equals assetaccount.AccountID
                         where account.ID == createContext.AssetAccountID
                         select new { Status = account.Status, RefSellDoc = assetaccount.RefenceSoldDocumentID }).FirstOrDefault();
            if (query.Status != (Byte)FinanceAccountStatus.Normal)
            {
                throw new BadRequestException("Account status is not normal");
            }
            if (query.RefSellDoc != null)
            {
                throw new BadRequestException("Asset has soldout doc already");
            }

            // Check 2: check the inputted date is valid > must be the later than all existing transactions;
            var query2 = (from docitem in this._context.FinanceDocumentItem
                          join docheader in this._context.FinanceDocument on docitem.DocID equals docheader.ID
                          where docitem.AccountID == createContext.AssetAccountID
                          orderby docheader.TranDate descending
                          select new { TranDate = docheader.TranDate }).FirstOrDefault();
            if (createContext.TranDate < query2.TranDate)
                throw new BadRequestException("Tran. data is invalid");

            // Check 3. Fetch current balance
            var query3 = (from acntbal in this._context.FinanceReporAccountGroupView
                          where acntbal.AccountID == createContext.AssetAccountID
                          select acntbal.Balance).First();
            if (query3 <= 0)
                throw new BadRequestException("Balance is less than zero");

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

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    vmFIDoc.CreatedAt = DateTime.Now;
                    vmFIDoc.Createdby = usrName;
                    var docEntity = _context.FinanceDocument.Add(vmFIDoc);
                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    vmFIDoc = docEntity.Entity;

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

            return Ok(vmFIDoc);
        }

        [HttpPost]
        public async Task<IActionResult> PostAssetDepreciationDocument([FromBody] FinanceAssetDepreciationContext depContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (depContext.HID <= 0)
                throw new BadRequestException("Not HID inputted");
            if (depContext.AssetAccountID <= 0)
                throw new BadRequestException("Asset Account is invalid");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == depContext.HID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // Construct the Doc.
            var vmFIDoc = new FinanceDocument();
            vmFIDoc.DocType = FinanceDocumentType.DocType_AssetDepreciation;
            vmFIDoc.HomeID = depContext.HID;
            vmFIDoc.TranCurr = depContext.TranCurr;
            vmFIDoc.Desp = depContext.Desp;
            vmFIDoc.TranDate = depContext.TranDate;
            var docItem = new FinanceDocumentItem();
            docItem.Desp = depContext.Desp;
            docItem.ItemID = 1;
            docItem.AccountID = depContext.AssetAccountID;
            docItem.TranType = FinanceTransactionType.TranType_AssetValueDecrease;
            docItem.ControlCenterID = depContext.ControlCenterID;
            docItem.OrderID = depContext.OrderID;
            docItem.TranAmount = depContext.TranAmount;
            vmFIDoc.Items.Add(docItem);

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    vmFIDoc.CreatedAt = DateTime.Now;
                    vmFIDoc.Createdby = usrName;
                    var docEntity = _context.FinanceDocument.Add(vmFIDoc);

                    await _context.SaveChangesAsync();
                    origdocid = docEntity.Entity.ID;

                    vmFIDoc = docEntity.Entity;

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

            return Ok(vmFIDoc);
        }

        [HttpPost]
        public IActionResult GetAssetDepreciationResult([FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
            // 0. Get inputted parameter
            Int32 hid = (Int32)parameters["HomeID"];
            Int32 year = (Int32)parameters["Year"];
            Int32 month = (Int32)parameters["Month"];

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            List<FinanceAssetDepreicationResult> rsts = new List<FinanceAssetDepreicationResult>();
            // Find out the date
            DateTime dtMonthFirstday = new DateTime(year, month, 1);
            DateTime dtMonthLastday = dtMonthFirstday.AddMonths(1).AddDays(-1);
            // Fetch all asset account
            var acnts = from account in _context.FinanceAccount
                          join accountext in _context.FinanceAccountExtraAS
                            on account.ID equals accountext.AccountID
                          where account.HomeID == hid && account.CategoryID == FinanceAccountCategory.AccountCategory_Asset
                            && account.Status == FinanceAccountStatus.Normal
                            && accountext.BoughtDate != null && accountext.BoughtDate < dtMonthFirstday
                            && accountext.ExpiredDate != null && accountext.ExpiredDate > dtMonthFirstday
                        select new {
                            account.ID,
                            accountext.ExpiredDate,
                            accountext.BoughtDate,
                            accountext.ResidualValue
                          };
            var lc = _context.HomeDefines.First(prop => prop.ID == hid).BaseCurrency;

            // Get the balance
            var dbresults = (
                from docitem in _context.FinanceDocumentItem
                join docheader in _context.FinanceDocument
                    on docitem.DocID equals docheader.ID
                join trantype in _context.FinTransactionType
                    on docitem.TranType equals trantype.ID
                join acnt in acnts
                    on docitem.AccountID equals acnt.ID
                select new
                {
                    AccountID = docitem.AccountID,
                    docheader.TranDate,
                    docheader.DocType,
                    IsExpense = trantype.Expense,
                    TranCurr = docheader.TranCurr,
                    TranCurr2 = docheader.TranCurr2,
                    UseCurr2 = docitem.UseCurr2,
                    TranAmount = docitem.TranAmount,
                    docheader.ExgRate,
                    docheader.ExgRate2,
                }).ToList();
                //into docitem2
                //group docitem2 by new { docitem2.AccountID, docitem2.IsExpense, docitem2.TranCurr, docitem2.TranCurr2, docitem2.UseCurr2, docitem2.ExgRate, docitem2.ExgRate2 } into docitem3
                //select new
                //{
                //    AccountID = docitem3.Key.AccountID,
                //    IsExpense = docitem3.Key.IsExpense,
                //    TranCurr = docitem3.Key.TranCurr,
                //    TranCurr2 = docitem3.Key.TranCurr2,
                //    UseCurr2 = docitem3.Key.UseCurr2,
                //    ExgRate = docitem3.Key.ExgRate,
                //    ExgRate2 = docitem3.Key.ExgRate2,
                //    TranAmount = docitem3.Sum(p => (Double)p.TranAmount)
                //}).ToList();

            foreach(var acnt in acnts)
            {
                Double doubleAmount = 0;
                if (dbresults.FindIndex(p => p.TranDate >= dtMonthFirstday && p.TranDate <= dtMonthLastday && p.DocType == FinanceDocumentType.DocType_AssetDepreciation) != -1)
                    continue;

                foreach (var dbrst in dbresults)
                {
                    if (dbrst.AccountID == acnt.ID)
                    {
                        double amountLC = (double)dbrst.TranAmount;
                        // Calculte the amount
                        if (dbrst.IsExpense)
                            amountLC = -1 * (double)dbrst.TranAmount;
                        if (dbrst.UseCurr2 != null)
                        {
                            if (dbrst.ExgRate2 != null && dbrst.ExgRate2.GetValueOrDefault() > 0)
                            {
                                amountLC *= (double)dbrst.ExgRate2.GetValueOrDefault();
                            }
                        }
                        else
                        {
                            if (dbrst.ExgRate != null && dbrst.ExgRate.GetValueOrDefault() > 0)
                            {
                                amountLC *= (double)dbrst.ExgRate.GetValueOrDefault();
                            }
                        }

                        doubleAmount += amountLC;
                    }
                }
                // Now we got the total amount
                if (acnt.ResidualValue != null)
                    doubleAmount -= (double)acnt.ResidualValue.Value;

                // Calculate how many months left
                var leftMonth = (acnt.ExpiredDate.Value - dtMonthLastday).TotalDays / 30;
                if (leftMonth > 0)
                {
                    FinanceAssetDepreicationResult result = new FinanceAssetDepreicationResult();
                    result.TranCurr = lc;
                    result.TranDate = dtMonthLastday;
                    result.TranAmount = Math.Round((Decimal)(doubleAmount / leftMonth), 2);
                    result.HID = hid;
                    result.AssetAccountID = acnt.ID;
                    rsts.Add(result);
                }
            }

            return Ok(rsts);
        }

        [HttpGet]
        public IActionResult IsChangable([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doc = _context.FinanceDocument.Find(key);
            if (doc == null)
            {
                return NotFound();
            }

            return Ok(doc.IsChangeAllowed(_context));
        }
    }
}
