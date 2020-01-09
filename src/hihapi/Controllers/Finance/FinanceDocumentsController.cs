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
            if (String.IsNullOrEmpty(createContext.TranCurr) || String.IsNullOrEmpty(createContext.ExtraAsset.Name)
                || (createContext.IsLegacy.HasValue && createContext.IsLegacy.Value && createContext.Items.Count > 0)
                || ((!createContext.IsLegacy.HasValue || (createContext.IsLegacy.HasValue && !createContext.IsLegacy.Value)) && createContext.Items.Count <= 0)
                )
                return BadRequest("Invalid input data");

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
            vmAccount.Comment = createContext.accountAsset.Name;
            vmAccount.ExtraAsset.Name = createContext.accountAsset.Name;
            vmAccount.ExtraAsset.Comment = createContext.accountAsset.Comment;
            vmAccount.ExtraAsset.CategoryID = createContext.accountAsset.CategoryID;

            // Construct the Doc.
            var vmFIDoc = new FinanceDocumentUIViewModel();
            vmFIDoc.DocType = FinanceDocTypeViewModel.DocType_AssetBuyIn;
            vmFIDoc.Desp = vm.Desp;
            vmFIDoc.TranDate = vm.TranDate;
            vmFIDoc.HID = vm.HID;
            vmFIDoc.TranCurr = vm.TranCurr;

            var maxItemID = 0;
            if (vm.IsLegacy.HasValue && vm.IsLegacy.Value)
            {
                // Legacy account...
            }
            else
            {
                Decimal totalAmt = 0;
                foreach (var di in vm.Items)
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

                if (totalAmt != vm.TranAmount)
                    return BadRequest("Amount is not even");
            }

            var nitem = new FinanceDocumentItemUIViewModel();
            nitem.ItemID = ++maxItemID;
            nitem.AccountID = -1;
            nitem.TranAmount = vm.TranAmount;
            nitem.Desp = vmFIDoc.Desp;
            nitem.TranType = FinanceTranTypeViewModel.TranType_OpeningAsset;
            if (vm.ControlCenterID.HasValue)
                nitem.ControlCenterID = vm.ControlCenterID.Value;
            if (vm.OrderID.HasValue)
                nitem.OrderID = vm.OrderID.Value;
            vmFIDoc.Items.Add(nitem);

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewDocID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                // Basic check again - document level
                FinanceDocumentController.FinanceDocumentBasicCheck(vmFIDoc);

                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    // Perfrom the doc. validation
                    await FinanceDocumentController.FinanceDocumentBasicValidationAsync(vmFIDoc, conn, -1);

                    // 0) Start the trasnaction for modifications
                    tran = conn.BeginTransaction();

                    // 1) craete the doc header => nNewDocID
                    queryString = HIHDBUtility.GetFinDocHeaderInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, vmFIDoc, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    vmFIDoc.ID = nNewDocID;
                    cmd.Dispose();
                    cmd = null;

                    // 2), create the new account => nNewAccountID
                    queryString = HIHDBUtility.GetFinanceAccountHeaderInsertString();

                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    HIHDBUtility.BindFinAccountInsertParameter(cmd, vmAccount, usrName);
                    SqlParameter idparam2 = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam2.Direction = ParameterDirection.Output;

                    nRst = await cmd.ExecuteNonQueryAsync();
                    vmAccount.ID = (Int32)idparam2.Value;
                    cmd.Dispose();
                    cmd = null;

                    // 3) create the Asset part of account
                    vmAccount.ExtraInfo_AS.AccountID = vmAccount.ID;
                    vmAccount.ExtraInfo_AS.RefDocForBuy = nNewDocID;
                    queryString = HIHDBUtility.GetFinanceAccountAssetInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    HIHDBUtility.BindFinAccountAssetInsertParameter(cmd, vmAccount.ExtraInfo_AS);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // 4) create the doc items
                    foreach (FinanceDocumentItemUIViewModel ivm in vmFIDoc.Items)
                    {
                        if (ivm.AccountID == -1)
                            ivm.AccountID = vmAccount.ID;

                        queryString = HIHDBUtility.GetFinDocItemInsertString();
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinDocItemInsertParameter(cmd, ivm, nNewDocID);

                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;

                        // Tags
                        if (ivm.TagTerms.Count > 0)
                        {
                            // Create tags
                            foreach (var term in ivm.TagTerms)
                            {
                                queryString = HIHDBUtility.GetTagInsertString();

                                cmd = new SqlCommand(queryString, conn, tran);

                                HIHDBUtility.BindTagInsertParameter(cmd, vm.HID, HIHTagTypeEnum.FinanceDocumentItem, nNewDocID, term, ivm.ItemID);

                                await cmd.ExecuteNonQueryAsync();

                                cmd.Dispose();
                                cmd = null;
                            }
                        }
                    }

                    // 5) Do the commit
                    tran.Commit();

                    // Update the buffer
                    // Account List
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinAccountList, vm.HID, null);
                        this._cache.Remove(cacheKey);
                    }
                    catch (Exception)
                    {
                        // Do nothing here.
                    }
                    // B.S.
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinReportBS, vm.HID);
                        this._cache.Remove(cacheKey);
                    }
                    catch (Exception)
                    {
                        // Do nothing here.
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif

                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;

                if (tran != null)
                    tran.Rollback();
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest(strErrMsg);
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            return Created(createContext.DocumentInfo);
        }
    }
}
