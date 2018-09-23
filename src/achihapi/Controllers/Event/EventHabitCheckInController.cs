using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Data.SqlClient;
using achihapi.ViewModels;
using achihapi.Utilities;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/EventHabitCheckIn")]
    [Authorize]
    public class EventHabitCheckInController : Controller
    {
        // GET: api/EventHabitCheckIn
        [HttpGet]
        public IActionResult Get()
        {
            return Forbid();
        }

        // GET: api/EventHabitCheckIn/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Forbid();
        }
        
        // POST: api/EventHabitCheckIn
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]EventHabitCheckInViewModel vm)
        {
            if (vm == null)
                return BadRequest("No data is inputted");

            if (!ModelState.IsValid)
                return BadRequest("Model status is invalid");
            if (vm.HabitID <= 0 || vm.HID <= 0)
                return BadRequest("Invalid data: home ID, habit ID");

            Boolean unitMode = Startup.UnitTestMode;

            SqlConnection conn = null;
            String queryString = "";
            Int32 nNewID = -1;
            String strErrMsg = "";
            String usrName = String.Empty;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            if (unitMode)
                usrName = UnitTestUtility.UnitTestUser;
            else
            {
                var usr = User.FindFirst(c => c.Type == "sub");
                if (usr != null)
                    usrName = usr.Value;
                if (String.IsNullOrEmpty(usrName))
                    return BadRequest("User is not recognized");
            }

            SqlCommand cmd = null;
            SqlTransaction tran = null;

            try
            {
                queryString = @"SELECT [ID]
                            FROM [dbo].[t_event_habit_checkin] WHERE [TranDate] = @trandate AND [HabitID] = @habitid";

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
                        throw; // Re-throw
                    }

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@trandate", vm.TranDate);
                    cmd.Parameters.AddWithValue("@habitid", vm.HabitID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Int32 nDuplicatedID = -1;
                        while (reader.Read())
                        {
                            nDuplicatedID = reader.GetInt32(0);
                            break;
                        }

                        errorCode = HttpStatusCode.BadRequest;
                        strErrMsg = "Event has been checked in at same date: " + nDuplicatedID.ToString();
                        throw new Exception(strErrMsg);
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        // Now go ahead for the creating
                        queryString = HIHDBUtility.Event_GetEventHabitCheckInInsertString();
                        tran = conn.BeginTransaction();

                        cmd = new SqlCommand(queryString, conn);
                        cmd.Transaction = tran;
                        HIHDBUtility.Event_BindEventHabitCheckInInsertParameters(cmd, vm, usrName);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewID = (Int32)idparam.Value;

                        tran.Commit();
                    }
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }

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
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
                if (conn != null)
                {
                    conn.Close();
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

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // PUT: api/EventHabitCheckIn/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
