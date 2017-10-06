using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceAssetBuyDocument")]
    public class FinanceAssetBuyDocumentController : Controller
    {
        // GET: api/FinanceAssetBuyDocument
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            return BadRequest();
        }

        // GET: api/FinanceAssetBuyDocument/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User info cannot fetch");

            FinanceAssetDocumentUIViewModel vm = new FinanceAssetDocumentUIViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = SqlUtility.getFinanceDocAssetQueryString(id, true);

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

                Int32 nRstBatch = 0; // Total 3 batch!

                while (reader.HasRows)
                {
                    if (nRstBatch == 0) // Doc. header
                    {
                        // Header
                        while (reader.Read())
                        {
                            SqlUtility.FinDocHeader_DB2VM(reader, vm);
                        }
                    }
                    else if (nRstBatch == 1) // Doc item
                    {
                        // Items
                        while (reader.Read())
                        {
                            FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                            SqlUtility.FinDocItem_DB2VM(reader, itemvm);

                            vm.Items.Add(itemvm);
                        }
                    }
                    else if (nRstBatch == 2) // Account
                    {
                        while (reader.Read())
                        {
                            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
                            Int32 aidx = 0;
                            aidx = SqlUtility.FinAccountHeader_DB2VM(reader, vmAccount, aidx);

                            vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
                            SqlUtility.FinAccountAsset_DB2VM(reader, vmAccount.ExtraInfo_AS, aidx);

                            vm.AccountVM = vmAccount;
                        }
                    }

                    ++nRstBatch;

                    reader.NextResult();
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
        
        // POST: api/FinanceAssetBuyDocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAssetDocumentUIViewModel vm)
        {
            if (vm == null || vm.DocType != FinanceDocTypeViewModel.DocType_AssetBuyIn)
            {
                return BadRequest("No data is inputted");
            }
            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User info cannot fetch");

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
                    queryString = SqlUtility.getFinDocHeaderInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    SqlUtility.bindFinDocHeaderParameter(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    cmd.Dispose();
                    cmd = null;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = SqlUtility.getFinDocItemInsertString();
                        SqlCommand cmd2 = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        SqlUtility.bindFinDocItemParameter(cmd2, ivm, nNewDocID);

                        await cmd2.ExecuteNonQueryAsync();

                        cmd2.Dispose();
                        cmd2 = null;
                    }

                    // Third, go to the account creation => nNewAccountID
                    queryString = SqlUtility.GetFinanceAccountHeaderInsertString();

                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    SqlUtility.BindFinAccountInsertParameter(cmd, vm.AccountVM, usrName);

                    SqlParameter idparam2 = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam2.Direction = ParameterDirection.Output;

                    nRst = await cmd.ExecuteNonQueryAsync();
                    Int32 nNewAccountID = (Int32)idparam2.Value;
                    cmd.Dispose();
                    cmd = null;

                    // Fourth, creat the Asset part
                    queryString = SqlUtility.GetFinanceAccountAssetInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    SqlUtility.BindFinAccountAssetInsertParameter(cmd, vm.AccountVM.ExtraInfo_AS, nNewDocID, nNewAccountID);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    tran.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                    if (tran != null)
                        tran.Rollback();
                    bError = true;
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

        // PUT: api/FinanceAssetBuyDocument/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}
