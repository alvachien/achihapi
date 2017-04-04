using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class LearnHistoryController : Controller
    {
        // GET: api/learnhistory
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<LearnHistoryUIViewModel> listVm = new BaseListViewModel<LearnHistoryUIViewModel>();
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
                    var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);

                    scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                queryString = this.getSQLString(true, top, skip, scopeFilter);

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
                        while (reader.Read())
                        {
                            LearnHistoryUIViewModel vm = new LearnHistoryUIViewModel();
                            onDB2VM(reader, vm);
                            listVm.Add(vm);
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
                return StatusCode(500, strErrMsg);

            return new ObjectResult(listVm);
        }

        private string getSQLString(Boolean bListMode, Int32? nTop, Int32? nSkip, String userFilter)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_hist]";
                if (String.IsNullOrEmpty(userFilter))
                    strSQL += ";";
                else
                    strSQL += " WHERE [USERID] = N'" + userFilter + "';";
            }

            strSQL += SqlUtility.getLearnHistoryQueryString(userFilter);
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode)
            {
                strSQL += @" WHERE [t_learn_hist].[USERID] = @USERID AND [t_learn_hist].[OBJECTID] = @OBJECTID AND [t_learn_hist].[LEARNDATE] = @LEARNDATE ";
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LearnHistoryController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, LearnHistoryUIViewModel vm)
        {
            Int32 idx = 0;
            vm.UserID = reader.GetString(idx++);
            vm.UserDisplayAs = reader.GetString(idx++);
            vm.ObjectID = reader.GetInt32(idx++);
            vm.ObjectName = reader.GetString(idx++);
            vm.LearnDate = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                vm.Comment = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
        }

        // GET api/learnhistory/5
        [HttpGet("{sid}")]
        [Authorize]
        public async Task<IActionResult> Get(String sid)
        {
            if (String.IsNullOrEmpty(sid))
            {
                return BadRequest("No data is inputted");
            }

            LearnHistoryUIViewModel vm = new LearnHistoryUIViewModel();
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
                    var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);

                    scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                vm.ParseGeneratedKey(sid);

                String usrID = vm.UserID;
                Int32 objID = vm.ObjectID;
                DateTime dtDate = vm.LearnDate;
                queryString = this.getSQLString(false, null, null, String.Empty);

                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", usrID);
                cmd.Parameters.AddWithValue("@OBJECTID", objID);
                cmd.Parameters.AddWithValue("@LEARNDATE", dtDate);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bExist = true;
                    while (reader.Read())
                    {
                        onDB2VM(reader, vm);
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
            return new ObjectResult(vm);
        }

        // POST api/learnhistory, create a learn history
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]LearnHistoryViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);
                var scopeValue = scopeObj.Value;

                if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to create learn history!");
                }
                else if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                {
                    if (String.CompareOrdinal(usrName, vm.UserID) != 0)
                    {
                        return StatusCode(401, "Current user cannot create the history where he/she is not responsible for.");
                    }
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            var usr = User.FindFirst(c => c.Type == "sub");
            if (usr != null)
                usrName = usr.Value;

            try
            {
                await conn.OpenAsync();

                // Do the check first: object id
                String checkString = @"SELECT [ID]
                            FROM [dbo].[t_learn_obj] WHERE [ID] = " + vm.ObjectID.ToString();
                SqlCommand chkcmd = new SqlCommand(checkString, conn);
                SqlDataReader chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid Object ID : " + vm.ObjectID.ToString());
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Do the check: name
                checkString = @"SELECT [USERID] FROM [dbo].[t_userdetail] WHERE [USERID] = N'" + vm.UserID + "'";
                chkcmd = new SqlCommand(checkString, conn);
                chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid user ID : " + vm.UserID);
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Do the check: primary key check
                checkString = @"SELECT [USERID], [OBJECTID], [LEARNDATE] FROM [dbo].[t_learn_hist] WHERE [USERID] = @USERID AND [OBJECTID] = @OBJECTID AND [LEARNDATE] = @LEARNDATE";
                chkcmd = new SqlCommand(checkString, conn);
                chkcmd.Parameters.AddWithValue("@USERID", vm.UserID);
                chkcmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                chkcmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                chkreader = chkcmd.ExecuteReader();
                if (chkreader.HasRows)
                {
                    return BadRequest("Duplicated entry: user (" + vm.UserID + "), object (" + vm.ObjectID.ToString() + ") at date (" + vm.LearnDate.ToString() + ")");
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Now go ahead for the creating
                queryString = @"INSERT INTO [dbo].[t_learn_hist]
                           ([USERID]
                           ,[OBJECTID]
                           ,[LEARNDATE]
                           ,[COMMENT]
                           ,[CREATEDBY]
                           ,[CREATEDAT]
                           ,[UPDATEDBY]
                           ,[UPDATEDAT])
                     VALUES
                           (@USERID
                           ,@OBJECTID
                           ,@LEARNDATE
                           ,@COMMENT
                           ,@CREATEDBY
                           ,@CREATEDAT
                           ,@UPDATEDBY
                           ,@UPDATEDAT);";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                cmd.Parameters.AddWithValue("@COMMENT", vm.Comment);
                cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);

                Int32 nRst = await cmd.ExecuteNonQueryAsync();
                //if (nRst )
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

            return new ObjectResult(vm);
        }

        // PUT api/learnhistory/5_a, change
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(String sid, [FromBody]LearnHistoryViewModel vm)
        {
            if (vm == null || String.CompareOrdinal(sid, vm.GeneratedKey()) != 0)
            {
                return BadRequest("No data is inputted");
            }

            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);
                var scopeValue = scopeObj.Value;
                if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to change learn history!");
                }
                else if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                {
                    if (String.CompareOrdinal(usrName, vm.UserID) != 0)
                    {
                        return StatusCode(401, "Current user cannot change the history where he/she is not responsible for.");
                    }
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                await conn.OpenAsync();

                // Do the check first: object id
                String checkString = @"SELECT [ID]
                            FROM [dbo].[t_learn_obj] WHERE [ID] = " + vm.ObjectID.ToString();
                SqlCommand chkcmd = new SqlCommand(checkString, conn);
                SqlDataReader chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid Object ID : " + vm.ObjectID.ToString());
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Do the check: name
                checkString = @"SELECT [USERID] FROM [dbo].[t_userdetail] WHERE [USERID] = N'" + vm.UserID + "'";
                chkcmd = new SqlCommand(checkString, conn);
                chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid user ID : " + vm.UserID);
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Now go ahead for the creating
                queryString = @"UPDATE [dbo].[t_learn_hist]
                           SET [COMMENT] = @COMMENT
                              ,[UPDATEDBY] = @UPDATEDBY
                              ,[UPDATEDAT] = @UPDATEDAT
                         WHERE [USERID] = @USERID 
                              AND [OBJECTID] = @OBJECTID
                              AND [LEARNDATE] = @LEARNDATE";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@COMMENT", vm.Comment);
                cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);

                Int32 nRst = await cmd.ExecuteNonQueryAsync();
                //if (nRst )
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

            return new ObjectResult(vm);
        }

        // DELETE api/learnhistory/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string sid)
        {
            if (String.IsNullOrEmpty(sid))
            {
                return BadRequest("No data is inputted");
            }

            LearnHistoryViewModel vm = new LearnHistoryViewModel();
            if (!vm.ParseGeneratedKey(sid))
            {
                return BadRequest("Key is not recognized: " + sid);
            }

            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);
                var scopeValue = scopeObj.Value;
                if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to delete history!");
                }
                else if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                {
                    if (String.CompareOrdinal(usrName, vm.UserID) != 0)
                    {
                        return StatusCode(401, "Current user cannot delete the history where he/she is not responsible for.");
                    }
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                await conn.OpenAsync();

                // Now go ahead for the delete
                queryString = @"DELETE FROM [dbo].[t_learn_hist]
                           WHERE [USERID] = @USERID
                             AND [OBJECTID] = @OBJECTID
                             AND [LEARNDATE] = @LEARNDATE;";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);

                Int32 nRst = await cmd.ExecuteNonQueryAsync();
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

            return Ok();
        }
    }
}
