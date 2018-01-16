using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using Microsoft.AspNetCore.JsonPatch;

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
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
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
                                HomeMsgViewModel vm = new HomeMsgViewModel();
                                SqlUtility.HomeMsg_DB2VM(reader, vm);
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

        // GET: api/HomeMsg/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
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

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                if (usrObj == null)
                    return BadRequest();
                var usrName = usrObj.Value;
                if (String.IsNullOrEmpty(usrName))
                    return BadRequest();
                if (String.CompareOrdinal(usrName, vm.UserFrom) != 0)
                    return BadRequest("Cannot send message for others");

                queryString = GetInsertString();

                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                BindInsertParameters(cmd, vm, usrName);
                SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                idparam.Direction = ParameterDirection.Output;
                await cmd.ExecuteNonQueryAsync();

                vm.ID = (Int32)idparam.Value;
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

            return new JsonResult(vm, setting);
        }

        // PUT: api/HomeMsg/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
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
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bNonExistEntry = false;
            Boolean bError = false;
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

                // Optimization logic for Mark as complete
                if (patch.Operations.Count == 1 && patch.Operations[0].path == "/readFlag")
                {
                    // Only update the complete time
                    queryString = SqlUtility.HomeMsg_GetMarkAsReadUpdateString();
                    SqlCommand cmdupdate = new SqlCommand(queryString, conn);
                    SqlUtility.HomeMsg_BindMarkAsReadUpdateParameters(cmdupdate, (Boolean)patch.Operations[0].value, id, hid);

                    await cmdupdate.ExecuteNonQueryAsync();
                }
                else if (patch.Operations.Count == 1 && patch.Operations[0].path == "/receiverDeletion")
                {
                    // Only update the complete time
                    queryString = SqlUtility.HomeMsg_GetReceiverDeletionUpdateString();
                    SqlCommand cmdupdate = new SqlCommand(queryString, conn);
                    SqlUtility.HomeMsg_BindReceiverDeletionUpdateParameters(cmdupdate, (Boolean)patch.Operations[0].value, id, hid);

                    await cmdupdate.ExecuteNonQueryAsync();
                }
                else if (patch.Operations.Count == 1 && patch.Operations[0].path == "/senderDeletion")
                {
                    // Only update the complete time
                    queryString = SqlUtility.HomeMsg_GetSenderDeletionUpdateString();
                    SqlCommand cmdupdate = new SqlCommand(queryString, conn);
                    SqlUtility.HomeMsg_BindSenderDeletioUpdateParameters(cmdupdate, (Boolean)patch.Operations[0].value, id, hid);

                    await cmdupdate.ExecuteNonQueryAsync();
                }
                else
                {
                    return BadRequest("Non support patch mode!");
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

            return new JsonResult(vm, setting);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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

            strSQL += SqlUtility.getHomeMsgQueryString(usrName, hid, sentbox);

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
