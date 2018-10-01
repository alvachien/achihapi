using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using achihapi.Utilities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceReportBSController : Controller
    {
        private IMemoryCache _cache;

        public FinanceReportBSController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/financereportbs
        [HttpGet]
        [Authorize]
        [Produces(typeof(List<FinanceReportBSViewModel>))]
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

            List<FinanceReportBSViewModel> listVm = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinReportBS, hid);

                if (_cache.TryGetValue<List<FinanceReportBSViewModel>>(cacheKey, out listVm))
                {
                    // Do nothing
                }
                else
                {
                    listVm = new List<FinanceReportBSViewModel>();

                    queryString = @"SELECT [accountid]
                          ,[ACCOUNTNAME]
                          ,[ACCOUNTCTGYID]
                          ,[ACCOUNTCTGYNAME]
                          ,[debit_balance]
                          ,[credit_balance]
                          ,[balance]
                      FROM [dbo].[v_fin_report_bs] WHERE [HID] = " + hid.ToString();

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
                                FinanceReportBSViewModel avm = new FinanceReportBSViewModel
                                {
                                    AccountID = reader.GetInt32(0),
                                    AccountName = reader.GetString(1),
                                    AccountCategoryID = reader.GetInt32(2),
                                    AccountCategoryName = reader.GetString(3)
                                };
                                if (reader.IsDBNull(4))
                                    avm.DebitBalance = 0;
                                else
                                    avm.DebitBalance = Math.Round(reader.GetDecimal(4), 2);
                                if (reader.IsDBNull(5))
                                    avm.CreditBalance = 0;
                                else
                                    avm.CreditBalance = Math.Round(reader.GetDecimal(5), 2);
                                if (reader.IsDBNull(6))
                                    avm.Balance = 0;
                                else
                                    avm.Balance = Math.Round(reader.GetDecimal(6), 2);
                                listVm.Add(avm);
                            }
                        }
                    }

                    this._cache.Set<List<FinanceReportBSViewModel>>(cacheKey, listVm, TimeSpan.FromSeconds(600));
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
