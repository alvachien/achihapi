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
    [Route("api/RecurEventSearch")]
    [Authorize]
    public class RecurEventSearchController : Controller
    {
        // GET: api/RecurEventSearch
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
                            RecurEventViewModel vm = new RecurEventViewModel();
                            HIHDBUtility.Event_RecurDB2VM(reader, vm, true);
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
