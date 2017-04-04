using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceAccountController : Controller
    {
        // GET: api/financeaccount
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
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
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);


                    scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                queryString = this.getQueryString(true, top, skip, null, scopeFilter);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceAccountUIViewModel vm = new FinanceAccountUIViewModel();
                                SqlUtility.FinAccount_DB2VM(reader, vm);
                                listVm.Add(vm);
                            }
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

            if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            return new ObjectResult(listVm);
        }

        // GET api/financeaccount/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
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
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);


                    scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                queryString = this.getQueryString(false, null, null, id, scopeFilter);

                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bExist = true;
                    while (reader.Read())
                    {
                        SqlUtility.FinAccount_DB2VM(reader, vmAccount);

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
                conn.Close();
                conn.Dispose();
            }

            // In case not found, return a 404
            if (!bExist)
                return NotFound();
            else if (bError)
                return StatusCode(500, strErrMsg);

            // Only return the meaningful object
            return new ObjectResult(vmAccount);
        }

        // POST api/financeaccount
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAccountViewModel vm)
        {
            if (vm == null || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment)
            {
                return BadRequest("No data is inputted or inputted data for Advance payment");
            }

            String usrName = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to create account!");
                }
                else if(String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                {
                    if (String.CompareOrdinal(vm.Owner, usrName) != 0)
                    {
                        return StatusCode(401, "Current user can only create account with owner.");
                    }
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
                  FROM [dbo].[t_fin_account] WHERE [Name] = N'" + vm.Name + "'";

                await conn.OpenAsync();

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
                    queryString = SqlUtility.getFinanceAccountInsertString();

                    try
                    {
                        cmd = new SqlCommand(queryString, conn);
                        cmd.Transaction = tran;

                        SqlUtility.bindFinAccountParameter(cmd, vm, usrName);
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
                conn.Close();
                conn.Dispose();
            }

            if (bDuplicatedEntry)
            {
                return BadRequest("Account already exists: " + nDuplicatedID.ToString());
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            return new ObjectResult(vm);
        }

        // PUT api/financeaccount/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]FinanceAccountViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }
            String usrName = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to change account!");
                }
                else if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                {
                    if (String.CompareOrdinal(vm.Owner, usrName) != 0)
                    {
                        return StatusCode(401, "Current user can only modify account with owner.");
                    }
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

                SqlTransaction tran = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
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
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(vm);
        }

        // DELETE api/financeaccount/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            String usrName = "";
            String scopeValue = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);
                scopeValue = scopeObj.Value;

                if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to create account!");
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                // Check owner and the existence
                queryString = @"SELECT [OWNER] FROM [dbo].[t_fin_account] WHERE [ID] = " + id.ToString();
                await conn.OpenAsync();
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

                SqlTransaction tran = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
                cmd.Parameters.AddWithValue("@ID", id);
                await cmd.ExecuteNonQueryAsync();

                // Ext. info
                queryString = @"DELETE FROM [dbo].[t_fin_account_ext_dp] WHERE [ACCOUNTID] = @ACCOUNTID";
                cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
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
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return Ok();
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, String strOwner)
        {
            
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_account] ";
                if (!String.IsNullOrEmpty(strOwner))
                {
                    strSQL += " WHERE [OWNER] = N'" + strOwner + "'";
                }
                strSQL += " ;";
            }

            strSQL += SqlUtility.getFinanceAccountQueryString(strOwner);

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
#if DEBUG
            System.Diagnostics.Debug.WriteLine("FinanceAccountController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }
    }
}
