using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceReportOrderController : Controller
    {
        // GET: api/financereportorder
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean? incInv = null)
        {
            if (hid <= 0)
                return BadRequest("No HID inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            List<FinanceReportOrderViewModel> listVm = new List<FinanceReportOrderViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [ORDERID]
                          ,[ORDERNAME]
                          ,[ORDERVALID_FROM]
                          ,[ORDERVALID_TO]
                          ,[debit_balance]
                          ,[credit_balance]
                          ,[balance]
                      FROM [dbo].[v_fin_report_order] WHERE [HID] = " + hid.ToString();
                if (!incInv.HasValue || !incInv.Value)
                    queryString += " AND [ORDERVALID_FROM] <= GETDATE() AND [ORDERVALID_TO] >= GETDATE()";

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
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceReportOrderViewModel avm = new FinanceReportOrderViewModel
                        {
                            OrderID = reader.GetInt32(0),
                            OrderName = reader.GetString(1),
                            ValidFrom = reader.GetDateTime(2),
                            ValidTo = reader.GetDateTime(3)
                        };

                        Int32 clnidx = 4;
                        if (reader.IsDBNull(clnidx))
                            avm.DebitBalance = 0;
                        else
                            avm.DebitBalance = Math.Round(reader.GetDecimal(clnidx), 2);

                        clnidx++;
                        if (reader.IsDBNull(clnidx))
                            avm.CreditBalance = 0;
                        else
                            avm.CreditBalance = Math.Round(reader.GetDecimal(clnidx), 2);

                        clnidx++;
                        if (reader.IsDBNull(clnidx))
                            avm.Balance = 0;
                        else
                            avm.Balance = Math.Round(reader.GetDecimal(clnidx), 2);

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
