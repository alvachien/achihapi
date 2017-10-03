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

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceReportTranType")]
    public class FinanceReportTranTypeController : Controller
    {
        // GET: api/FinanceReportTranType
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            List<FinanceReportTranTypeViewModel> listVm = new List<FinanceReportTranTypeViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            if (hid <= 0)
                return BadRequest("No HID inputted");

            try
            {
                queryString = @"SELECT [HID]
                              ,[TRANDATE]
                              ,[TRANTYPE]
                              ,[NAME]
                              ,[EXPENSE]
                              ,[tranamount]
                      FROM [dbo].[v_fin_report_trantype] WHERE [HID] = @hid ";
                if (dtbgn.HasValue)
                    queryString += " AND [TRANDATE] >= @dtbgn";
                if (dtend.HasValue)
                    queryString += " AND [TRANDATE] <= @dtend";

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
                        FinanceReportTranTypeViewModel avm = new FinanceReportTranTypeViewModel
                        {
                            TranDate = reader.GetDateTime(1),
                            TranType = reader.GetInt32(2)
                        };
                        if (reader.IsDBNull(3))
                            avm.Name = String.Empty;
                        else
                            avm.Name = reader.GetString(3);
                        if (reader.IsDBNull(4))
                            avm.ExpenseFlag = false;
                        else
                            avm.ExpenseFlag = reader.GetBoolean(4);
                        if (reader.IsDBNull(5))
                            avm.TranAmount = 0;
                        else
                            avm.TranAmount = Math.Round(reader.GetDecimal(5), 2);
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