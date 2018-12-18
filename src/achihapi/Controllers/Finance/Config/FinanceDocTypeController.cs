using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data.SqlClient;
using achihapi.Utilities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceDocTypeController : Controller
    {
        private IMemoryCache _cache;
        public FinanceDocTypeController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/financedoctype
        [HttpGet]
        [Authorize]
        [Produces(typeof(List<FinanceDocTypeViewModel>))]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0)
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

            List<FinanceDocTypeViewModel> listVMs = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinDocTypeList, hid);

                if (_cache.TryGetValue<List<FinanceDocTypeViewModel>>(cacheKey, out listVMs))
                {
                    // Do nothing
                }
                else
                {
                    listVMs = new List<FinanceDocTypeViewModel>();
                    if (hid == 0)
                        queryString = @"SELECT [ID]
                              ,[HID]
                              ,[NAME]
                              ,[COMMENT]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                              FROM [dbo].[t_fin_doc_type] WHERE [HID] IS NULL";
                    else
                        queryString = @"SELECT [ID]
                          ,[HID]
                          ,[NAME]
                          ,[COMMENT]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                          FROM [dbo].[t_fin_doc_type] WHERE [HID] IS NULL OR [HID] = " + hid.ToString();

                    using (conn = new SqlConnection(Startup.DBConnectionString))
                    {
                        await conn.OpenAsync();

                        if (hid != 0)
                        {
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
                        }

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceDocTypeViewModel avm = new FinanceDocTypeViewModel();
                                this.onDB2VM(reader, avm);
                                listVMs.Add(avm);
                            }
                        }
                    }

                    this._cache.Set<List<FinanceDocTypeViewModel>>(cacheKey, listVMs, TimeSpan.FromSeconds(1200));
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
            
            return new JsonResult(listVMs, setting);
        }

        // GET api/financedoctype/5
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get([FromRoute]int id, [FromQuery]Int32 hid = 0)
        {
            return Forbid();

            //FinanceDocTypeViewModel vm = new FinanceDocTypeViewModel();
            //SqlConnection conn = null;
            //SqlCommand cmd = null;
            //SqlDataReader reader = null;
            //String queryString = "";
            //String strErrMsg = "";
            //HttpStatusCode errorCode = HttpStatusCode.OK;

            //try
            //{
            //    queryString = this.getQueryString(false, null, null, id, null);

            //    using (conn = new SqlConnection(Startup.DBConnectionString))
            //    {
            //        await conn.OpenAsync();

            //        cmd = new SqlCommand(queryString, conn);
            //        reader = cmd.ExecuteReader();
            //        if (reader.HasRows)
            //        {
            //            while (reader.Read())
            //            {
            //                onDB2VM(reader, vm);
            //                break; // Should only one result!!!
            //            }
            //        }
            //        else
            //        {
            //            errorCode = HttpStatusCode.NotFound;
            //            throw new Exception();
            //        }
            //    }
            //}
            //catch (Exception exp)
            //{
            //    System.Diagnostics.Debug.WriteLine(exp.Message);
            //    strErrMsg = exp.Message;
            //    if (errorCode == HttpStatusCode.OK)
            //        errorCode = HttpStatusCode.InternalServerError;
            //}
            //finally
            //{
            //    if (reader != null)
            //    {
            //        reader.Dispose();
            //        reader = null;
            //    }
            //    if (cmd != null)
            //    {
            //        cmd.Dispose();
            //        cmd = null;
            //    }
            //    if (conn != null)
            //    {
            //        conn.Dispose();
            //        conn = null;
            //    }
            //}

            //if (errorCode != HttpStatusCode.OK)
            //{
            //    switch (errorCode)
            //    {
            //        case HttpStatusCode.Unauthorized:
            //            return Unauthorized();
            //        case HttpStatusCode.NotFound:
            //            return NotFound();
            //        case HttpStatusCode.BadRequest:
            //            return BadRequest();
            //        default:
            //            return StatusCode(500, strErrMsg);
            //    }
            //}

            //var setting = new Newtonsoft.Json.JsonSerializerSettings
            //{
            //    DateFormatString = HIHAPIConstants.DateFormatPattern,
            //    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            //};
            
            //return new JsonResult(vm, setting);
        }

        // POST api/financedoctype
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]string value)
        {
            return BadRequest();
        }

        // PUT api/financedoctype/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put([FromRoute]int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/financedoctype/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }

        #region Implement methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_doc_type] WHERE [HID] IS NULL ";
                if (hid.HasValue && hid.Value != 0)
                    strSQL += " OR [HID] = " + hid.Value.ToString() + ";";
            }

            strSQL += @" SELECT [ID]
                          ,[HID]
                          ,[NAME]
                          ,[COMMENT]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                          FROM [dbo].[t_fin_doc_type] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += "WHERE [HID] IS NULL ";
                if (hid.HasValue && hid.Value != 0)
                    strSQL += " OR [HID] = " + hid.Value.ToString();
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " WHERE [t_fin_doc_type].[ID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, FinanceDocTypeViewModel vm)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt16(idx++);
            vm.HID = null;
            if (!reader.IsDBNull(idx))
                vm.HID = reader.GetInt32(idx++);
            else
                ++idx;
            vm.Name = reader.GetString(idx++);
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
        #endregion
    }
}
