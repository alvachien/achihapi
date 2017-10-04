using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceControlCenterController : Controller
    {
        // GET: api/financecontrolcenter
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            BaseListViewModel<FinanceControlCenterViewModel> listVMs = new BaseListViewModel<FinanceControlCenterViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getQueryString(true, top, skip, null, hid);

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
                            listVMs.TotalCount = reader.GetInt32(0);
                            //if (listVMs.TotalCount > top)
                            //{
                            //    listVMs.TotalCount = top;
                            //}
                            break;
                        }
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceControlCenterViewModel avm = new FinanceControlCenterViewModel();
                                this.onDB2VM(reader, avm);

                                listVMs.Add(avm);
                            }
                        }
                    }
                    ++nRstBatch;

                    reader.NextResult();
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

        // GET api/financecontrolcenter/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            FinanceControlCenterViewModel vm = new FinanceControlCenterViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = this.getQueryString(false, null, null, id, null);

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
                    bNotFound = true;
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

            if (bNotFound)
            {
                return NotFound();
            }
            else if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
        }

        // POST api/financecontrolcenter
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceControlCenterViewModel vm)
        {
            if (vm == null)
                return BadRequest("No data is inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            if (vm.HID <= 0)
                return BadRequest("No Home Inputted");
            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
            Int32 nDuplicatedID = -1;
            Int32 nNewID = -1;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_controlcenter] WHERE [Name] = N'" + vm.Name + "'";

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

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bDuplicatedEntry = true;
                    while (reader.Read())
                    {
                        nDuplicatedID = reader.GetInt32(0);
                        break;
                    }
                }
                else
                {
                    reader.Dispose();
                    reader = null;

                    cmd.Dispose();
                    cmd = null;

                    // Now go ahead for the creating
                    queryString = @"INSERT INTO [dbo].[t_fin_controlcenter]
                               ([HID]    
                               ,[NAME]
                               ,[PARID]
                               ,[COMMENT]
                               ,[OWNER]
                               ,[CREATEDBY]
                               ,[CREATEDAT]
                               ,[UPDATEDBY]
                               ,[UPDATEDAT])
                         VALUES (@HID 
                               ,@NAME
                               ,@PARID
                               ,@COMMENT
                               ,@OWNER
                               ,@CREATEDBY
                               ,@CREATEDAT
                               ,@UPDATEDBY
                               ,@UPDATEDAT); SELECT @Identity = SCOPE_IDENTITY();";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@NAME", vm.Name);
                    if (vm.ParID.HasValue)
                        cmd.Parameters.AddWithValue("@PARID", vm.ParID.Value);
                    else
                        cmd.Parameters.AddWithValue("@PARID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
                    if (String.IsNullOrEmpty(vm.Owner))
                    {
                        cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
                    }
                    cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                    cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                    cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;
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

            if (bDuplicatedEntry)
            {
                return BadRequest("Controlling center already exists: " + nDuplicatedID.ToString());
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
        }

        // PUT api/financecontrollingcenter/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]FinanceControlCenterViewModel vm)
        {
            return BadRequest();
        }

        // DELETE api/financecontrollingcenter/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }

        #region Implemented methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_controlcenter] WHERE [HID] = " + hid.Value.ToString() + ";";
            }

            strSQL += @" SELECT [ID]
                              ,[HID]
                              ,[NAME]
                              ,[PARID]
                              ,[COMMENT]
                              ,[OWNER]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT] FROM [dbo].[t_fin_controlcenter] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += " WHERE [HID] = " + hid.Value.ToString() 
                            + @" ORDER BY (SELECT NULL) OFFSET " 
                            + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += @" WHERE [ID] = " + nSearchID.Value.ToString();
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, FinanceControlCenterViewModel vm)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.Name = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.ParID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Comment = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Owner = reader.GetString(idx++);
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
