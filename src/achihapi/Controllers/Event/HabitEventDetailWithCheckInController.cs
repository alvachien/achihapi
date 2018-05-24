using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;
using achihapi.ViewModels;
using achihapi.Utilities;
using System.Data;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/HabitEventDetailWithCheckIn")]
    [Authorize]
    public class HabitEventDetailWithCheckInController : Controller
    {
        // GET: api/RecurEventSearch
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, DateTime dtbgn, DateTime dtend)
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

            List<EventHabitDetailWithCheckInViewModel> listVm = new List<EventHabitDetailWithCheckInViewModel>();
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

                queryString = HIHDBUtility.Event_GetHabitDetailWithCheckInSearchString();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                HIHDBUtility.Event_BindHabitDetailWithCheckInSearchParameter(cmd, hid, dtbgn, dtend);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    EventHabitDetailWithCheckInViewModel vm = new EventHabitDetailWithCheckInViewModel();
                    HIHDBUtility.Event_HabitDetailWithCheckInDB2VM(reader, vm);
                    listVm.Add(vm);
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
    }
}
