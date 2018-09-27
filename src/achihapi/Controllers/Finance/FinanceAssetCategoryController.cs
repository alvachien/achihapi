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

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceAssetCategoryController : Controller
    {
        // GET: api/financeassetcategory
        [HttpGet]
        [Authorize]
        [ResponseCache(Duration = 600)]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0, Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceAssetCtgyViewModel> listVMs = new BaseListViewModel<FinanceAssetCtgyViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

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

            try
            {
                if (hid == 0)
                    queryString = this.getQueryString(true, top, skip, null, null);
                else
                    queryString = this.getQueryString(true, top, skip, null, hid);

                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Basic check
                    if (hid != 0)
                    {
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
                            listVMs.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    reader.NextResult();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceAssetCtgyViewModel avm = new FinanceAssetCtgyViewModel();
                            this.onDB2VM(reader, avm);
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

        // GET api/financeassetcategory/5
        [HttpGet("{id}")]
        [Authorize]
        [ResponseCache(Duration = 600)]
        public async Task<IActionResult> Get([FromRoute]int id, [FromQuery]Int32 hid = 0)
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

            FinanceAssetCtgyViewModel vm = new FinanceAssetCtgyViewModel();

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                if (hid == 0)
                    queryString = this.getQueryString(false, null, null, id, null);
                else
                    queryString = this.getQueryString(false, null, null, id, hid);

                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Basic check
                    if (hid != 0)
                    {
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
                            onDB2VM(reader, vm);
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

        // POST api/financeassetcateogry
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]string value)
        {
            return BadRequest();
        }

        // PUT api/financeassetcateogry/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put([FromRoute]int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/financeassetcateogry/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }

        #region Implmented methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_asset_ctgy] WHERE [HID] IS NULL ";
                if (hid.HasValue)
                    strSQL += @" OR [HID] = " + hid.Value.ToString() + ";";
            }

            strSQL += @" SELECT [ID]
                          ,[HID]
                          ,[NAME]
                          ,[DESP]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT] FROM [dbo].[t_fin_asset_ctgy] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WHERE [HID] IS NULL ";
                if (hid.HasValue)
                    strSQL += " OR [HID] = " + hid.Value.ToString();
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " WHERE [t_fin_asset_ctgy].[ID] = " + nSearchID.Value.ToString();
                if (hid.HasValue)
                    strSQL += " AND [t_fin_asset_ctgy].[ID] =  " + hid.Value.ToString();
            }

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, FinanceAssetCtgyViewModel vm)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = null;
            if (!reader.IsDBNull(idx))
                vm.HID = reader.GetInt32(idx++);
            else
                ++idx;
            vm.Name = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.Desp = reader.GetString(idx++);
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
