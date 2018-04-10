using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using achihapi.ViewModels;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/EventHabit")]
    [Authorize]
    public class EventHabitController : Controller
    {
        // GET: api/EventHabit
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");

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

            BaseListViewModel<EventHabitViewModel> listVm = new BaseListViewModel<EventHabitViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

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
                    return BadRequest(exp.Message);
                }

                queryString = SqlUtility.Event_GetEventHabitQueryString(true, usrName, hid, skip, top);

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
                        Dictionary<Int32, EventHabitViewModel> dictVM = new Dictionary<int, EventHabitViewModel>();
                        while (reader.Read())
                        {
                            EventHabitViewModel vm = new EventHabitViewModel();
                            EventHabitDetail detail = new EventHabitDetail();
                            SqlUtility.Event_HabitDB2VM(reader, vm, detail, true);
                            if (dictVM.ContainsKey(vm.ID))
                            {
                                dictVM[vm.ID].Details.Add(detail);
                            }
                            else
                            {
                                vm.Details.Add(detail);
                                listVm.Add(vm);
                                dictVM.Add(vm.ID, vm);
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
                strErrMsg = exp.Message;
                bError = true;
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

            if (bError)
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listVm, setting);
        }

        // GET: api/EventHabit/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");

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

            EventHabitViewModel vm = new EventHabitViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

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
                    return BadRequest(exp.Message);
                }

                queryString = SqlUtility.Event_GetEventHabitQueryString(false, usrName, hid, null, null, id);

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                // Detail
                while (reader.Read())
                {
                    EventHabitDetail detail = new EventHabitDetail();
                    SqlUtility.Event_HabitDB2VM(reader, vm, detail, false);
                    vm.Details.Add(detail);
                }
                reader.NextResult();

                // Checkin
                while(reader.Read())
                {
                    EventHabitCheckInViewModel civm = new EventHabitCheckInViewModel();
                    civm.ID = reader.GetInt32(0);
                    civm.TranDate = reader.GetDateTime(1);
                    if (!reader.IsDBNull(2))
                        civm.Score = reader.GetInt32(2);
                    if (!reader.IsDBNull(3))
                        civm.Comment = reader.GetString(3);
                    if (!reader.IsDBNull(4))
                        civm.CreatedBy = reader.GetString(4);
                    if (!reader.IsDBNull(5))
                        civm.CreatedAt = reader.GetDateTime(5);
                    if (!reader.IsDBNull(6))
                        civm.UpdatedBy = reader.GetString(6);
                    if (!reader.IsDBNull(7))
                        civm.UpdatedAt = reader.GetDateTime(7);
                    vm.CheckInLogs.Add(civm);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
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

            if (bError)
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // POST: api/EventHabit
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]EventHabitViewModel vm, [FromQuery]Boolean geneMode = false)
        {
            if (vm == null)
                return BadRequest("No data is inputted");

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
                return BadRequest("Name is a must!");

            // Check the details' generation
            EventGenerationInputViewModel datInput = new EventGenerationInputViewModel();
            datInput.StartTimePoint = vm.StartDate;
            datInput.EndTimePoint = vm.EndDate;
            datInput.Name = vm.Name;
            datInput.RptType = vm.RptType;

            List<EventGenerationResultViewModel> listDetails = null;
            try
            {
                listDetails = EventUtility.GenerateHabitDetails(datInput);
            }
            catch(Exception exp)
            {
                return BadRequest(exp.Message);
            }
            if (listDetails.Count <= 0)
            {
                return BadRequest("Failed to generate the details");
            }

            // For generation mode, just return the results
            if (geneMode)
            {
                return Json(listDetails);
            }

            // For non-generation mode, go ahead for creating
            if (!ModelState.IsValid)
            {
                return BadRequest("Model status is invalid");
            }
            if (vm.Count <= 0)
            {
                return BadRequest("Count is must");
            }

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
                            FROM [dbo].[t_event_habit] WHERE [Name] = N'" + vm.Name + "'";

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
                    queryString = SqlUtility.Event_GetEventHabitInsertString();
                    tran = conn.BeginTransaction();

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Transaction = tran;
                    SqlUtility.Event_BindEventHabitInsertParameters(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;

                    // Then go to the details.
                    foreach(var detail in listDetails)
                    {
                        EventHabitDetail detailVM = new EventHabitDetail();
                        detailVM.HabitID = nNewID;
                        detailVM.StartDate = detail.StartTimePoint;
                        detailVM.EndDate = detail.EndTimePoint;

                        queryString = SqlUtility.Event_GetEventHabitDetailInsertString();

                        cmd.Dispose();
                        cmd = null;
                        cmd = new SqlCommand(queryString, conn, tran);
                        SqlUtility.Event_BindEventHabitDetailInsertParameter(cmd, detailVM);
                        await cmd.ExecuteNonQueryAsync();

                        vm.Details.Add(detailVM);
                    }

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
                return BadRequest("Object with name already exists: " + nDuplicatedID.ToString());
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

        // PUT: api/EventHabit/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");

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

            EventHabitViewModel vm = new EventHabitViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

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
                    return BadRequest(exp.Message);
                }

                queryString = SqlUtility.Event_GetEventHabitDeleteString();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlUtility.Event_BindEventHabitDeleteParameters(cmd, id, hid);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
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

            if (bError)
                return StatusCode(500, strErrMsg);

            return Ok();
        }
    }
}
