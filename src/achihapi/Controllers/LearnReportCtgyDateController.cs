using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Data.SqlClient;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LearnReportCtgyDate")]
    public class LearnReportCtgyDateController : Controller
    {
        // GET: api/LearnReportCtgyDate
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

            List<LearnReportCtgyDateViewModel> listVm = new List<LearnReportCtgyDateViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [HID]
                              ,[CATEGORY]
                              ,[LEARNDATE]
                              ,[LEARNCOUNT]
                      FROM [dbo].[v_lrn_ctgylrndate] WHERE [HID] = @hid ";
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
                        LearnReportCtgyDateViewModel avm = new LearnReportCtgyDateViewModel
                        {
                            HID = reader.GetInt32(0),
                            Category = reader.GetInt32(1),
                            LearnDate = reader.GetDateTime(2),
                            LearnCount = reader.GetInt32(3)
                        };
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
                conn.Close();
                conn.Dispose();
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
