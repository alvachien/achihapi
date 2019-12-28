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
    }
}
