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
    public class HomeDefController : Controller
    {
        private IMemoryCache _cache;

        /**
         * Create
         *      When a user create a Home defintion, it will be the host automatically.
         *          Insert an entry in t_homedef, Insert an entry in t_homemem;
         *          
         * Assign an user
         *      The host can add an user into home definition; Todo: add t_homemessage?
         *          Insert an entry in t_homemem;
         * 
         * Remove an user
         *      The host can remove an user from the home definition;
         *          Delete an entry from t_homemem;
         * 
         * Handover the host
         *      The host can name another user in the home to be the host;
         *          Change the t_homedef directly;
         * 
         * Login integration
         *      1) When an user is login but no home assigned, prompt the user to create the home definition;
         *      2) When an user is login, fetch all home definitions relevant (via t_homemember), and let the user choose one;
         * 
         */

        public HomeDefController(IMemoryCache cache)
        {
            this._cache = cache;
        }

        // GET: api/homedef
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<HomeDefViewModel> listVm = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";

            try
            {
                String scopeFilter = String.Empty;

                String usrName = "";
                try
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;

                    // Disabled scope check just make it work, 2017.10.1
                    scopeFilter = usrName;

                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                    //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                    //if (String.IsNullOrEmpty(scopeFilter))
                    //    scopeFilter = usrName;
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                if (String.IsNullOrEmpty(scopeFilter))
                {
                    return BadRequest();
                }

                // Cache key
                var cacheKey = String.Format(CacheKeys.HomeDefList, scopeFilter, top, skip);
                if (_cache.TryGetValue<BaseListViewModel<HomeDefViewModel>>(cacheKey, out listVm))
                {
                    // Do nothing
                }
                else
                {
                    listVm = new BaseListViewModel<HomeDefViewModel>();
                    queryString = this.getQueryString(true, top, skip, null, scopeFilter);
                    using (conn = new SqlConnection(Startup.DBConnectionString))
                    {
                        await conn.OpenAsync();

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                listVm.TotalCount = reader.GetInt32(0);
                                break;
                            }
                        }
                        reader.NextResult();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                HomeDefViewModel vm = new HomeDefViewModel();
                                HIHDBUtility.HomeDef_DB2VM(reader, vm);
                                listVm.Add(vm);
                            }
                        }
                    }

                    if (listVm.TotalCount > 0)
                    {
                        _cache.Set<BaseListViewModel<HomeDefViewModel>>(cacheKey, listVm, TimeSpan.FromMinutes(10));
                    }
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

        // GET api/homedef/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            HomeDefViewModel vm = null;
            SqlConnection conn = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            try
            {
                String scopeFilter = String.Empty;
                String usrName = "";
                try
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;

                    // Disabled scope check just make it work, 2017.10.1
                    scopeFilter = usrName;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                    //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }

                var cacheKey = String.Format(CacheKeys.HomeDef, id);
                if (_cache.TryGetValue<HomeDefViewModel>(cacheKey, out vm))
                {
                    // Do nothing
                }
                else
                {
                    vm = new HomeDefViewModel();
                    queryString = this.getQueryString(false, null, null, id, scopeFilter);

                    using (conn = new SqlConnection(Startup.DBConnectionString))
                    {
                        await conn.OpenAsync();

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            // Header part
                            while (reader.Read())
                            {
                                HIHDBUtility.HomeDef_DB2VM(reader, vm);

                                // It should return one entry only!
                                // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;

                                break;
                            }

                            reader.NextResult();

                            while (reader.Read())
                            {
                                HomeMemViewModel vmMem = new HomeMemViewModel();
                                HIHDBUtility.HomeMem_DB2VM(reader, vmMem);
                                vm.Members.Add(vmMem);
                            }
                        }
                        else
                        {
                            errorCode = HttpStatusCode.NotFound;
                            throw new Exception();
                        }
                    }

                    _cache.Set<HomeDefViewModel>(cacheKey, vm, TimeSpan.FromMinutes(10));
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

            // Only return the meaningful object
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // POST api/homedef
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]HomeDefViewModel vm)
        {
            String usrId = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrId = usrObj.Value;

                // Disabled scope check just make it work, 2017.10.1

                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                //if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                //{
                //    return StatusCode(401, "Current user has no authority to create home!");
                //}
                //else if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                //{
                //    if (String.CompareOrdinal(vm.Host, usrId) != 0)
                //    {
                //        return StatusCode(401, "Current user can only create home with owner.");
                //    }
                //}
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = HIHDBUtility.getHomeDefInsertString();
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();
                    tran = conn.BeginTransaction();                    
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    // Home def.
                    HIHDBUtility.bindHomeDefInsertParameter(cmd, vm, usrId);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;

                    // Home members
                    cmd.Dispose();
                    cmd = null;
                    queryString = HIHDBUtility.getHomeMemInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    HomeMemViewModel vmMem = new HomeMemViewModel
                    {
                        HomeID = nNewID,
                        CreatedBy = usrId,
                        CreatedAt = DateTime.Now,
                        User = usrId,
                        DisplayAs = vm.CreatorDisplayAs,
                        Relation = (Int16)(HIHHomeMemberRelationship.Self)
                    };
                    HIHDBUtility.bindHomeMemInsertParameter(cmd, vmMem, usrId);
                    await cmd.ExecuteNonQueryAsync();

                    tran.Commit();
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

                if (tran != null)
                    tran.Rollback();
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
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

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PUT api/homedef/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]HomeDefViewModel vm)
        {
            if (vm.ID != id)
            {
                return BadRequest("ID is not match!");
            }

            String usrName = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;

                // Disabled scope check just make it work, 2017.10.1

                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                //if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                //{
                //    return StatusCode(401, "Current user has no authority to create home!");
                //}
                //else if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                //{
                //    if (String.CompareOrdinal(vm.Host, usrName) != 0)
                //    {
                //        return StatusCode(401, "Current user can only create home with owner.");
                //    }
                //}
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String queryString = "";
            String strErrMsg = "";

            try
            {
                queryString = HIHDBUtility.getHomeDefUpdateString();
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    cmd = new SqlCommand(queryString, conn);

                    HIHDBUtility.bindHomeDefUpdateParameter(cmd, vm, usrName);

                    await cmd.ExecuteNonQueryAsync();
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

        // DELETE api/homedef/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest("ID is not match!");
            }

            String usrName = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
                var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                {
                    return StatusCode(401, "Current user has no authority to create home!");
                }
                else if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                {
                    // To do!!
                    //if (String.CompareOrdinal(vm.Host, usrName) != 0)
                    //{
                    //    return StatusCode(401, "Current user can only create home with owner.");
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";

            try
            {
                queryString = HIHDBUtility.getHomeDefDeleteString();
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@ID", id);

                    await cmd.ExecuteNonQueryAsync();
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
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
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

            return Ok();
        }

        #region Implementation methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, String strUserID)
        {

            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[v_homemember] ";
                if (!String.IsNullOrEmpty(strUserID))
                {
                    strSQL += " WHERE [USER] = N'" + strUserID + "'";
                }
                strSQL += " ;";
            }

            strSQL += HIHDBUtility.getHomeDefQueryString(strUserID);

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                if (!String.IsNullOrEmpty(strUserID))
                {
                    strSQL += @" AND [v_homemember].[ID] = " + nSearchID.Value.ToString() + "; ";
                }
                else
                {
                    strSQL += @" WHERE [v_homemember].[ID] = " + nSearchID.Value.ToString() +"; ";
                }

                // Add home member part
                strSQL += HIHDBUtility.getHomeMemQueryString(nSearchID);
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("HomeDefController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }
        #endregion
    }
}
