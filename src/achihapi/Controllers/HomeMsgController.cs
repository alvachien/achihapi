using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using Microsoft.AspNetCore.JsonPatch;
using achihapi.Utilities;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/HomeMsg")]
    [Authorize]
    public class HomeMsgController : Controller
    {
        // GET: api/HomeMsg
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean sentbox = false, Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<HomeMsgViewModel> listVm = new BaseListViewModel<HomeMsgViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";

            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                if (usrObj == null)
                    return BadRequest();
                var usrName = usrObj.Value;
                if (String.IsNullOrEmpty(usrName))
                    return BadRequest();

                queryString = this.getQueryString(true, top, skip, hid, sentbox, usrName);

                using (conn = new SqlConnection(Startup.DBConnectionString))
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
                            HomeMsgViewModel vm = new HomeMsgViewModel();
                            HIHDBUtility.HomeMsg_DB2VM(reader, vm);
                            listVm.Add(vm);
                        }
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
                    conn.Close();
                    conn.Dispose();
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

        // GET: api/HomeMsg/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return BadRequest();
        }
        
        // POST: api/HomeMsg
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]HomeMsgViewModel vm)
        {
            if (vm == null)
                return BadRequest();
            if (String.IsNullOrEmpty(vm.Title))
                return BadRequest("Title is a must");
            if (String.IsNullOrEmpty(vm.UserTo))
                return BadRequest("Who shall be send to");
            if (String.IsNullOrEmpty(vm.Content))
                return BadRequest("Content is a must");

            SqlConnection conn = null;
            SqlCommand cmd = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            if (usrObj == null)
                return BadRequest();
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest();
            if (String.CompareOrdinal(usrName, vm.UserFrom) != 0)
                return BadRequest("Cannot send message for others");

            try
            {
                queryString = GetInsertString();

                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    cmd = new SqlCommand(queryString, conn);
                    BindInsertParameters(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;
                    await cmd.ExecuteNonQueryAsync();

                    vm.ID = (Int32)idparam.Value;
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
                    conn.Close();
                    conn.Dispose();
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

        // PUT: api/HomeMsg/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // PATCH api/HomeMsg/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromQuery]int hid, [FromBody]JsonPatchDocument<HomeMsgViewModel> patch)
        {
            if (patch == null || id <= 0)
                return BadRequest("No data is inputted");
            if (hid <= 0)
                return BadRequest("No home is inputted");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";

            var usr = User.FindFirst(c => c.Type == "sub");
            String usrName = String.Empty;
            if (usr != null)
                usrName = usr.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User is not recognized");
            HomeMsgViewModel vm = new HomeMsgViewModel();

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
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
                        throw; // Re-throw
                    }

                    // Optimization logic for Mark as complete
                    if (patch.Operations.Count == 1 && patch.Operations[0].path == "/readFlag")
                    {
                        // Only update the complete time
                        queryString = HIHDBUtility.HomeMsg_GetMarkAsReadUpdateString();
                        cmd = new SqlCommand(queryString, conn);
                        HIHDBUtility.HomeMsg_BindMarkAsReadUpdateParameters(cmd, (Boolean)patch.Operations[0].value, id, hid);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    else if (patch.Operations.Count == 1 && patch.Operations[0].path == "/receiverDeletion")
                    {
                        // Only update the complete time
                        queryString = HIHDBUtility.HomeMsg_GetReceiverDeletionUpdateString();
                        cmd = new SqlCommand(queryString, conn);
                        HIHDBUtility.HomeMsg_BindReceiverDeletionUpdateParameters(cmd, (Boolean)patch.Operations[0].value, id, hid);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    else if (patch.Operations.Count == 1 && patch.Operations[0].path == "/senderDeletion")
                    {
                        // Only update the complete time
                        queryString = HIHDBUtility.HomeMsg_GetSenderDeletionUpdateString();
                        cmd = new SqlCommand(queryString, conn);
                        HIHDBUtility.HomeMsg_BindSenderDeletioUpdateParameters(cmd, (Boolean)patch.Operations[0].value, id, hid);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Non support patch mode!");
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
                    conn.Close();
                    conn.Dispose();
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

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return BadRequest();
        }

        #region Implementation methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32 hid, Boolean sentbox, String usrName)
        {

            String strSQL = "";
            if (bListMode)
            {
                if (sentbox)
                    strSQL += @"SELECT count(*) FROM [dbo].[t_homemsg] WHERE [HID] = " + hid.ToString() + " AND [USERFROM] = N'" + usrName + "'; ";
                else
                    strSQL += @"SELECT count(*) FROM [dbo].[t_homemsg] WHERE [HID] = " + hid.ToString() + " AND [USERTO] = N'" + usrName + "'; ";
            }

            strSQL += HIHDBUtility.getHomeMsgQueryString(usrName, hid, sentbox);

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && !String.IsNullOrEmpty(usrName))
            {
                strSQL += @" AND [t_homemsg].[USERTO] = " + usrName;
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine("HomeMsgController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }

        private string GetInsertString()
        {
            return @"INSERT INTO [dbo].[t_homemsg]
                       ([HID]
                       ,[USERTO]
                       ,[SENDDATE]
                       ,[USERFROM]
                       ,[TITLE]
                       ,[CONTENT]
                       ,[READFLAG])
                 VALUES
                       (@HID
                       ,@USERTO
                       ,@SENDDATE
                       ,@USERFROM
                       ,@TITLE
                       ,@CONTENT
                       ,@READFLAG ); SELECT @Identity = SCOPE_IDENTITY();";
        }
        private void BindInsertParameters(SqlCommand cmd, HomeMsgViewModel vm, String curuser)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@USERTO", vm.UserTo);
            cmd.Parameters.AddWithValue("@SENDDATE", DateTime.Now);
            cmd.Parameters.AddWithValue("@USERFROM", curuser);
            cmd.Parameters.AddWithValue("@TITLE", vm.Title);
            cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
            cmd.Parameters.AddWithValue("@READFLAG", false);
        }
        #endregion
    }
}
