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
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

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
                cmd.Parameters.AddWithValue("@curr", tgtcurr);
                SqlDataReader reader = cmd.ExecuteReader();

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
            return new JsonResult(listVMs, setting);
        }

        // GET: api/FinanceDocWithPlanExgRate/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            return BadRequest();
        }
        
        // POST: api/FinanceDocWithPlanExgRate
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceDocPlanExgRateForUpdViewModel vm)
        {
            if (vm == null)
                return BadRequest("No Data inputted");
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
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

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

            SqlTransaction tran = null;
            try
            {
                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
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

                    SqlCommand cmd = new SqlCommand(queryString, conn)
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
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                if (tran != null)
                    tran.Rollback();

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

            return Ok();
        }

        // PUT: api/FinanceDocWithPlanExgRate/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}
