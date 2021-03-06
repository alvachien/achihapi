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
using achihapi.Utilities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceDocWithPlanExgRate")]
    public class FinanceDocWithPlanExgRateController : Controller
    {
        // GET: api/FinanceDocWithPlanExgRate
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, String tgtcurr)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

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

            List<FinanceDocPlanExgRateViewModel> listVMs = new List<FinanceDocPlanExgRateViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = @"SELECT [ID]
                                  ,[HID]
                                  ,[DOCTYPE]
                                  ,[TRANDATE]
                                  ,[TRANCURR]
                                  ,[DESP]
                                  ,[EXGRATE]
                                  ,[EXGRATE_PLAN]
                                  ,[EXGRATE_PLAN2]
                                  ,[TRANCURR2]
                                  ,[EXGRATE2]
                              FROM [dbo].[t_fin_document]
	                            WHERE [HID] = @hid
	                            AND ( ( [EXGRATE_PLAN] = 1 AND [TRANCURR] = @curr )
		                            OR ( [EXGRATE_PLAN2] = 1 AND [TRANCURR2] = @curr ) )";

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

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@hid", hid);
                    cmd.Parameters.AddWithValue("@curr", tgtcurr);
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceDocPlanExgRateViewModel avm = new FinanceDocPlanExgRateViewModel();
                            Int32 idx = 0;
                            avm.DocID = reader.GetInt32(idx++);
                            avm.HID = reader.GetInt32(idx++);
                            avm.DocType = reader.GetInt16(idx++);
                            avm.TranDate = reader.GetDateTime(idx++);
                            avm.TranCurr = reader.GetString(idx++);
                            avm.Desp = reader.GetString(idx++);
                            if (reader.IsDBNull(idx))
                                ++idx;
                            else
                                avm.ExgRate = reader.GetDecimal(idx++);
                            if (reader.IsDBNull(idx))
                                ++idx;
                            else
                                avm.ExgRate_Plan = reader.GetBoolean(idx++);
                            if (reader.IsDBNull(idx))
                                ++idx;
                            else
                                avm.ExgRate_Plan2 = reader.GetBoolean(idx++);
                            if (reader.IsDBNull(idx))
                                ++idx;
                            else
                                avm.TranCurr2 = reader.GetString(idx++);
                            if (reader.IsDBNull(idx))
                                ++idx;
                            else
                                avm.ExgRate2 = reader.GetDecimal(idx++);


                            listVMs.Add(avm);
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

            return new JsonResult(listVMs, setting);
        }

        // GET: api/FinanceDocWithPlanExgRate/5
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get([FromRoute]int id)
        {
            return BadRequest();
        }
        
        // POST: api/FinanceDocWithPlanExgRate
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceDocPlanExgRateForUpdViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (vm.HID <= 0)
                return BadRequest("No Home inputted");

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

            List<FinanceDocPlanExgRateViewModel> listVMs = new List<FinanceDocPlanExgRateViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            // Basic check
            if (String.IsNullOrEmpty(vm.TargetCurrency))
            {
                return BadRequest();
            }
            if (vm.ExchangeRate <= 0)
            {
                return BadRequest();
            }
            if (vm.DocIDs.Count <= 0)
            {
                return BadRequest();
            }

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    tran = conn.BeginTransaction();
                    foreach (var did in vm.DocIDs)
                    {
                        queryString = @"UPDATE [dbo].[t_fin_document]
                               SET [EXGRATE] = @EXGRATE
                                  ,[EXGRATE_PLAN] = @EXGRATE_PLAN
                                  ,[UPDATEDBY] = @UPDATEDBY
                                  ,[UPDATEDAT] = @UPDATEDAT
                             WHERE [ID] = @id
                               AND [TRANCURR] = @tcurr
                               AND [EXGRATE_PLAN] = @isplan";

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        cmd.Parameters.AddWithValue("@EXGRATE", vm.ExchangeRate);
                        cmd.Parameters.AddWithValue("@EXGRATE_PLAN", false);
                        cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                        cmd.Parameters.AddWithValue("@id", did);
                        cmd.Parameters.AddWithValue("@tcurr", vm.TargetCurrency);
                        cmd.Parameters.AddWithValue("@isplan", true);

                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        queryString = @"UPDATE [dbo].[t_fin_document]
                               SET [EXGRATE2] = @EXGRATE
                                  ,[EXGRATE_PLAN2] = @EXGRATE_PLAN
                                  ,[UPDATEDBY] = @UPDATEDBY
                                  ,[UPDATEDAT] = @UPDATEDAT
                             WHERE [ID] = @id
                               AND [TRANCURR2] = @tcurr
                               AND [EXGRATE_PLAN2] = @isplan";

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        cmd.Parameters.AddWithValue("@EXGRATE", vm.ExchangeRate);
                        cmd.Parameters.AddWithValue("@EXGRATE_PLAN", false);
                        cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                        cmd.Parameters.AddWithValue("@id", did);
                        cmd.Parameters.AddWithValue("@tcurr", vm.TargetCurrency);
                        cmd.Parameters.AddWithValue("@isplan", true);

                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;
                    }

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                if (tran != null)
                    tran.Rollback();

                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
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

            return Ok();
        }

        // PUT: api/FinanceDocWithPlanExgRate/5
        [HttpPut("{id}")]
        public IActionResult Put([FromRoute]int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }
    }
}
