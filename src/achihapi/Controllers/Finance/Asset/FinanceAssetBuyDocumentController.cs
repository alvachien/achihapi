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
    [Route("api/FinanceAssetBuyDocument")]
    public class FinanceAssetBuyDocumentController : Controller
    {
        private IMemoryCache _cache;
        public FinanceAssetBuyDocumentController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/FinanceAssetBuyDocument
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return BadRequest();
        }

        //// GET: api/FinanceAssetBuyDocument/5
        //[HttpGet("{id}")]
        //[Authorize]
        //public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
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
        //        queryString = HIHDBUtility.getFinanceDocAssetQueryString(id, true, hid);

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
        //            reader.NextResult();

        //            // Items
        //            while (reader.Read())
        //            {
        //                FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
        //                HIHDBUtility.FinDocItem_DB2VM(reader, itemvm);

        //                vm.Items.Add(itemvm);
        //            }
        //            reader.NextResult();

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
        //            reader.NextResult();

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
        //                return BadRequest();
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
        
        // POST: api/FinanceAssetBuyDocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAssetBuyinDocViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (vm == null || vm.accountAsset == null)
                return BadRequest("No data is inputted");

            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");

            // Do basic checks
            if (String.IsNullOrEmpty(vm.TranCurr) || String.IsNullOrEmpty(vm.accountAsset.Name)
                || (vm.IsLegacy.HasValue && vm.IsLegacy.Value && vm.Items.Count > 0) 
                || ((!vm.IsLegacy.HasValue || (vm.IsLegacy.HasValue && !vm.IsLegacy.Value)) && vm.Items.Count <= 0)
                )
                return BadRequest("Invalid input data");

            foreach(var di in vm.Items)
            {
                if (di.TranAmount == 0 || di.AccountID <= 0 || di.TranType <= 0 || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");
            }

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

            // Construct the Account
            var vmAccount = new FinanceAccountViewModel();
            vmAccount.HID = vm.HID;
            vmAccount.Name = vm.accountAsset.Name;
            vmAccount.Status = FinanceAccountStatus.Normal;
            vmAccount.CtgyID = FinanceAccountCtgyViewModel.AccountCategory_Asset;
            vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
            vmAccount.Owner = vm.AccountOwner;
            vmAccount.Comment = vm.accountAsset.Name;
            vmAccount.ExtraInfo_AS.Name = vm.accountAsset.Name;
            vmAccount.ExtraInfo_AS.Comment = vm.accountAsset.Comment;
            vmAccount.ExtraInfo_AS.CategoryID = vm.accountAsset.CategoryID;

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
                // Legacy account
                if (!vm.LegacyDate.HasValue || vm.LegacyDate.Value > vm.TranDate)
                    return BadRequest("Invalid input data in legacy date!");
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
                await FinanceDocumentController.FinanceDocumentBasicCheckAsync(vmFIDoc);

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

                if (tran != null)
                    tran.Rollback();
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
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
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            return Ok(nNewDocID);
        }

        // PUT: api/FinanceAssetBuyDocument/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put([FromRoute]int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // PATCH: 
        //[HttpPatch("{id}")]
        //public async Task<IActionResult> Patch([FromRoute]int id, [FromQuery]int hid, [FromBody]JsonPatchDocument<FinanceAssetDocumentUIViewModel> patch)
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
        //        switch(oper.path)
        //        {
        //            case "/tranDate":
        //                headTranDateUpdate = true;
        //                headTranDate = (DateTime)oper.value;
        //                break;

        //            case "/desp":
        //                headDespUpdate = true;
        //                headDesp = (String) oper.value;
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
        //                reader.Dispose();
        //                reader = null;
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
        //                return BadRequest();
        //            default:
        //                return StatusCode(500, strErrMsg);
        //        }
        //    }

        //    return Ok();
        //}

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }
    }
}
