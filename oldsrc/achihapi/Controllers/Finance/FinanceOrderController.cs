using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.Utilities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class FinanceOrderController : Controller
    {
        private IMemoryCache _cache;
        public FinanceOrderController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/financeorder
        [HttpGet]
        [Authorize]
        [Produces(typeof(List<FinanceOrderViewModel>))]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean? incInv = null)
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

            List<FinanceOrderViewModel> listVMs = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinOrderList, hid, incInv);
                if (_cache.TryGetValue<List<FinanceOrderViewModel>>(cacheKey, out listVMs))
                {
                    // Do nothing
                }
                else
                {
                    listVMs = new List<FinanceOrderViewModel>();

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

                        queryString = this.getListQueryString(hid);
                        if (!incInv.HasValue || !incInv.Value)
                            queryString += " AND [VALID_FROM] <= GETDATE() AND [VALID_TO] >= GETDATE()";
                        cmd = new SqlCommand(queryString, conn);
                        reader = await cmd.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceOrderViewModel vm = new FinanceOrderViewModel();
                                this.onListDB2VM(reader, vm);
                                listVMs.Add(vm);
                            }
                        }
                    }

                    _cache.Set<List<FinanceOrderViewModel>>(cacheKey, listVMs, TimeSpan.FromMinutes(20));
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

        // GET api/financeorder/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get([FromRoute]int id, [FromQuery]Int32 hid = 0)
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

            FinanceOrderViewModel vm = new FinanceOrderViewModel();

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                //queryString = this.getQueryString(false, null, null, id);
                queryString = this.getItemQueryString(id);

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
                        //BaseListViewModel<FinanceOrderViewModel> listVM = new BaseListViewModel<FinanceOrderViewModel>();
                        //onDB2VM(reader, listVM);
                        //if (listVM.ContentList.Count == 1)
                        //{
                        //    vm = listVM.ContentList[0];
                        //}
                        //else
                        //{
                        //    throw new Exception("Failed to read db entry successfully!");
                        //}

                        this.onItemDB2VM(reader, vm);
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
            ;
            return new JsonResult(vm, setting);
        }

        // POST api/financeorder
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            if (vm.HID <= 0)
                return BadRequest("No Home Inputted");

            // Check on name
            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
                return BadRequest("Name is a must!");

            // Check the s.rule
            if (vm.SRuleList.Count <= 0)
                return BadRequest("No rule has been assigned yet");
            Int32 nPer = 0;
            foreach(FinanceOrderSRuleUIViewModel svm in vm.SRuleList)
            {
                if (svm.RuleID <= 0)
                    return BadRequest("Invalid rule ID");

                if (svm.Precent <= 0)
                    return BadRequest("Percentage less than zero!");

                nPer += svm.Precent;
            }
            if (nPer != 100 )
                return BadRequest("Total percentage shall sum up to 100");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_order] WHERE [Name] = N'" + vm.Name + "'";

                using(conn = new SqlConnection(Startup.DBConnectionString))
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

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Int32 nDuplicatedID = -1;
                        while (reader.Read())
                        {
                            nDuplicatedID = reader.GetInt32(0);
                            break;
                        }
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Order with same name already exists: " + nDuplicatedID.ToString());
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        // Now go ahead for the creating
                        tran = conn.BeginTransaction();

                        queryString = HIHDBUtility.GetFinOrderInsertString();

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinOrderInsertParameter(cmd, vm, usrName);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewID = (Int32)idparam.Value;

                        cmd.Dispose();
                        cmd = null;

                        // Then, creating the srules
                        foreach (FinanceOrderSRuleUIViewModel suivm in vm.SRuleList)
                        {
                            queryString = HIHDBUtility.GetFinOrderSRuleInsertString();

                            SqlCommand cmd2 = new SqlCommand(queryString, conn)
                            {
                                Transaction = tran
                            };
                            HIHDBUtility.BindFinOrderSRuleInsertParameter(cmd2, suivm, nNewID);
                            await cmd2.ExecuteNonQueryAsync();

                            cmd2.Dispose();
                            cmd2 = null;
                        }

                        tran.Commit();

                        // Update the buffer
                        // Order List
                        try
                        {
                            // Include invalid
                            var cacheKey = String.Format(CacheKeys.FinOrderList, vm.HID, true);
                            this._cache.Remove(cacheKey);
                            // Exclude invalid
                            var cacheKey2 = String.Format(CacheKeys.FinOrderList, vm.HID, false);
                            this._cache.Remove(cacheKey2);
                        }
                        catch (Exception)
                        {
                            // Do nothing here.
                        }
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
                        return BadRequest(strErrMsg);
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PUT api/financeorder/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put([FromRoute]int id, [FromBody]FinanceOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            if (vm == null)
                return BadRequest("No data is inputted");
            if (vm.HID <= 0)
                return BadRequest("No Home Inputted");
            if (vm.ID != id)
                return BadRequest("Data inconsistent!");

            // Check on name
            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
                return BadRequest("Name is a must!");

            // Check the s.rule
            if (vm.SRuleList.Count <= 0)
                return BadRequest("No rule has been assigned yet");
            Int32 nPer = 0;
            foreach (FinanceOrderSRuleUIViewModel svm in vm.SRuleList)
            {
                if (svm.RuleID <= 0)
                    return BadRequest("Invalid rule ID");

                if (svm.Precent <= 0)
                    return BadRequest("Percentage less than zero!");

                nPer += svm.Precent;
            }
            if (nPer != 100)
                return BadRequest("Total percentage shall sum up to 100");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_order] WHERE [Name] = N'" + vm.Name + "' AND [ID] <> " + id.ToString();

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

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Int32 nDuplicatedID = -1;
                        while (reader.Read())
                        {
                            nDuplicatedID = reader.GetInt32(0);
                            break;
                        }

                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Order with same name already exists: " + nDuplicatedID.ToString());
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        // Now go ahead for the creating
                        tran = conn.BeginTransaction();

                        queryString = HIHDBUtility.GetFinOrderUpdateString();

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinOrderUpdateParameter(cmd, vm, usrName);

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;

                        // Then, delete existing srules
                        queryString = HIHDBUtility.GetFinOrderSRuleDeleteString();
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinOrderSRuleDeleteParameter(cmd, id);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        // Then, creating the srules
                        foreach (FinanceOrderSRuleUIViewModel suivm in vm.SRuleList)
                        {
                            queryString = HIHDBUtility.GetFinOrderSRuleInsertString();
                            SqlCommand cmd2 = new SqlCommand(queryString, conn)
                            {
                                Transaction = tran
                            };
                            HIHDBUtility.BindFinOrderSRuleInsertParameter(cmd2, suivm, vm.ID);
                            await cmd2.ExecuteNonQueryAsync();

                            cmd2.Dispose();
                            cmd2 = null;
                        }

                        tran.Commit();

                        // Order List
                        try
                        {
                            // Include invalid
                            var cacheKey = String.Format(CacheKeys.FinOrderList, vm.HID, true);
                            this._cache.Remove(cacheKey);
                            // Exclude invalid
                            var cacheKey2 = String.Format(CacheKeys.FinOrderList, vm.HID, false);
                            this._cache.Remove(cacheKey2);
                        }
                        catch (Exception)
                        {
                            // Do nothing here.
                        }
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

            return new JsonResult(vm, setting);
        }

        // DELETE api/financeorder/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute]int id, [FromQuery] Int32 hid = 0)
        {
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

            if (hid <= 0)
                return BadRequest("No Home Inputted");

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
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

                    // Check whether the order is used already
                    queryString = @"SELECT ID FROM [t_fin_order] WHERE EXISTS (SELECT * FROM [t_fin_document_item] WHERE ORDERID = t_fin_order.ID) AND ID = " + id.ToString();
                    cmd = new SqlCommand(queryString, conn);
                    reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Order still in use!");
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        // Now go ahead for the deletion
                        tran = conn.BeginTransaction();

                        queryString = @"DELETE FROM [dbo].[t_fin_order] WHERE [ID] = @id";

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        cmd.Parameters.AddWithValue("@id", id);
                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        // Then, delete srules
                        queryString = @"DELETE FROM [dbo].[t_fin_order_srule] WHERE [ORDID] = @id";
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        tran.Commit();

                        // Order List
                        try
                        {
                            // Include invalid
                            var cacheKey = String.Format(CacheKeys.FinOrderList, hid, true);
                            this._cache.Remove(cacheKey);
                            // Exclude invalid
                            var cacheKey2 = String.Format(CacheKeys.FinOrderList, hid, false);
                            this._cache.Remove(cacheKey2);
                        }
                        catch (Exception)
                        {
                            // Do nothing here.
                        }
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

            return Ok();
        }

        #region Implmented methods
        private string getListQueryString(Int32 hid)
        {
            return @"SELECT [ID]
                      ,[HID]
                      ,[NAME]
                      ,[VALID_FROM]
                      ,[VALID_TO]
                      ,[COMMENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.ToString();
        }
        private void onListDB2VM(SqlDataReader reader, FinanceOrderViewModel vm)
        {
            Int32 idx = 0;

            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.Name = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.ValidFrom = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ValidTo = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Comment = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
        }

        private string getItemQueryString(Int32 nID)
        {
            String strSQL = @"SELECT [ID]
                              ,[HID]
                              ,[NAME]
                              ,[VALID_FROM]
                              ,[VALID_TO]
                              ,[COMMENT]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [t_fin_order] WHERE [ID] = " + nID.ToString() + "; ";
            strSQL += @"SELECT [ID] AS [ORDID]
                              ,[RULEID]
                              ,[CONTROLCENTERID]
                              ,[CONTROLCENTERNAME]
                              ,[PRECENT]
                              ,[COMMENT]
                          FROM [v_fin_order_srule] WHERE [ID] = " + nID.ToString();

            return strSQL;
        }
        private void onItemDB2VM(SqlDataReader reader, FinanceOrderViewModel vm)
        {
            Int32 idx = 0;

            if (reader.HasRows)
            {
                idx = 0;
                while (reader.Read())
                {
                    vm.ID = reader.GetInt32(idx++);
                    vm.HID = reader.GetInt32(idx++);
                    vm.Name = reader.GetString(idx++);
                    if (!reader.IsDBNull(idx))
                        vm.ValidFrom = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ValidTo = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.Comment = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.CreatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.CreatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.UpdatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.UpdatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                }
            }
            reader.NextResult();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    FinanceOrderSRuleUIViewModel srvm = new FinanceOrderSRuleUIViewModel();

                    idx = 0;
                    srvm.OrdID = reader.GetInt32(idx++);
                    srvm.RuleID = reader.GetInt32(idx++);
                    srvm.ControlCenterID = reader.GetInt32(idx++);
                    if (!reader.IsDBNull(idx))
                        srvm.ControlCenterName = reader.GetString(idx++);
                    else
                        ++idx;
                    srvm.Precent = reader.GetInt32(idx++);
                    if (!reader.IsDBNull(idx))
                    {
                        srvm.Comment = reader.GetString(idx++);
                    }
                    else
                        ++idx;

                    vm.SRuleList.Add(srvm);
                }
            }
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.Value.ToString() + "; ";
            }

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WITH ZOrder_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.Value.ToString() 
                        + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
                strSQL += @" SELECT [ZOrder_CTE].[ID] ";
            }
            else
            {
                strSQL += @" SELECT [ID] ";
            }

            strSQL += @" ,[NAME]
                      ,[VALID_FROM]
                      ,[VALID_TO]
                      ,[COMMENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                      ,[RULEID]
                      ,[CONTROLCENTERID]
                      ,[CONTROLCENTERNAME]
                      ,[PRECENT] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += " FROM [ZOrder_CTE] LEFT OUTER JOIN [v_fin_order_srule] ON [ZOrder_CTE].[ID] = [v_fin_order_srule].[ID] ORDER BY [ID] ";
            }                
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " FROM [v_fin_order_srule] WHERE [ID] = " + nSearchID.Value.ToString();
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, BaseListViewModel<FinanceOrderViewModel> listVMs)
        {
            Int32 nOrderID = -1;
            while (reader.Read())
            {
                Int32 idx = 0;
                Int32 nCurrentID = reader.GetInt32(idx++);
                FinanceOrderViewModel vm = null;
                if (nOrderID != nCurrentID)
                {
                    nOrderID = nCurrentID;
                    vm = new FinanceOrderViewModel
                    {
                        ID = nCurrentID,
                        Name = reader.GetString(idx++)
                    };
                    if (!reader.IsDBNull(idx))
                        vm.ValidFrom = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ValidTo = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.Comment = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.CreatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.CreatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.UpdatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.UpdatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                }
                else
                {
                    foreach (FinanceOrderViewModel ovm in listVMs)
                    {
                        if (ovm.ID == nCurrentID)
                        {
                            vm = ovm;
                            break;
                        }
                    }
                }

                idx = 9;
                FinanceOrderSRuleUIViewModel srvm = new FinanceOrderSRuleUIViewModel
                {
                    RuleID = reader.GetInt32(idx++),
                    ControlCenterID = reader.GetInt32(idx++)
                };
                if (!reader.IsDBNull(idx))
                    srvm.ControlCenterName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    srvm.Precent = reader.GetInt32(idx++);
                else
                    ++idx;
                vm.SRuleList.Add(srvm);

                if (nOrderID != nCurrentID)
                {
                    listVMs.Add(vm);
                }
            }
        }
        #endregion
    }
}
