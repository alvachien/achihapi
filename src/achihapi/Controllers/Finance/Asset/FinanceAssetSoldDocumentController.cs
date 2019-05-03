using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using achihapi.Utilities;
using Microsoft.AspNetCore.JsonPatch;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceAssetSoldDocument")]
    public class FinanceAssetSoldDocumentController : Controller
    {
        private IMemoryCache _cache;
        public FinanceAssetSoldDocumentController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/FinanceAssetSoldDocument
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return BadRequest();
        }

        //// GET: api/FinanceAssetSoldDocument/5
        //[HttpGet("{id}")]
        //[Authorize]
        //public async Task<IActionResult> Get([FromRoute]int id, [FromQuery]Int32 hid = 0)
        //{
        //    if (hid <= 0)
        //        return BadRequest("Not HID inputted");

        //    String usrName = String.Empty;
        //    if (Startup.UnitTestMode)
        //        usrName = UnitTestUtility.UnitTestUser;
        //    else
        //    {
        //        var usrObj = HIHAPIUtility.GetUserClaim(this);
        //        usrName = usrObj.Value;
        //    }
        //    if (String.IsNullOrEmpty(usrName))
        //        return BadRequest("User cannot recognize");

        //    FinanceAssetDocumentUIViewModel vm = new FinanceAssetDocumentUIViewModel();
        //    SqlConnection conn = null;
        //    SqlCommand cmd = null;
        //    SqlDataReader reader = null;
        //    String queryString = "";
        //    String strErrMsg = "";
        //    HttpStatusCode errorCode = HttpStatusCode.OK;

        //    try
        //    {
        //        queryString = HIHDBUtility.getFinanceDocAssetQueryString(id, false, hid);

        //        using(conn = new SqlConnection(Startup.DBConnectionString))
        //        {
        //            await conn.OpenAsync();

        //            // Check Home assignment with current user
        //            try
        //            {
        //                HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
        //            }
        //            catch (Exception)
        //            {
        //                errorCode = HttpStatusCode.BadRequest;
        //                throw;
        //            }

        //            cmd = new SqlCommand(queryString, conn);
        //            reader = cmd.ExecuteReader();

        //            if (!reader.HasRows)
        //            {
        //                errorCode = HttpStatusCode.NotFound;
        //                throw new Exception();
        //            }

        //            // Header
        //            while (reader.Read())
        //            {
        //                HIHDBUtility.FinDocHeader_DB2VM(reader, vm);
        //            }
        //            await reader.NextResultAsync();

        //            // Items
        //            while (reader.Read())
        //            {
        //                FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
        //                HIHDBUtility.FinDocItem_DB2VM(reader, itemvm);

        //                vm.Items.Add(itemvm);
        //            }
        //            await reader.NextResultAsync();

        //            // Account
        //            while (reader.Read())
        //            {
        //                FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
        //                Int32 aidx = 0;
        //                aidx = HIHDBUtility.FinAccountHeader_DB2VM(reader, vmAccount, aidx);
        //                vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
        //                HIHDBUtility.FinAccountAsset_DB2VM(reader, vmAccount.ExtraInfo_AS, aidx);
        //                vm.AccountVM = vmAccount;
        //            }
        //            await reader.NextResultAsync();

        //            // Tags
        //            if (reader.HasRows)
        //            {
        //                while (reader.Read())
        //                {
        //                    Int32 itemID = reader.GetInt32(0);
        //                    String sterm = reader.GetString(1);

        //                    foreach (var vitem in vm.Items)
        //                    {
        //                        if (vitem.ItemID == itemID)
        //                        {
        //                            vitem.TagTerms.Add(sterm);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        System.Diagnostics.Debug.WriteLine(exp.Message);
        //        strErrMsg = exp.Message;
        //        if (errorCode == HttpStatusCode.OK)
        //            errorCode = HttpStatusCode.InternalServerError;
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Dispose();
        //            reader = null;
        //        }
        //        if (cmd != null)
        //        {
        //            cmd.Dispose();
        //            cmd = null;
        //        }
        //        if (conn != null)
        //        {
        //            conn.Dispose();
        //            conn = null;
        //        }
        //    }

        //    if (errorCode != HttpStatusCode.OK)
        //    {
        //        switch (errorCode)
        //        {
        //            case HttpStatusCode.Unauthorized:
        //                return Unauthorized();
        //            case HttpStatusCode.NotFound:
        //                return NotFound();
        //            case HttpStatusCode.BadRequest:
        //                return BadRequest(strErrMsg);
        //            default:
        //                return StatusCode(500, strErrMsg);
        //        }
        //    }

        //    var setting = new Newtonsoft.Json.JsonSerializerSettings
        //    {
        //        DateFormatString = HIHAPIConstants.DateFormatPattern,
        //        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        //    };

        //    return new JsonResult(vm, setting);
        //}

        // POST: api/FinanceAssetSoldDocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAssetSoldoutDocViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Perform checks
            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");
            if (vm.AssetAccountID <= 0)
                return BadRequest("Asset Account is invalid");
            if (vm.TranAmount <= 0)
                return BadRequest("Amount is less than zero");
            if (vm.Items.Count <= 0)
                return BadRequest("No items inputted");

            String usrName = String.Empty;
            if (Startup.UnitTestMode)
                usrName = UnitTestUtility.UnitTestUser;
            else
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            // Construct the Doc.
            var vmFIDoc = new FinanceDocumentUIViewModel();
            vmFIDoc.DocType = FinanceDocTypeViewModel.DocType_AssetSoldOut;
            vmFIDoc.Desp = vm.Desp;
            vmFIDoc.TranDate = vm.TranDate;
            vmFIDoc.HID = vm.HID;
            vmFIDoc.TranCurr = vm.TranCurr;

            Decimal totalAmt = 0;
            var maxItemID = 0;
            foreach (var di in vm.Items)
            {
                if (di.ItemID <= 0 || di.TranAmount == 0 || di.AccountID <= 0 
                    || di.TranType != FinanceTranTypeViewModel.TranType_AssetSoldoutIncome
                    || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");

                totalAmt += di.TranAmount;
                vmFIDoc.Items.Add(di);

                if (maxItemID < di.ItemID)
                    maxItemID = di.ItemID;
            }
            if (Decimal.Compare(totalAmt, vm.TranAmount) != 0)
                return BadRequest("Amount is not even");

            var nitem = new FinanceDocumentItemUIViewModel();
            nitem.ItemID = ++maxItemID;
            nitem.AccountID = vm.AssetAccountID;
            nitem.TranAmount = vm.TranAmount;
            nitem.Desp = vmFIDoc.Desp;
            nitem.TranType = FinanceTranTypeViewModel.TranType_AssetSoldout;
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
            Decimal dCurrBalance = 0;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                // Basic check again
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
                    await FinanceDocumentController.FinanceDocumentBasicValidationAsync(vmFIDoc, conn);

                    // Additional checks
                    SqlCommand cmdAddCheck = null;
                    SqlDataReader readerAddCheck = null;
                    try
                    {
                        // Check 1: check account is a valid asset?
                        String strsqls = @"SELECT 
                                      [t_fin_account].[STATUS]
                                      ,[t_fin_account_ext_as].[REFDOC_SOLD] AS [ASREFDOC_SOLD]
                                  FROM [dbo].[t_fin_account]
                                  INNER JOIN [dbo].[t_fin_account_ext_as]
                                       ON [t_fin_account].[ID] = [t_fin_account_ext_as].[ACCOUNTID]
                                  WHERE [t_fin_account].[ID] = " + vm.AssetAccountID.ToString()
                                   + " AND [t_fin_account].[HID] = " + vm.HID.ToString();
                        cmdAddCheck = new SqlCommand(strsqls, conn);
                        readerAddCheck = await cmdAddCheck.ExecuteReaderAsync();
                        
                        if (readerAddCheck.HasRows)
                        {
                            while(readerAddCheck.Read())
                            {
                                if (!readerAddCheck.IsDBNull(0))
                                {
                                    var acntStatus = (FinanceAccountStatus)readerAddCheck.GetByte(0);
                                    if (acntStatus != FinanceAccountStatus.Normal)
                                    {
                                        throw new Exception("Account status is not normal");
                                    }
                                }
                                else
                                {
                                    // Status is NULL stands for Active Status
                                    // throw new Exception("Account status is not normal");
                                }

                                if (!readerAddCheck.IsDBNull(1))
                                {
                                    throw new Exception("Account has soldout doc already");
                                }

                                break;
                            }
                        }
                        readerAddCheck.Close();
                        readerAddCheck = null;
                        cmdAddCheck.Dispose();
                        cmdAddCheck = null;

                        // Check 2: check the inputted date is valid > must be the later than all existing transactions;
                        strsqls = @"SELECT MAX(t_fin_document.TRANDATE)
                                    FROM [dbo].[t_fin_document_item]
	                                INNER JOIN [dbo].[t_fin_document] 
                                       ON [dbo].[t_fin_document_item].[DOCID] = [dbo].[t_fin_document].[ID]
                                    WHERE [dbo].[t_fin_document_item].[ACCOUNTID] = " + vm.AssetAccountID.ToString();
                        cmdAddCheck = new SqlCommand(strsqls, conn);
                        readerAddCheck = await cmdAddCheck.ExecuteReaderAsync();

                        if (readerAddCheck.HasRows)
                        {
                            while (readerAddCheck.Read())
                            {
                                var latestdate = readerAddCheck.GetDateTime(0);
                                if (vm.TranDate.Date < latestdate.Date)
                                {
                                    throw new Exception("Invalid date");
                                }

                                break;
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid account - no doc items");
                        }

                        readerAddCheck.Close();
                        readerAddCheck = null;
                        cmdAddCheck.Dispose();
                        cmdAddCheck = null;

                        // Check 3. Fetch current balance
                        strsqls = @"SELECT [balance]
                            FROM [dbo].[v_fin_report_bs] WHERE [accountid] = " + vm.AssetAccountID.ToString();
                        cmdAddCheck = new SqlCommand(strsqls, conn);
                        readerAddCheck = await cmdAddCheck.ExecuteReaderAsync();

                        if (readerAddCheck.HasRows)
                        {
                            while (readerAddCheck.Read())
                            {
                                dCurrBalance = readerAddCheck.GetDecimal(0);
                                if (dCurrBalance <= 0)
                                {
                                    throw new Exception("Balance is zero");
                                }

                                break;
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid account - no doc items");
                        }

                        readerAddCheck.Close();
                        readerAddCheck = null;
                        cmdAddCheck.Dispose();
                        cmdAddCheck = null;

                        var ncmprst = Decimal.Compare(dCurrBalance, vm.TranAmount);
                        if (ncmprst > 0)
                        {
                            var nitem2 = new FinanceDocumentItemUIViewModel();
                            nitem2.ItemID = ++maxItemID;
                            nitem2.AccountID = vm.AssetAccountID;
                            nitem2.TranAmount = Decimal.Subtract(dCurrBalance, vm.TranAmount);
                            nitem2.Desp = vmFIDoc.Desp;
                            nitem2.TranType = FinanceTranTypeViewModel.TranType_AssetValueDecrease;
                            if (vm.ControlCenterID.HasValue)
                                nitem2.ControlCenterID = vm.ControlCenterID.Value;
                            if (vm.OrderID.HasValue)
                                nitem2.OrderID = vm.OrderID.Value;
                            vmFIDoc.Items.Add(nitem2);
                        }
                        else if(ncmprst < 0)
                        {
                            var nitem2 = new FinanceDocumentItemUIViewModel();
                            nitem2.ItemID = ++maxItemID;
                            nitem2.AccountID = vm.AssetAccountID;
                            nitem2.TranAmount = Decimal.Subtract(vm.TranAmount, dCurrBalance);
                            nitem2.Desp = vmFIDoc.Desp;
                            nitem2.TranType = FinanceTranTypeViewModel.TranType_AssetValueIncrease;
                            if (vm.ControlCenterID.HasValue)
                                nitem2.ControlCenterID = vm.ControlCenterID.Value;
                            if (vm.OrderID.HasValue)
                                nitem2.OrderID = vm.OrderID.Value;
                            vmFIDoc.Items.Add(nitem2);
                        }
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }
                    finally
                    {
                        if (readerAddCheck != null)
                        {
                            readerAddCheck.Close();
                            readerAddCheck = null;
                        }
                        if (cmdAddCheck != null)
                        {
                            cmdAddCheck.Dispose();
                            cmdAddCheck = null;
                        }
                    }

                    // Begin the modification
                    tran = conn.BeginTransaction();

                    // First, craete the doc header => nNewDocID
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
                    cmd.Dispose();
                    cmd = null;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vmFIDoc.Items)
                    {
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

                    // Third, update the Account's status
                    queryString = HIHDBUtility.GetFinanceAccountStatusUpdateString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    // Close this account
                    HIHDBUtility.BindFinAccountStatusUpdateParameter(cmd, FinanceAccountStatus.Closed, vm.AssetAccountID, vm.HID, usrName);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Fourth, Update the Asset account part for sold doc
                    queryString = HIHDBUtility.GetFinanceAccountAssetUpdateSoldDocString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    HIHDBUtility.BindFinAccountAssetUpdateSoldDocParameter(cmd, nNewDocID, vm.AssetAccountID);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Do the commit
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
                    // Account itself
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinAccount, vm.HID, vm.AssetAccountID);
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

            // Return nothing
            return Ok(nNewDocID);
        }
        
        // PUT: api/FinanceAssetSoldDocument/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // PATCH: 
        //[HttpPatch("{id}")]
        //public async Task<IActionResult> Patch(int id, [FromQuery]int hid, [FromBody]JsonPatchDocument<FinanceAssetDocumentUIViewModel> patch)
        //{
        //    if (patch == null || id <= 0 || patch.Operations.Count <= 0)
        //        return BadRequest("No data is inputted");
        //    if (hid <= 0)
        //        return BadRequest("No home is inputted");

        //    // Update the database
        //    SqlConnection conn = null;
        //    SqlCommand cmd = null;
        //    SqlDataReader reader = null;
        //    String queryString = "";
        //    String strErrMsg = "";
        //    Boolean headTranDateUpdate = false;
        //    DateTime? headTranDate = null;
        //    Boolean headDespUpdate = false;
        //    String headDesp = null;
        //    HttpStatusCode errorCode = HttpStatusCode.OK;

        //    // Check the inputs.
        //    // Allowed to change:
        //    //  1. Header: Transaction date, Desp;
        //    //  2. Item: Transaction amount, Desp, Control Center ID, Order ID,
        //    foreach (var oper in patch.Operations)
        //    {
        //        switch (oper.path)
        //        {
        //            case "/tranDate":
        //                headTranDateUpdate = true;
        //                headTranDate = (DateTime)oper.value;
        //                break;

        //            case "/desp":
        //                headDespUpdate = true;
        //                headDesp = (String)oper.value;
        //                break;

        //            default:
        //                return BadRequest("Unsupport field found");
        //        }
        //    }

        //    // User name
        //    String usrName = String.Empty;
        //    if (Startup.UnitTestMode)
        //        usrName = UnitTestUtility.UnitTestUser;
        //    else
        //    {
        //        var usrObj = HIHAPIUtility.GetUserClaim(this);
        //        usrName = usrObj.Value;
        //    }
        //    if (String.IsNullOrEmpty(usrName))
        //        return BadRequest("User cannot recognize");

        //    try
        //    {
        //        queryString = HIHDBUtility.GetFinDocHeaderExistCheckString(id);

        //        using(conn = new SqlConnection(Startup.DBConnectionString))
        //        {
        //            await conn.OpenAsync();

        //            // Check Home assignment with current user
        //            try
        //            {
        //                HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
        //            }
        //            catch (Exception)
        //            {
        //                errorCode = HttpStatusCode.BadRequest;
        //                throw;
        //            }

        //            cmd = new SqlCommand(queryString, conn);
        //            reader = cmd.ExecuteReader();
        //            if (!reader.HasRows)
        //            {
        //                errorCode = HttpStatusCode.BadRequest;
        //                throw new Exception("Doc ID not exist");
        //            }
        //            else
        //            {
        //                reader.Close();
        //                cmd.Dispose();
        //                cmd = null;

        //                //var vm = new FinanceAssetDocumentUIViewModel();

        //                //// Header
        //                //while (reader.Read())
        //                //{
        //                //    HIHDBUtility.FinDocHeader_DB2VM(reader, vm);
        //                //}
        //                //reader.NextResult();

        //                //// Items
        //                //while (reader.Read())
        //                //{
        //                //    FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
        //                //    HIHDBUtility.FinDocItem_DB2VM(reader, itemvm);

        //                //    vm.Items.Add(itemvm);
        //                //}
        //                //reader.NextResult();

        //                //// Account
        //                //while (reader.Read())
        //                //{
        //                //    FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
        //                //    Int32 aidx = 0;
        //                //    aidx = HIHDBUtility.FinAccountHeader_DB2VM(reader, vmAccount, aidx);

        //                //    vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
        //                //    HIHDBUtility.FinAccountAsset_DB2VM(reader, vmAccount.ExtraInfo_AS, aidx);

        //                //    vm.AccountVM = vmAccount;
        //                //}
        //                //reader.NextResult();

        //                //reader.Dispose();
        //                //reader = null;

        //                //cmd.Dispose();
        //                //cmd = null;

        //                //// Now go ahead for the update
        //                ////var patched = vm.Copy();
        //                //patch.ApplyTo(vm, ModelState);
        //                //if (!ModelState.IsValid)
        //                //{
        //                //    return new BadRequestObjectResult(ModelState);
        //                //}

        //                // Optimized logic go ahead
        //                if (headTranDateUpdate || headDespUpdate)
        //                {
        //                    queryString = HIHDBUtility.GetFinDocHeader_PatchString(headTranDateUpdate, headDespUpdate);
        //                    cmd = new SqlCommand(queryString, conn);
        //                    if (headTranDateUpdate)
        //                        cmd.Parameters.AddWithValue("@TRANDATE", headTranDate);
        //                    if (headDespUpdate)
        //                        cmd.Parameters.AddWithValue("@DESP", headDesp);
        //                    cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
        //                    cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
        //                    cmd.Parameters.AddWithValue("@ID", id);

        //                    await cmd.ExecuteNonQueryAsync();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        System.Diagnostics.Debug.WriteLine(exp.Message);
        //        strErrMsg = exp.Message;
        //        if (errorCode == HttpStatusCode.OK)
        //            errorCode = HttpStatusCode.InternalServerError;
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Dispose();
        //            reader = null;
        //        }
        //        if (cmd != null)
        //        {
        //            cmd.Dispose();
        //            cmd = null;
        //        }
        //        if (conn != null)
        //        {
        //            conn.Dispose();
        //            conn = null;
        //        }
        //    }

        //    if (errorCode != HttpStatusCode.OK)
        //    {
        //        switch (errorCode)
        //        {
        //            case HttpStatusCode.Unauthorized:
        //                return Unauthorized();
        //            case HttpStatusCode.NotFound:
        //                return NotFound();
        //            case HttpStatusCode.BadRequest:
        //                return BadRequest(strErrMsg);
        //            default:
        //                return StatusCode(500, strErrMsg);
        //        }
        //    }

        //    return Ok();
        //}

        // DELETE: api/FinanceAssetSoldDocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            return BadRequest();
        }
    }
}
