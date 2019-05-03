using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.Utilities;
using System.Net;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class LearnHistoryController : Controller
    {
        // GET: api/learnhistory
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            BaseListViewModel<LearnHistoryUIViewModel> listVm = new BaseListViewModel<LearnHistoryUIViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);

                //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            try
            {

                queryString = this.getSQLString(true, top, skip, scopeFilter, hid);

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LearnHistoryUIViewModel vm = new LearnHistoryUIViewModel();
                            onDB2VM(reader, vm);
                            listVm.Add(vm);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listVm, setting);
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
            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);

                //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            LearnHistoryUIViewModel vm = new LearnHistoryUIViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                vm.ParseGeneratedKey(sid);

                queryString = this.getSQLString(false, null, null, String.Empty, null);

                using(conn = new SqlConnection(Startup.DBConnectionString))
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

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                    cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                    cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            onDB2VM(reader, vm);
                            // It should return one entry only!
                            // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;
                            break;
                        }
                    }
                    else
                    {
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
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

            // Only return the meaningful object
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
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

                // Disable the scope filter just for now, 2017.10.2

                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);
                //var scopeValue = scopeObj.Value;

                //if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                //{
                //    return StatusCode(401, "Current user has no authority to create learn history!");
                //}
                //else if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                //{
                //    if (String.CompareOrdinal(usrName, vm.UserID) != 0)
                //    {
                //        return StatusCode(401, "Current user cannot create the history where he/she is not responsible for.");
                //    }
                //}
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest();

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
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

                    // Do the check first: object id
                    String checkString = @"SELECT [ID] FROM [dbo].[t_learn_obj] WHERE [ID] = " + vm.ObjectID.ToString();
                    cmd = new SqlCommand(checkString, conn);
                    reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Invalid Object ID : " + vm.ObjectID.ToString());
                    }
                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Do the check: name
                    checkString = @"SELECT [USER] FROM [dbo].[t_homemem] WHERE [HID] = " + vm.HID.ToString() + " AND [USER] = N'" + vm.UserID + "'";
                    cmd = new SqlCommand(checkString, conn);
                    reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Invalid user ID : " + vm.UserID);
                    }
                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Do the check: primary key check
                    checkString = @"SELECT [HID], [USERID], [OBJECTID], [LEARNDATE] FROM [dbo].[t_learn_hist] WHERE [HID] = @hid AND [USERID] = @USERID AND [OBJECTID] = @OBJECTID AND [LEARNDATE] = @LEARNDATE";
                    cmd = new SqlCommand(checkString, conn);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                    cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                    cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Duplicated entry: user (" + vm.UserID + "), object (" + vm.ObjectID.ToString() + ") at date (" + vm.LearnDate.ToString() + ")");
                    }
                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Now go ahead for the creating
                    queryString = @"INSERT INTO [dbo].[t_learn_hist]
                           ([HID]
                           ,[USERID]
                           ,[OBJECTID]
                           ,[LEARNDATE]
                           ,[COMMENT]
                           ,[CREATEDBY]
                           ,[CREATEDAT])
                     VALUES (@HID
                           ,@USERID
                           ,@OBJECTID
                           ,@LEARNDATE
                           ,@COMMENT
                           ,@CREATEDBY
                           ,@CREATEDAT);";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                    cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                    cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                    if (!String.IsNullOrEmpty(vm.Comment))
                        cmd.Parameters.AddWithValue("@COMMENT", vm.Comment);
                    else
                        cmd.Parameters.AddWithValue("@COMMENT", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                    cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
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
                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);
                //var scopeValue = scopeObj.Value;
                //if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                //{
                //    return StatusCode(401, "Current user has no authority to change learn history!");
                //}
                //else if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                //{
                //    if (String.CompareOrdinal(usrName, vm.UserID) != 0)
                //    {
                //        return StatusCode(401, "Current user cannot change the history where he/she is not responsible for.");
                //    }
                //}
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
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

                    // Do the check first: object id
                    String checkString = @"SELECT [ID] FROM [dbo].[t_learn_obj] WHERE [ID] = " + vm.ObjectID.ToString();
                    cmd = new SqlCommand(checkString, conn);
                    reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Invalid Object ID : " + vm.ObjectID.ToString());
                    }
                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Do the check: name
                    checkString = @"SELECT [USER] FROM [dbo].[t_homemem] WHERE [HID] = " + vm.HID.ToString() + " AND [USER] = N'" + vm.UserID + "'";
                    cmd = new SqlCommand(checkString, conn);
                    reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Invalid user ID : " + vm.UserID);
                    }
                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Now go ahead for the creating
                    queryString = @"UPDATE [dbo].[t_learn_hist]
                           SET [COMMENT] = @COMMENT
                              ,[UPDATEDBY] = @UPDATEDBY
                              ,[UPDATEDAT] = @UPDATEDAT
                         WHERE [HID] = @HID
                              AND [USERID] = @USERID 
                              AND [OBJECTID] = @OBJECTID
                              AND [LEARNDATE] = @LEARNDATE";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@COMMENT", vm.Comment);
                    cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                    cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                    cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                    cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
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
                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.LearnHistoryScope);
                //var scopeValue = scopeObj.Value;
                //if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                //{
                //    return StatusCode(401, "Current user has no authority to delete history!");
                //}
                //else if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                //{
                //    if (String.CompareOrdinal(usrName, vm.UserID) != 0)
                //    {
                //        return StatusCode(401, "Current user cannot delete the history where he/she is not responsible for.");
                //    }
                //}
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
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

                    // Now go ahead for the delete
                    queryString = @"DELETE FROM [dbo].[t_learn_hist]
                           WHERE [HID] = @HID
                             AND [USERID] = @USERID
                             AND [OBJECTID] = @OBJECTID
                             AND [LEARNDATE] = @LEARNDATE;";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                    cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                    cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
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

            return Ok();
        }

        private string getSQLString(Boolean bListMode, Int32? nTop, Int32? nSkip, String userFilter, Int32? hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_hist] WHERE [hid] = " + hid.Value.ToString();

                if (!String.IsNullOrEmpty(userFilter))
                {
                    strSQL += " AND [USERID] = N'" + userFilter + "'";
                }

                strSQL += ";";
            }

            strSQL += HIHDBUtility.getLearnHistoryQueryString(hid, userFilter);
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode)
            {
                strSQL += @" WHERE [t_learn_hist].[HID] = @HID AND [t_learn_hist].[USERID] = @USERID AND [t_learn_hist].[OBJECTID] = @OBJECTID AND [t_learn_hist].[LEARNDATE] = @LEARNDATE ";
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LearnHistoryController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, LearnHistoryUIViewModel vm)
        {
            Int32 idx = 0;
            vm.HID = reader.GetInt32(idx++);
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
    }
}
