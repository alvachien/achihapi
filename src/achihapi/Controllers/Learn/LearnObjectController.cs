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

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class LearnObjectController : Controller
    {
        // GET: api/learnobject
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0, Int32? ctgyid = null, Boolean bIncContent = false)
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

            BaseListViewModel<LearnObjectUIViewModel> listVm = new BaseListViewModel<LearnObjectUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getQueryString(bIncContent, ctgyid, true, top, skip, null, hid);

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
                        while(reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            LearnObjectUIViewModel vm = new LearnObjectUIViewModel();
                            onDB2VM(reader, bIncContent, vm);
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
                    conn = null;
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

        private string getQueryString(Boolean bIncContent, Int32? nCtgyID, Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32 hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_obj] WHERE [HID] = " + hid.ToString() + "; ";
            }
            
            strSQL += @"SELECT [t_learn_obj].[ID]
                      ,[t_learn_obj].[HID]
                      ,[t_learn_obj].[CATEGORY]
                      ,[t_learn_ctgy].[NAME] as [CATEGORYNAME]
                      ,[t_learn_obj].[NAME]";
            if (bIncContent)
                strSQL += @",[t_learn_obj].[CONTENT] ";
            strSQL += @",[t_learn_obj].[CREATEDBY]
                      ,[t_learn_obj].[CREATEDAT]
                      ,[t_learn_obj].[UPDATEDBY]
                      ,[t_learn_obj].[UPDATEDAT] 
                        FROM [dbo].[t_learn_obj]
                            INNER JOIN [dbo].[t_learn_ctgy]
                        ON [t_learn_obj].[CATEGORY] = [t_learn_ctgy].[ID] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WHERE [t_learn_obj].[HID] = " + hid.ToString() 
                       + (nCtgyID.HasValue ? (" AND [t_learn_obj].[CATEGORY] = " + nCtgyID.Value.ToString()) : "" )
                       + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " WHERE [t_learn_obj].[ID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, Boolean bIncContent, LearnObjectUIViewModel vm)
        {
            var idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.CategoryID = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                vm.CategoryName = reader.GetString(idx++);
            else
            {
                vm.CategoryName = String.Empty;
                ++idx;
            }                
            if (!reader.IsDBNull(idx))
                vm.Name = reader.GetString(idx++);
            else
            {
                vm.Name = String.Empty;
                ++idx;
            }
                
            if (bIncContent)
            {
                if (!reader.IsDBNull(idx))
                    vm.Content = reader.GetString(idx ++);
                else
                    ++idx;
            }
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

        // GET api/learnobject/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
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

            LearnObjectUIViewModel vm = new LearnObjectUIViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = this.getQueryString(true, null, false, null, null, id, hid);

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
                        onDB2VM(reader, true, vm);
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
                    conn = null;
                }
            }

            if (bNotFound)
            {
                return NotFound();
            } 
            else if(bError)
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // POST api/learnobject, create an object
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]LearnObjectViewModel vm)
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

            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }
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
                            FROM [dbo].[t_learn_obj] WHERE [Name] = N'" + vm.Name + "' AND [HID] = " + vm.HID.ToString();

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
                    queryString = @"INSERT INTO [dbo].[t_learn_obj]
                                       ([HID]
                                       ,[CATEGORY]
                                       ,[NAME]
                                       ,[CONTENT]
                                       ,[CREATEDBY]
                                       ,[CREATEDAT])
                                 VALUES (@HID
                                       ,@CTGY
                                       ,@NAME
                                       ,@CONTENT
                                       ,@CREATEDBY
                                       ,@CREATEDAT
                                    ); SELECT @Identity = SCOPE_IDENTITY();";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@CTGY", vm.CategoryID);
                    cmd.Parameters.AddWithValue("@NAME", vm.Name);
                    cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
                    cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                    cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
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
                return BadRequest("Object with name already exists: " + nDuplicatedID.ToString());
            }

            if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PUT api/learnobject/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]LearnObjectViewModel vm)
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

            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }
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
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"UPDATE [dbo].[t_learn_obj]
                                SET [CATEGORY] = @CTGY
                                    ,[NAME] = @NAME
                                    ,[CONTENT] = @CONTENT
                                    ,[UPDATEDBY] = @UPDATEDBY
                                    ,[UPDATEDAT] = @UPDATEDAT
                                WHERE [ID] = @OBJID";

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
                cmd.Parameters.AddWithValue("@CTGY", vm.CategoryID);
                cmd.Parameters.AddWithValue("@NAME", vm.Name);
                cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
                cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                cmd.Parameters.AddWithValue("@OBJID", vm.ID);
                Int32 nRst = await cmd.ExecuteNonQueryAsync();
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

        // DELETE api/learnobject/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, [FromQuery]Int32 hid = 0)
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

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            Int32 usageAmount = 0;

            String strErrMsg = "";

            try
            {
                queryString = @"SELECT COUNT( * )
                            FROM [dbo].[t_learn_hist] WHERE [OBJECTID] = " + id.ToString() + " AND [HID] = " + hid.ToString();

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
                        usageAmount = reader.GetInt32(0);
                        break;
                    }
                }

                if (usageAmount <= 0)
                {
                    reader.Dispose();
                    reader = null;

                    cmd.Dispose();
                    cmd = null;

                    // Now go ahead for the creating
                    queryString = @"DELETE FROM [t_learn_obj] WHERE [ID] = " + id.ToString();

                    cmd = new SqlCommand(queryString, conn);
                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
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

            if (usageAmount > 0)
            {
                return BadRequest("Object still in use: " + usageAmount.ToString());
            }

            if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            return Ok();
        }
    }
}
