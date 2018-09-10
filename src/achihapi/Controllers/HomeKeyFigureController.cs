using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/HomeKeyFigure")]
    [Authorize]
    public class HomeKeyFigureController : Controller
    {
        // GET: api/HomeKeyFigure
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid)
        {
            if (hid <= 0)
                return BadRequest("No home inputted");

            HomeKeyFigure figure = new HomeKeyFigure();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                String usrName = "";
                try
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                queryString = this.getQueryString(hid, usrName);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                // 1. Total assets and liability
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader.GetBoolean(0))
                        {
                            figure.TotalLiability = reader.GetDecimal(1);
                        }
                        else
                        {
                            figure.TotalAsset = reader.GetDecimal(1);
                        }
                    }
                }
                await reader.NextResultAsync();

                // 2. Total assets and liability
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader.GetBoolean(0))
                        {
                            figure.TotalLiabilityUnderMyName = reader.GetDecimal(1);
                        }
                        else
                        {
                            figure.TotalAssetUnderMyName = reader.GetDecimal(1);
                        }
                    }
                }
                await reader.NextResultAsync();

                // 3. Total unread message
                if (reader.HasRows)
                {
                    while(reader.Read())
                    {
                        figure.TotalUnreadMessage = reader.GetInt32(0);
                    }
                }
                await reader.NextResultAsync();

                // 4. My uncomplated event
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        figure.MyUnCompletedEvents = reader.GetInt32(0);
                    }
                }
                await reader.NextResultAsync();

                // 5. My completed events
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        figure.MyCompletedEvents = reader.GetInt32(0);
                    }
                }
                await reader.NextResultAsync();
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
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(figure, setting);
        }

        private string getQueryString(Int32 hid, String usrName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT [TRANTYPE_EXP], SUM(TRANAMOUNT) FROM [V_FIN_DOCUMENT_ITEM] WHERE [HID] = " + hid.ToString() + @" GROUP BY[TRANTYPE_EXP];");
            sb.Append(@"SELECT [TRANTYPE_EXP], SUM(TRANAMOUNT) FROM [V_FIN_DOCUMENT_ITEM] AS taba 
                    INNER JOIN t_fin_account AS tabb ON taba.ACCOUNTID = tabb.ID 
                    WHERE taba.[HID] = " + hid.ToString() + " AND tabb.OWNER = N'" + usrName + "' GROUP BY [TRANTYPE_EXP]; ");
            sb.Append(@"SELECT COUNT(*) AS MSGCOUNT FROM t_homemsg WHERE [READFLAG] = 0 AND [HID] = " + hid.ToString() + ";");
            sb.Append(@"SELECT COUNT(*) AS EVENTCOUNT FROM t_event WHERE [CompleteTime] IS NULL AND [HID] = " + hid.ToString() + " AND [Assignee] = N'" + usrName + "';");
            sb.Append(@"SELECT COUNT(*) AS EVENTCOUNT FROM t_event WHERE [CompleteTime] IS NOT NULL AND [HID] = " + hid.ToString() + " AND [Assignee] = N'" + usrName + "';");

            return sb.ToString();
        }
    }
}
