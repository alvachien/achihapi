using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LearnReportUserDate")]
    public class LearnReportUserDateController : Controller
    {
        // GET: api/values
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            if (hid <= 0)
                return BadRequest("No HID inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            List<LearnReportUserDateViewModel> listVm = new List<LearnReportUserDateViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [HID]
                                  ,[USERID]
                                  ,[DISPLAYAS]
                                  ,[LEARNDATE]
                                  ,[LEARNCOUNT]
                              FROM [dbo].[v_lrn_usrlrndate] WHERE [HID] = @hid ";
                if (dtbgn.HasValue)
                    queryString += " AND [LEARNDATE] >= @dtbgn";
                if (dtend.HasValue)
                    queryString += " AND [LEARNDATE] <= @dtend";

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
                cmd.Parameters.AddWithValue("@hid", hid);
                if (dtbgn.HasValue)
                    cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                if (dtbgn.HasValue)
                    cmd.Parameters.AddWithValue("@dtend", dtend.Value);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnReportUserDateViewModel avm = new LearnReportUserDateViewModel
                        {
                            HID = reader.GetInt32(0),
                            UserID = reader.GetString(1),
                            LearnDate = reader.GetDateTime(3),
                            LearnCount = reader.GetInt32(4)
                        };
                        if (!reader.IsDBNull(2))
                            avm.DisplayAs = reader.GetString(2);
                        listVm.Add(avm);
                    }
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
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(listVm, setting);
        }
    }
}
