using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using achihapi.Utilities;
using System.Data.SqlClient;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceReportTrendEx")]
    [Authorize]
    public class FinanceReportTrendExController : Controller
    {
        // GET: api/FinanceReportTrendEx
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, DateTime? dtbgn = null, DateTime? dtend = null,
            Boolean exctran = false, FinanceReportTrendExType trendtype = FinanceReportTrendExType.Daily)
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

            List<FinanceReportTrendExViewModel> listVm = new List<FinanceReportTrendExViewModel>();
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

                switch(trendtype)
                {
                    case FinanceReportTrendExType.Daily:
                        {
                            queryString = @"SELECT TRANDATE, EXPENSE, SUM(TRANAMOUNT) AS TOTALAMTS 
                                FROM v_fin_report_trantype WHERE [HID] = @hid ";
                            // Exclude transfer
                            if (exctran)
                                queryString += " AND [TRANTYPE] !=  " + FinanceTranTypeViewModel.TranType_TransferIn.ToString() + " AND [TRANTYPE] != " + FinanceTranTypeViewModel.TranType_TransferOut.ToString();
                            if (dtbgn.HasValue)
                                queryString += " AND [TRANDATE] >= @dtbgn ";
                            if (dtend.HasValue)
                                queryString += " AND [TRANDATE] <= @dtend ";
                            queryString += @" GROUP BY TRANDATE, EXPENSE";
                        }
                        break;

                    case FinanceReportTrendExType.Weekly:
                        {
                            queryString = @"SELECT YEAR(TRANDATE) AS TRANYEAR, DATENAME(week, TRANDATE) AS TRANWEEK, EXPENSE, SUM(TRANAMOUNT) AS TOTALAMTS 
                                FROM v_fin_report_trantype WHERE [HID] = @hid ";
                            // Exclude transfer
                            if (exctran)
                                queryString += " AND [TRANTYPE] !=  " + FinanceTranTypeViewModel.TranType_TransferIn.ToString() + " AND [TRANTYPE] != " + FinanceTranTypeViewModel.TranType_TransferOut.ToString();
                            if (dtbgn.HasValue)
                                queryString += " AND [TRANDATE] >= @dtbgn ";
                            if (dtend.HasValue)
                                queryString += " AND [TRANDATE] <= @dtend ";
                            queryString += @" GROUP BY YEAR(TRANDATE), DATENAME(week, TRANDATE), EXPENSE";
                        }
                        break;

                    case FinanceReportTrendExType.Monthly:
                        {
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
                        }
                        break;

                    default:
                        break;
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
                        switch (trendtype)
                        {
                            case FinanceReportTrendExType.Daily:
                                FinanceReportTrendExViewModel dvm = new FinanceReportTrendExViewModel
                                {
                                    TranDate = reader.GetDateTime(0),
                                    Expense = reader.GetBoolean(1),
                                    TranAmount = reader.GetDecimal(2)
                                };
                                listVm.Add(dvm);
                                break;

                            case FinanceReportTrendExType.Weekly:
                                FinanceReportTrendExViewModel wvm = new FinanceReportTrendExViewModel
                                {
                                    TranYear = reader.GetInt32(0),
                                    TranWeek = Int32.Parse(reader.GetString(1)),
                                    Expense = reader.GetBoolean(2),
                                    TranAmount = reader.GetDecimal(3)
                                };
                                listVm.Add(wvm);
                                break;

                            case FinanceReportTrendExType.Monthly:
                                FinanceReportTrendExViewModel mvm = new FinanceReportTrendExViewModel
                                {
                                    TranYear = reader.GetInt32(0),
                                    TranMonth = reader.GetInt32(1),
                                    Expense = reader.GetBoolean(2),
                                    TranAmount = reader.GetDecimal(3)
                                };
                                listVm.Add(mvm);
                                break;

                            default:
                                break;
                        }
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

            return new JsonResult(listVm, setting);
        }
    }
}
