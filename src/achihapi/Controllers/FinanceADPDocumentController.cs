using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceADPDocumentController : Controller
    {
        // GET: api/financeadpdocument
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean skipPosted = true, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            return BadRequest();
        }

        // GET api/financeadpdocument/5
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

            FinanceADPDocumentUIViewModel vm = new FinanceADPDocumentUIViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = SqlUtility.getFinanceDocADPQueryString(id);

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

                Int32 nRstBatch = 0; // Total 4 batch!

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
                        while(reader.Read())
                        {
                            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
                            SqlUtility.FinAccount_DB2VM(reader, vmAccount);
                            vm.AccountVM = vmAccount;
                        }
                    } 
                    else if(nRstBatch == 3) // Tmp doc
                    {
                        while(reader.Read())
                        {
                            FinanceTmpDocDPViewModel dpvm = new FinanceTmpDocDPViewModel();
                            SqlUtility.FinTmpDoc_DB2VM(reader, dpvm);
                            vm.TmpDocs.Add(dpvm);
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
                conn.Close();
                conn.Dispose();
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
            ;
            return new JsonResult(vm, setting);
        }

        // POST api/financeadpdocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceADPDocumentUIViewModel vm)
        {
            if (vm == null || vm.DocType != FinanceDocTypeViewModel.DocType_AdvancePayment)
            {
                return BadRequest("No data is inputted");
            }
            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User info cannot fetch");

            // Check the items
            if (vm.Items.Count <= 0 || vm.TmpDocs.Count <= 0)
            {
                return BadRequest("No item or no template docs");
            }

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
                    queryString = SqlUtility.getFinanceAccountInsertString();

                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    SqlUtility.bindFinAccountParameter(cmd, vm.AccountVM, usrName);

                    SqlParameter idparam2 = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam2.Direction = ParameterDirection.Output;

                    nRst = await cmd.ExecuteNonQueryAsync();
                    Int32 nNewAccountID = (Int32)idparam2.Value;
                    cmd.Dispose();
                    cmd = null;

                    // Fourth, creat the ADP part
                    queryString = SqlUtility.getFinanceAccountADPInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    SqlUtility.bindFinAccountADPParameter(cmd, vm.AccountVM.AdvancePaymentInfo, nNewDocID, nNewAccountID, usrName);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Fifth, create template docs
                    foreach (FinanceTmpDocDPViewModel avm in vm.TmpDocs)
                    {
                        queryString = SqlUtility.getFinanceTmpDocADPInsertString();

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };

                        SqlUtility.bindFinTmpDocADPParameter(cmd, avm, nNewAccountID, usrName);
                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;
                    }

                    tran.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewDocID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
        }

        // PUT api/financeadpdocument/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/financeadpdocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}
