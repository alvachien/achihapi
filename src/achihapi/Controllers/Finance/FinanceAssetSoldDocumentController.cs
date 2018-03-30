using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using achihapi.Utilities;
using Microsoft.AspNetCore.JsonPatch;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceAssetSoldDocument")]
    public class FinanceAssetSoldDocumentController : Controller
    {
        // GET: api/FinanceAssetSoldDocument
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            return BadRequest();
        }

        // GET: api/FinanceAssetSoldDocument/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

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

            FinanceAssetDocumentUIViewModel vm = new FinanceAssetDocumentUIViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = SqlUtility.getFinanceDocAssetQueryString(id, false, hid);

                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                // Header
                while (reader.Read())
                {
                    SqlUtility.FinDocHeader_DB2VM(reader, vm);
                }
                await reader.NextResultAsync();

                // Items
                while (reader.Read())
                {
                    FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                    SqlUtility.FinDocItem_DB2VM(reader, itemvm);

                    vm.Items.Add(itemvm);
                }
                await reader.NextResultAsync();

                // Account
                while (reader.Read())
                {
                    FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
                    Int32 aidx = 0;
                    aidx = SqlUtility.FinAccountHeader_DB2VM(reader, vmAccount, aidx);
                    vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
                    SqlUtility.FinAccountAsset_DB2VM(reader, vmAccount.ExtraInfo_AS, aidx);
                    vm.AccountVM = vmAccount;
                }
                await reader.NextResultAsync();

                // Tags
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Int32 itemID = reader.GetInt32(0);
                        String sterm = reader.GetString(1);

                        foreach (var vitem in vm.Items)
                        {
                            if (vitem.ItemID == itemID)
                            {
                                vitem.TagTerms.Add(sterm);
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (bNotFound)
            {
                return NotFound();
            }
            else if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // POST: api/FinanceAssetSoldDocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAssetDocumentUIViewModel vm)
        {
            if (vm == null || vm.DocType != FinanceDocTypeViewModel.DocType_AssetSoldOut)
            {
                return BadRequest("No data is inputted");
            }
            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");

            // Do basic checks
            if (String.IsNullOrEmpty(vm.TranCurr) 
                || vm.AccountVM.ID <= 0
                || vm.AccountVM.CtgyID != FinanceAccountCtgyViewModel.AccountCategory_Asset)
                return BadRequest("Invalid input data");

            foreach (var di in vm.Items)
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

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nNewDocID = -1;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlTransaction tran = conn.BeginTransaction();
                SqlCommand cmd = null;

                try
                {
                    // First, craete the doc header => nNewDocID
                    queryString = SqlUtility.GetFinDocHeaderInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    SqlUtility.BindFinDocHeaderInsertParameter(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    cmd.Dispose();
                    cmd = null;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = SqlUtility.GetFinDocItemInsertString();
                        SqlCommand cmd2 = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        SqlUtility.BindFinDocItemInsertParameter(cmd2, ivm, nNewDocID);

                        await cmd2.ExecuteNonQueryAsync();

                        cmd2.Dispose();
                        cmd2 = null;

                        // Tags
                        if (ivm.TagTerms.Count > 0)
                        {
                            // Create tags
                            foreach (var term in ivm.TagTerms)
                            {
                                queryString = SqlUtility.GetTagInsertString();

                                cmd2 = new SqlCommand(queryString, conn, tran);

                                SqlUtility.BindTagInsertParameter(cmd2, vm.HID, HIHTagTypeEnum.FinanceDocumentItem, nNewDocID, term, ivm.ItemID);

                                await cmd2.ExecuteNonQueryAsync();

                                cmd2.Dispose();
                                cmd2 = null;
                            }
                        }
                    }

                    // Third, update the Account
                    queryString = SqlUtility.GetFinanceAccountHeaderUpdateString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    // Close this account
                    vm.AccountVM.Status = (Byte)FinanceAccountStatus.Closed;
                    SqlUtility.BindFinAccountUpdateParameter(cmd, vm.AccountVM, usrName);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Fourth, Update the Asset account part
                    queryString = SqlUtility.GetFinanceAccountAssetUpdateString();
                    vm.AccountVM.ExtraInfo_AS.RefDocForSold = nNewDocID;
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    SqlUtility.BindFinAccountAssetUpdateParameter(cmd, vm.AccountVM.ExtraInfo_AS);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Fifth, the tag

                    tran.Commit();
                }
                catch (Exception exp)
                {
                    if (tran != null)
                        tran.Rollback();

                    throw exp; // Re-throw
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewDocID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }
        
        // PUT: api/FinanceAssetSoldDocument/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // PATCH: 
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromQuery]int hid, [FromBody]JsonPatchDocument<FinanceAssetDocumentUIViewModel> patch)
        {
            if (patch == null || id <= 0 || patch.Operations.Count <= 0)
                return BadRequest("No data is inputted");
            if (hid <= 0)
                return BadRequest("No home is inputted");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean headTranDateUpdate = false;
            DateTime? headTranDate = null;
            Boolean headDespUpdate = false;
            String headDesp = null;

            // Check the inputs.
            // Allowed to change:
            //  1. Header: Transaction date, Desp;
            //  2. Item: Transaction amount, Desp, Control Center ID, Order ID,
            foreach (var oper in patch.Operations)
            {
                switch (oper.path)
                {
                    case "/tranDate":
                        headTranDateUpdate = true;
                        headTranDate = (DateTime)oper.value;
                        break;

                    case "/desp":
                        headDespUpdate = true;
                        headDesp = (String)oper.value;
                        break;

                    default:
                        return BadRequest("Unsupport field found");
                }
            }

            // User name
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

            try
            {
                queryString = SqlUtility.GetFinDocHeaderExistCheckString(id);

                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    throw new Exception("Doc ID not exist");
                }
                else
                {
                    reader.Close();
                    cmd.Dispose();
                    cmd = null;

                    //var vm = new FinanceAssetDocumentUIViewModel();

                    //// Header
                    //while (reader.Read())
                    //{
                    //    SqlUtility.FinDocHeader_DB2VM(reader, vm);
                    //}
                    //reader.NextResult();

                    //// Items
                    //while (reader.Read())
                    //{
                    //    FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                    //    SqlUtility.FinDocItem_DB2VM(reader, itemvm);

                    //    vm.Items.Add(itemvm);
                    //}
                    //reader.NextResult();

                    //// Account
                    //while (reader.Read())
                    //{
                    //    FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
                    //    Int32 aidx = 0;
                    //    aidx = SqlUtility.FinAccountHeader_DB2VM(reader, vmAccount, aidx);

                    //    vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
                    //    SqlUtility.FinAccountAsset_DB2VM(reader, vmAccount.ExtraInfo_AS, aidx);

                    //    vm.AccountVM = vmAccount;
                    //}
                    //reader.NextResult();

                    //reader.Dispose();
                    //reader = null;

                    //cmd.Dispose();
                    //cmd = null;

                    //// Now go ahead for the update
                    ////var patched = vm.Copy();
                    //patch.ApplyTo(vm, ModelState);
                    //if (!ModelState.IsValid)
                    //{
                    //    return new BadRequestObjectResult(ModelState);
                    //}

                    // Optimized logic go ahead
                    if (headTranDateUpdate || headDespUpdate)
                    {
                        queryString = SqlUtility.GetFinDocHeader_PatchString(headTranDateUpdate, headDespUpdate);
                        cmd = new SqlCommand(queryString, conn);
                        if (headTranDateUpdate)
                            cmd.Parameters.AddWithValue("@TRANDATE", headTranDate);
                        if (headDespUpdate)
                            cmd.Parameters.AddWithValue("@DESP", headDesp);
                        cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                        cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@ID", id);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return Ok();
        }

        // DELETE: api/FinanceAssetSoldDocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}
