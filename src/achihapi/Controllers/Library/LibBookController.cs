using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LibBook")]
    public class LibBookController : Controller
    {
        // GET: api/LibBook
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0, Int32 top = 100, Int32 skip = 0)
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

            BaseListViewModel<LibBookViewModel> listVm = new BaseListViewModel<LibBookViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.GetQueryString(true, top, skip, null, hid);

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

                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            LibBookViewModel vm = new LibBookViewModel();
                            OnDB2VM(reader, vm);
                            listVm.Add(vm);
                        }
                    }

                    ++nRstBatch;

                    reader.NextResult();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
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

            return new JsonResult(listVm, setting);
        }

        // GET: api/LibBook/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            return BadRequest();
        }

        // POST: api/LibBook
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]string value)
        {
            return BadRequest();
        }

        // PUT: api/LibBook/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }

        #region Implemented methods
        private string GetQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32 hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_lib_book] WHERE [HID] = " + hid.ToString() + "; ";
            }

            strSQL += @"SELECT [ID]
                          ,[HID]
                          ,[Ctgy]
                          ,[NativeName]
                          ,[EnglishName]
                          ,[EnglishIsNative]
                          ,[ShortIntro]
                          ,[ExtLink1]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_lib_book] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WHERE [t_lib_book].[HID] = " + hid.ToString()
                       + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " WHERE [t_lib_book].[ID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void OnDB2VM(SqlDataReader reader, LibBookViewModel vm)
        {
            var idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.CategoryID = reader.GetInt32(idx++);
            vm.NativeName = reader.GetString(idx++);            
            if (!reader.IsDBNull(idx))
                vm.EnglishName = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.EnglishIsNative = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ShortIntro = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ExtLink1 = reader.GetString(idx++);
            else
                ++ idx;
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
