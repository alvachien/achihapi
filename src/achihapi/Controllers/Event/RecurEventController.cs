using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;
using achihapi.ViewModels;
using achihapi.Utilities;
using System.Data;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/RecurEvent")]
    [Authorize]
    public class RecurEventController : Controller
    {
        // GET: api/RecurEvent
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

            BaseListViewModel<RecurEventViewModel> listVm = new BaseListViewModel<RecurEventViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

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

                queryString = HIHDBUtility.Event_GetRecurEventQueryString(true, usrName, hid, skip, top);

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

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
                        RecurEventViewModel vm = new RecurEventViewModel();
                        HIHDBUtility.Event_RecurDB2VM(reader, vm, true);
                        listVm.Add(vm);
                    }
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

        // GET: api/RecurEvent/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");
            if (id <= 0)
                return BadRequest("Invalid ID");

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

            RecurUIEventViewModel vm = new RecurUIEventViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

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

                queryString = HIHDBUtility.Event_GetRecurEventQueryString(false, usrName, hid, null, null, id);

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        HIHDBUtility.Event_RecurDB2VM(reader, vm, false);
                    }
                }

                reader.Close();
                cmd.Dispose();
                cmd = null;

                queryString = HIHDBUtility.Event_GetNormalEventByRecurIDString();
                cmd = new SqlCommand(queryString, conn);
                HIHDBUtility.Event_BindNormalEventForRecurDeletionParameters(cmd, hid, id);
                reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var vmevent = new EventViewModel();
                        HIHDBUtility.Event_DB2VM(reader, vmevent, true);
                        vm.EventList.Add(vmevent);
                    }
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

        // POST: api/RecurEvent
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RecurEventViewModel vm)
        {
            if (vm == null)
                return BadRequest("No data is inputted");
            if (vm.HID <= 0)
                return BadRequest("Home not defined");

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
                return BadRequest("Name is a must!");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
            Int32 nDuplicatedID = -1;
            Int32 nNewID = -1;
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

            // Get the simulate items
            EventGenerationInputViewModel datInput = new EventGenerationInputViewModel();
            datInput.Name = vm.Name;
            datInput.RptType = vm.RptType;
            datInput.StartTimePoint = vm.StartTimePoint;
            datInput.EndTimePoint = vm.EndTimePoint;
            List<EventGenerationResultViewModel> listRsts = EventUtility.GenerateEvents(datInput);
            if (listRsts.Count <= 0)
                return BadRequest("Failed to generate recur items");

            SqlTransaction tran = null;
            try
            {
                queryString = @"SELECT [ID]
                            FROM [dbo].[t_event_recur] WHERE [Name] = N'" + vm.Name + "'";

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

                    tran = conn.BeginTransaction();
                    // Now go ahead for the creating
                    queryString = HIHDBUtility.Event_GetRecurEventInsertString();

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Transaction = tran;
                    HIHDBUtility.Event_BindRecurEventInsertParameters(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;

                    cmd.Dispose();
                    cmd = null;

                    // Go for the recur item creation
                    foreach(var gitem in listRsts)
                    {
                        queryString = HIHDBUtility.Event_GetNormalEventInsertString(false);
                        cmd = new SqlCommand(queryString, conn);
                        cmd.Transaction = tran;
                        var vmEvent = new EventViewModel();
                        vmEvent.EndTimePoint = gitem.EndTimePoint;
                        vmEvent.StartTimePoint = gitem.StartTimePoint;
                        vmEvent.RefRecurrID = nNewID;
                        vmEvent.IsPublic = vm.IsPublic;
                        vmEvent.Name = gitem.Name;
                        vmEvent.HID = vm.HID;
                        vmEvent.Content = vm.Content;
                        vmEvent.Assignee = vm.Assignee;
                        HIHDBUtility.Event_BindNormalEventInsertParameters(cmd, vmEvent, usrName);
                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;
                    }

                    tran.Commit();
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
                    tran.Dispose();
                    tran = null;
                }
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

        // PUT: api/RecurEvent/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // PACH: api/RecurEvent/5
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody]string value)
        {
            return BadRequest();
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

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            SqlTransaction tran = null;
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

                // Step 1. Delete events for recur
                tran = conn.BeginTransaction();

                queryString = HIHDBUtility.Event_GetNormalEventForRecurDeletionString();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
                HIHDBUtility.Event_BindNormalEventForRecurDeletionParameters(cmd, hid, id);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                // Step 2. Delete recur item
                queryString = HIHDBUtility.Event_GetRecurEventDeletionString();
                cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
                HIHDBUtility.Event_BindRecurEventDeletionParameters(cmd, hid, id);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                tran.Commit();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;

                if (tran != null)
                {
                    tran.Rollback();
                    tran.Dispose();
                    tran = null;
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

            if (bError)
                return StatusCode(500, strErrMsg);

            return Ok();
        }
    }
}
