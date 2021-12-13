using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Deltas;

namespace hihapi.Controllers
{
    [Authorize]
    public class FinanceDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceDocuments
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
            //var query = from hmem in _context.HomeMembers
            //            where hmem.User == usrName
            //            select new { HomeID = hmem.HomeID } into hids
            //            join docs in _context.FinanceDocument on hids.HomeID equals docs.HomeID
            //            select docs;
            var query = from hmem in _context.HomeMembers
                        where hmem.User == usrName
                        select new { HomeID = hmem.HomeID, User = hmem.User, IsChild = hmem.IsChild } into hmems
                        join docs in _context.FinanceDocument
                          on hmems.HomeID equals docs.HomeID
                        where (hmems.IsChild == true && hmems.User == docs.Createdby)
                            || hmems.IsChild == null
                            || hmems.IsChild == false
                        select docs;

            return option.ApplyTo(query);
        }

        // The Route will never reach following codes...
        // 
        //[EnableQuery]
        //[Authorize]
        //public SingleResult<FinanceDocument> Get([FromODataUri]Int32 docid)
        //{
        //    String usrName = String.Empty;
        //    try
        //    {
        //        usrName = HIHAPIUtility.GetUserID(this);
        //        if (String.IsNullOrEmpty(usrName))
        //            throw new UnauthorizedAccessException();
        //    }
        //    catch
        //    {
        //        throw new UnauthorizedAccessException();
        //    }

        //    var hidquery = from hmem in _context.HomeMembers
        //                   where hmem.User == usrName
        //                   select new { HomeID = hmem.HomeID };
        //    var docquery = from doc in _context.FinanceDocument
        //                  where doc.ID == docid
        //                  select doc;
        //    var rstquery = from doc in docquery
        //                   join hid in hidquery
        //                   on doc.HomeID equals hid.HomeID
        //                   select doc;

        //    return SingleResult.Create(rstquery);
        //}

        public async Task<IActionResult> Post([FromBody]FinanceDocument document)
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

        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody]FinanceDocument update)
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

            if (!update.IsValid(this._context))
                return BadRequest();

            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(update).State = EntityState.Modified;

            // Items
            var itemsInDB = _context.FinanceDocumentItem.Where(p => p.DocID == update.ID).ToList();
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
                    _context.Entry(itemindb).State = EntityState.Modified;
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

            return Updated(update);
        }

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

        public async Task<IActionResult> Patch([FromODataUri] int id, [FromBody] Delta<FinanceDocument> doc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            var entity = await _context.FinanceDocument.FindAsync(id);
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

            // Patch it
            doc.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FinanceDocument.Any(p => p.ID == id))
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
        public async Task<IActionResult> PostDPDocument([FromBody]FinanceADPDocumentCreateContext createContext)
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
            foreach(var tmpdoc in createContext.AccountInfo.ExtraDP.DPTmpDocs)
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

            return Created(createContext.DocumentInfo);
        }

        [HttpPost]
        public async Task<IActionResult> PostLoanDocument([FromBody]FinanceLoanDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (createContext == null || createContext.DocumentInfo == null || createContext.AccountInfo == null
                || createContext.AccountInfo.HomeID <= 0
                || createContext.AccountInfo.Status != (Byte)FinanceAccountStatus.Normal
                || createContext.DocumentInfo.HomeID <= 0
                || createContext.DocumentInfo.HomeID != createContext.AccountInfo.HomeID
                || createContext.AccountInfo.ExtraLoan == null
                || createContext.AccountInfo.ExtraLoan.LoanTmpDocs.Count <= 0)
            {
                throw new BadRequestException("Invalid inputted data");
            }
            // Check tmp doc ID
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

            return Created(createContext.DocumentInfo);
        }
    
        [HttpPost]
        public async Task<IActionResult> PostAssetBuyDocument([FromBody]FinanceAssetBuyDocumentCreateContext createContext)
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
            vmAccount.Status = (Byte)FinanceAccountStatus.Normal;
            vmAccount.CategoryID = FinanceAccountCategory.AccountCategory_Asset;
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

            return Created(vmFIDoc);
        }

        [HttpPost]
        public async Task<IActionResult> PostAssetValueChangeDocument([FromBody]FinanceAssetRevaluationDocumentCreateContext createContext)
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

            return Created(vmFIDoc);
        }

        [HttpPost]
        public async Task<IActionResult> PostAssetSellDocument([FromBody]FinanceAssetSellDocumentCreateContext createContext)
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

            return Created(vmFIDoc);
        }
    }
}
