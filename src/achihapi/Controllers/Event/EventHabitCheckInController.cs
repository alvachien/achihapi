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

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/EventHabitCheckIn")]
    [Authorize]
    public class EventHabitCheckInController : Controller
    {
        // GET: api/EventHabitCheckIn
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/EventHabitCheckIn/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
            Int32 nDuplicatedID = -1;
            Int32 nNewID = -1;
            Boolean bError = false;
            String strErrMsg = "";
            String usrName = String.Empty;

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

                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                }
                catch (Exception exp)
                {
                    throw exp; // Re-throw
                }

                cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@trandate", vm.TranDate);
                cmd.Parameters.AddWithValue("@habitid", vm.HabitID);
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
            catch (Exception exp)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }

                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;
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
                }
            }

            if (bDuplicatedEntry)
            {
                return BadRequest("Event has been checked in at same date: " + nDuplicatedID.ToString());
            }

            if (bError)
            {
                return StatusCode(500, strErrMsg);
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
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
