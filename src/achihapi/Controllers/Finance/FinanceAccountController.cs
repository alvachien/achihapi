using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.Utilities;
using Microsoft.AspNetCore.JsonPatch;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceAccountController : Controller
    {
        // GET: api/financeaccount
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Byte? status = null, Int32 top = 100, Int32 skip = 0)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");

            BaseListViewModel<FinanceAccountUIViewModel> listVm = new BaseListViewModel<FinanceAccountUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                String usrName = "";
                String scopeFilter = String.Empty;
                try
                {
                    if (Startup.UnitTestMode)
                        usrName = UnitTestUtility.UnitTestUser;
                    else
                    {
                        var usrObj = HIHAPIUtility.GetUserClaim(this);
                        usrName = usrObj.Value;
                        //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                        //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                    }
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                if (String.IsNullOrEmpty(usrName))
                    return BadRequest("No user found");

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

                queryString = this.getQueryString(true, status, top, skip, null, scopeFilter, hid);

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                // 1. Count
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        listVm.TotalCount = reader.GetInt32(0);
                        break;
                    }
                    await reader.NextResultAsync();
                }

                // 2. Records
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceAccountUIViewModel vm = new FinanceAccountUIViewModel();
                        HIHDBUtility.FinAccount_DB2VM(reader, vm);
                        listVm.Add(vm);
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
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(listVm, setting);
        }

        // GET api/financeaccount/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bExist = false;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                String usrName = "";
                String scopeFilter = String.Empty;
                try
                {
                    if (Startup.UnitTestMode)
                        usrName = UnitTestUtility.UnitTestUser;
                    else
                    {
                        var usrObj = HIHAPIUtility.GetUserClaim(this);
                        usrName = usrObj.Value;
                        //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                        //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                    }
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }
                if (String.IsNullOrEmpty(usrName))
                {
                    return BadRequest("No user found");
                }

                queryString = this.getQueryString(false, null, null, null, id, scopeFilter, null);

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
                if (reader.HasRows)
                {
                    bExist = true;
                    while (reader.Read())
                    {
                        HIHDBUtility.FinAccount_DB2VM(reader, vmAccount);

                        // It should return one entry only!
                        // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;

                        break;
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

            // In case not found, return a 404
            if (!bExist)
                return NotFound();
            else if (bError)
                return StatusCode(500, strErrMsg);

            // Only return the meaningful object
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vmAccount, setting);
        }

        // POST api/financeaccount
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAccountViewModel vm)
        {
            if (vm == null 
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Loan)
            {
                return BadRequest("No data is inputted or inputted data for Advance payment/Asset/Loan");
            }

            String usrName = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                    //if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                    //{
                    //    return StatusCode(401, "Current user has no authority to create account!");
                    //}
                    //else if(String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                    //{
                    //    if (String.CompareOrdinal(vm.Owner, usrName) != 0)
                    //    {
                    //        return StatusCode(401, "Current user can only create account with owner.");
                    //    }
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

            if (vm.HID <= 0)
                return BadRequest("No HID inputted!");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
            Int32 nDuplicatedID = -1;
            Int32 nNewID = -1;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_account] WHERE [HID] = " + vm.HID.ToString() + " AND [Name] = N'" + vm.Name + "'";

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

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bDuplicatedEntry = true;
                    while (reader.Read())
                    {
                        nDuplicatedID = reader.GetInt32(0);
                        break;
                    }
                }
                else
                {
                    reader.Dispose();
                    reader = null;

                    cmd.Dispose();
                    cmd = null;

                    SqlTransaction tran = conn.BeginTransaction();

                    // Now go ahead for the creating
                    queryString = HIHDBUtility.GetFinanceAccountHeaderInsertString();

                    try
                    {
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };

                        HIHDBUtility.BindFinAccountInsertParameter(cmd, vm, usrName);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewID = (Int32)idparam.Value;

                        // Now commit it!
                        tran.Commit();
                    }
                    catch (Exception exp)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                        if (tran != null)
                            tran.Rollback();
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

            if (bDuplicatedEntry)
            {
                return BadRequest("Account already exists: " + nDuplicatedID.ToString());
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PUT api/financeaccount/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]FinanceAccountViewModel vm)
        {
            if (vm == null || vm.HID <= 0)
            {
                return BadRequest("Invalid inputted data, such as miss HID");
            }

            String usrName = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                    //if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                    //{
                    //    return StatusCode(401, "Current user has no authority to change account!");
                    //}
                    //else if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                    //{
                    //    if (String.CompareOrdinal(vm.Owner, usrName) != 0)
                    //    {
                    //        return StatusCode(401, "Current user can only modify account with owner.");
                    //    }
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name) || vm.ID != id)
            {
                return BadRequest("Name is a must!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            SqlTransaction tran = null;

            try
            {
                queryString = @"UPDATE [dbo].[t_fin_account]
                       SET [CTGYID] = @CTGYID
                          ,[NAME] = @NAME
                          ,[COMMENT] = @COMMENT
                          ,[OWNER] = @OWNER
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";

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

                tran = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
                cmd.Parameters.AddWithValue("@NAME", vm.Name);
                cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
                if (String.IsNullOrEmpty(vm.Owner))
                {
                    cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
                }
                cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                vm.UpdatedAt = DateTime.Now;
                cmd.Parameters.AddWithValue("@UPDATEDAT", vm.UpdatedAt);
                cmd.Parameters.AddWithValue("@ID", vm.ID);
                await cmd.ExecuteNonQueryAsync();

                // Now commit it!
                tran.Commit();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                if (tran != null)
                    tran.Rollback();
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PATCH api/event/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromQuery]int hid, [FromBody]JsonPatchDocument<FinanceAccountUIViewModel> patch)
        {
            if (patch == null || id <= 0)
                return BadRequest("No data is inputted");
            if (hid <= 0)
                return BadRequest("No home is inputted");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            SqlTransaction tran = null;
            String queryString = "";
            Boolean bNonExistEntry = false;
            Boolean bError = false;
            String strErrMsg = "";

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
            //FinanceAccountViewModel vm = new FinanceAccountViewModel();
            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();

            try
            {
                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                }
                catch (Exception exp)
                {
                    throw exp; // Re-throw
                }

                // Read the account
                queryString = this.getQueryString(false, null, null, null, id, String.Empty, null);
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {                        
                        HIHDBUtility.FinAccount_DB2VM(reader, vmAccount);

                        // It should return one entry only!
                        // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;

                        break;
                    }
                }
                else
                {
                    bNonExistEntry = true;
                }
                reader.Close();
                reader = null;
                cmd.Dispose();
                cmd = null;

                if (!bNonExistEntry)
                {
                    // Optimization logic for Status change
                    if (patch.Operations.Count == 1 && patch.Operations[0].path == "/status")
                    {
                        // Only update the status
                        tran = conn.BeginTransaction();

                        queryString = HIHDBUtility.GetFinanceAccountStatusUpdateString();
                        SqlCommand cmdupdate = new SqlCommand(queryString, conn, tran);

                        FinanceAccountStatus newstatus = (FinanceAccountStatus)Byte.Parse((string)patch.Operations[0].value);
                        vmAccount.Status = newstatus;
                        HIHDBUtility.BindFinAccountStatusUpdateParameter(cmdupdate, newstatus, id, hid, usrName);
                        await cmdupdate.ExecuteNonQueryAsync();

                        if (newstatus == FinanceAccountStatus.Closed)
                        {
                            // Close account.
                            if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment)
                            {
                                // It have to stop all unposted advance payment
                                queryString = HIHDBUtility.GetFinanceDocADPDeleteString(hid, vmAccount.ID, true);
                                SqlCommand cmdTmpDoc = new SqlCommand(queryString, conn, tran);
                                await cmdTmpDoc.ExecuteNonQueryAsync();
                            }
                            else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                            {
                                // For asset
                            }
                            else if(vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Loan)
                            {
                                // It have to stop all unposted advance payment
                                queryString = HIHDBUtility.GetFinanceDocLoanDeleteString(hid, vmAccount.ID, true);
                                SqlCommand cmdTmpDoc = new SqlCommand(queryString, conn, tran);
                                await cmdTmpDoc.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                // Normal case
                            }
                        }

                        tran.Commit();
                    }
                    else
                    {
                        throw new Exception("Not supported yet");

                        // Now go ahead for the update
                        //var patched = vm.Copy();
                        patch.ApplyTo(vmAccount, ModelState);
                        if (!ModelState.IsValid)
                        {
                            return new BadRequestObjectResult(ModelState);
                        }

                        //var model = new
                        //{
                        //    original = vm,
                        //    patched = vm
                        //};

                        //queryString = HIHDBUtility.Event_GetNormalEventUpdateString();

                        //cmd = new SqlCommand(queryString, conn);
                        //HIHDBUtility.Event_BindNormalEventUpdateParameters(cmd, vm, usrName);

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                if (tran != null)
                {
                    tran.Rollback();
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                    conn = null;
                }
            }

            if (bNonExistEntry)
            {
                return BadRequest("Object with ID doesnot exist: " + id.ToString());
            }

            if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vmAccount, setting);
        }

        // DELETE api/financeaccount/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, [FromQuery]Int32 hid = 0)
        {
            String usrName = "";
            String scopeValue = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);
                    //scopeValue = scopeObj.Value;

                    //if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                    //{
                    //    return StatusCode(401, "Current user has no authority to create account!");
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (String.IsNullOrEmpty(usrName))
                return BadRequest("No user found");
            if (hid == 0)
                return BadRequest("No HID inputted!");

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            SqlTransaction tran = null;

            try
            {
                // Check owner and the existence
                queryString = @"SELECT [OWNER] FROM [dbo].[t_fin_account] WHERE [ID] = " + id.ToString() + " AND [HID] = " + hid.ToString();
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

                SqlCommand cmdread = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmdread.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        String strOwner = reader.GetString(0);
                        if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                        {
                            if (String.CompareOrdinal(strOwner, usrName) != 0)
                            {
                                return StatusCode(401, "Current user can only delete the account with owner");
                            }
                        }

                        break;
                    }
                }
                else
                {
                    return StatusCode(500, "Account not exist!");
                }
                reader.Dispose();
                cmdread.Dispose();

                // Deletion
                queryString = @"DELETE FROM [dbo].[t_fin_account] 
                     WHERE [ID] = @ID";

                await conn.OpenAsync();

                tran = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@ID", id);
                await cmd.ExecuteNonQueryAsync();

                // Ext. info
                queryString = @"DELETE FROM [dbo].[t_fin_account_ext_dp] WHERE [ACCOUNTID] = @ACCOUNTID";
                cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@ACCOUNTID", id);
                await cmd.ExecuteNonQueryAsync();

                // Now commit it!
                tran.Commit();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                if (tran != null)
                    tran.Rollback();
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

        #region Implemented methods
        private string getQueryString(Boolean bListMode, Byte? status, Int32? nTop, Int32? nSkip, Int32? nSearchID, String strOwner, Int32? hid)
        {
            
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_account] WHERE [hid] = " + hid.Value.ToString();
                if (status.HasValue)
                {
                    if (status.Value == 0)
                        strSQL += " AND ( [STATUS] = 0 OR [STATUS] IS NULL) ";
                    else
                        strSQL += " AND [STATUS] = " + status.Value.ToString();
                }
                    
                if (!String.IsNullOrEmpty(strOwner))
                    strSQL += " AND [OWNER] = N'" + strOwner + "'";
                strSQL += ";";
            }

            strSQL += HIHDBUtility.getFinanceAccountQueryString(hid, status, strOwner);

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                if (!String.IsNullOrEmpty(strOwner))
                {
                    strSQL += @" AND [t_fin_account].[ID] = " + nSearchID.Value.ToString();
                }
                else
                {
                    strSQL += @" WHERE [t_fin_account].[ID] = " + nSearchID.Value.ToString();
                }
            }

            return strSQL;
        }
        #endregion
    }
}
