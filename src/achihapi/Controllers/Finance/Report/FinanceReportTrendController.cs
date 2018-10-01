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
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceReportTrend")]
    public class FinanceReportTrendController : Controller
    {
        // GET: api/FinanceReportTrend
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean exctran = false, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            if (hid <= 0)
                return BadRequest("No HID inputted");

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

            List<FinanceReportTrendBaseViewModel> listVm = new List<FinanceReportTrendBaseViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    queryString = @"SELECT YEAR(TRANDATE) AS TRANYEAR, MONTH(TRANDATE) AS TRANMONTH, EXPENSE, SUM(TRANAMOUNT) AS TOTALAMTS 
                    FROM v_fin_report_trantype WHERE [HID] = @hid ";
                    // Exclude transfer
                    if (exctran)
                        queryString += " AND [TRANTYPE] !=  " + FinanceTranTypeViewModel.TranType_TransferIn.ToString() + " AND [TRANTYPE] != " + FinanceTranTypeViewModel.TranType_TransferOut.ToString();
                    if (dtbgn.HasValue)
                        queryString += " AND [TRANDATE] >= @dtbgn ";
                    if (dtend.HasValue)
                        queryString += " AND [TRANDATE] <= @dtend ";
                    queryString += @" GROUP BY YEAR(TRANDATE), MONTH(TRANDATE), EXPENSE";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@hid", hid);
                    if (dtbgn.HasValue)
                        cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                    if (dtbgn.HasValue)
                        cmd.Parameters.AddWithValue("@dtend", dtend.Value);

                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceReportTrendBaseViewModel avm = new FinanceReportTrendBaseViewModel
                            {
                                Year = reader.GetInt32(0),
                                Month = reader.GetInt32(1),
                                Expense = reader.GetBoolean(2),
                                TranAmount = reader.GetDecimal(3)
                            };
                            listVm.Add(avm);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(listVm, setting);
        }
    }
}
