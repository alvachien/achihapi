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

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
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
        public string Get(int id)
        {
            // TBD.
            return "value";
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

            if (geneMode)
            {
                EventGenerationInputViewModel datInput = new EventGenerationInputViewModel();
                datInput.StartTimePoint = vm.StartDate;
                datInput.EndTimePoint = vm.EndDate;
                datInput.Name = vm.Name;
                datInput.RptType = vm.RptType;
                var listRst = EventUtility.GenerateHabitDetails(datInput);
                return Json(listRst);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Model status is invalid");
            }
            if (vm.EndDate <= vm.StartDate)
            {
                return BadRequest("Date range is invaid");
            }
            if (vm.Count <= 0)
            {
                return BadRequest("Count is must");
            }

            Boolean unitMode = Startup.UnitTestMode;            

            if (unitMode)
            {
                var results = EventUtility.GenerateHabitDetails(new EventGenerationInputViewModel() {
                    StartTimePoint = vm.StartDate,
                    EndTimePoint = vm.EndDate,
                    RptType = vm.RptType,
                    Name = vm.Name
                });
                foreach(var result in results)
                {
                    EventHabitDetail detail = new EventHabitDetail();
                    detail.StartDate = result.StartTimePoint;
                    detail.EndDate = result.EndTimePoint;
                    vm.Details.Add(detail);
                }
                return Json(vm);
            }

            if (vm.HID <= 0)
                return BadRequest("Home not defined");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
            Int32 nDuplicatedID = -1;
            Int32 nNewID = -1;
            Boolean bError = false;
            String strErrMsg = "";
            var usr = User.FindFirst(c => c.Type == "sub");
            String usrName = String.Empty;
            if (usr != null)
                usrName = usr.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User is not recognized");

            try
            {
                queryString = @"SELECT [ID]
                            FROM [dbo].[t_event] WHERE [Name] = N'" + vm.Name + "'";

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

                    // Now go ahead for the creating
                    queryString = SqlUtility.Event_GetNormalEventInsertString();

                    cmd = new SqlCommand(queryString, conn);
                    //SqlUtility.Event_BindNormalEventInsertParameters(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;
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
        public void Delete(int id)
        {
        }
    }
}
