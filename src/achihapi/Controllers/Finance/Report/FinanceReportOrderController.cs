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
using achihapi.Utilities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceReportOrderController : Controller
    {
        private IMemoryCache _cache;

        public FinanceReportOrderController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/financereportorder
        [HttpGet]
        [Authorize]
        [Produces(typeof(List<FinanceReportOrderViewModel>))]
        public async Task<IActionResult> Get([FromQuery]Int32 hid)
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

            List<FinanceReportOrderViewModel> listVm = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinReportOrder, hid);

                if (_cache.TryGetValue<List<FinanceReportOrderViewModel>>(cacheKey, out listVm))
                {
                    // Do nothing
                }
                else
                {
                    listVm = new List<FinanceReportOrderViewModel>();

                    queryString = @"SELECT [ORDERID]
                          ,[ORDERNAME]
                          ,[ORDERVALID_FROM]
                          ,[ORDERVALID_TO]
                          ,[debit_balance]
                          ,[credit_balance]
                          ,[balance]
                      FROM [dbo].[v_fin_report_order] WHERE [HID] = " + hid.ToString();
                    //if (!incInv.HasValue || !incInv.Value)
                    //    queryString += " AND [ORDERVALID_FROM] <= GETDATE() AND [ORDERVALID_TO] >= GETDATE()";

                    using (conn = new SqlConnection(Startup.DBConnectionString))
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

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();
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

                    _cache.Set<List<FinanceReportOrderViewModel>>(cacheKey, listVm, TimeSpan.FromMinutes(20));
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
                        return BadRequest(strErrMsg);
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
