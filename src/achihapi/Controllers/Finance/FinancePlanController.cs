using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinancePlanController : ControllerBase
    {
        private IMemoryCache _cache;
        public FinancePlanController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/FinancePlan
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");

            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (String.IsNullOrEmpty(usrName))
                return BadRequest("No user found");

            List<FinancePlanViewModel> listVm = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinPlanList, hid);
                if (_cache.TryGetValue<List<FinancePlanViewModel>>(cacheKey, out listVm))
                {
                    // Do nothing
                }
                else
                {
                    listVm = new List<FinancePlanViewModel>();

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

                        queryString = HIHDBUtility.GetFinPlanSelectionString() + " WHERE [HID] = " + hid.ToString();

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            FinancePlanViewModel vm = new FinancePlanViewModel();
                            HIHDBUtility.FinPlan_DB2VM(reader, vm);
                            listVm.Add(vm);
                        }
                    }

                    _cache.Set<List<FinancePlanViewModel>>(cacheKey, listVm, TimeSpan.FromMinutes(20));
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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

        // GET: api/FinancePlan/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute]int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0 || id <= 0)
                return BadRequest("Invalid ID or HID inputted");

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

            FinancePlanViewModel vm = new FinancePlanViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = HIHDBUtility.GetFinPlanSelectionString() + " WHERE [ID] = " + id.ToString() + " AND [HID] = " + hid.ToString();

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
                    reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HIHDBUtility.FinPlan_DB2VM(reader, vm);
                            break; // Should only one result!!!
                        }
                    }
                    else
                    {
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
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

            return new JsonResult(vm, setting);
        }

        // POST: api/FinancePlan
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]FinancePlanViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Perform the checks
            if (vm.HID <= 0)
                return BadRequest("No HID inputted!");
            if (vm == null
                || vm.StartDate > vm.TargetDate
                || (vm.PlanType == FinancePlanTypeEnum.Account && (!vm.AccountID.HasValue || vm.AccountID.Value <= 0))
                || (vm.PlanType == FinancePlanTypeEnum.AccountCategory && (!vm.AccountCategoryID.HasValue || vm.AccountCategoryID.Value <= 0))
                || (vm.PlanType == FinancePlanTypeEnum.ControlCenter && (!vm.ControlCenterID.HasValue || vm.ControlCenterID.Value <= 0))
                || (vm.PlanType == FinancePlanTypeEnum.TranType && (!vm.TranTypeID.HasValue || vm.TranTypeID.Value <= 0))
                )
            {
                return BadRequest("Invalid data to create");
            }

            String usrName = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewPlanID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check HID assignment
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    if (vm.PlanType == FinancePlanTypeEnum.Account)
                    {
                        // Check the account
                        queryString = @"SELECT [ID], [Status]
                            FROM [dbo].[t_fin_account] WHERE [ID] = " + vm.AccountID.Value.ToString() + " AND [HID] = " + vm.HID.ToString();

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw new Exception("Account doesnot exist: " + vm.AccountID.Value.ToString());
                        }
                        else
                        {
                            // Check the status
                            await reader.ReadAsync();
                            if (!reader.IsDBNull(1))
                            {
                                FinanceAccountStatus nAccountStatus = (FinanceAccountStatus)reader.GetByte(1);
                                if (nAccountStatus == FinanceAccountStatus.Frozen
                                    || nAccountStatus == FinanceAccountStatus.Closed)
                                {
                                    errorCode = HttpStatusCode.BadRequest;
                                    throw new Exception("Account status is invalid: " + vm.AccountID.Value.ToString());
                                }
                            }
                        }
                        reader.Dispose();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;

                        // Now create the DB entry
                        // Begin the transaction
                        tran = conn.BeginTransaction();

                        // Now go ahead for the creating
                        queryString = HIHDBUtility.GetFinPlanInsertString();

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };

                        vm.CreatedBy = usrName;
                        vm.CreatedAt = DateTime.Now;
                        HIHDBUtility.BindFinPlanInsertParameter(cmd, vm);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewPlanID = (Int32)idparam.Value;

                        // Now commit it!
                        tran.Commit();

                        // Update the buffer
                        try
                        {
                            var cacheKey = String.Format(CacheKeys.FinPlanList, vm.HID);
                            this._cache.Remove(cacheKey);
                        }
                        catch (Exception)
                        {
                            // Do nothing here.
                        }
                    }
                    else if (vm.PlanType == FinancePlanTypeEnum.AccountCategory)
                    {
                    }
                    else if (vm.PlanType == FinancePlanTypeEnum.ControlCenter)
                    {
                    }
                    else if (vm.PlanType == FinancePlanTypeEnum.TranType)
                    {
                    }


                    // Update the buffer
                    // Account List
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinPlanList, vm.HID, null);
                        this._cache.Remove(cacheKey);
                    }
                    catch (Exception)
                    {
                        // Do nothing here.
                    }
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
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            vm.ID = nNewPlanID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // PUT: api/FinancePlan/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]FinancePlanViewModel vm)
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
