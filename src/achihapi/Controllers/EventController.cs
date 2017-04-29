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
    public class EventController : Controller
    {
        // GET: api/event
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<EventViewModel> listVm = new BaseListViewModel<EventViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT count(*) FROM [dbo].[t_event];
                    SELECT [ID]
                          ,[Name]
                          ,[StartTime]
                          ,[EndTime]
                          ,[Content]
                          ,[IsPublic]
                          ,[Owner]
                          ,[RefID]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_event]
                        ORDER BY (SELECT NULL)
                        OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY;";

                await conn.OpenAsync();
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
                            EventViewModel vm = new EventViewModel();
                            onDB2VM(reader, vm);
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
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(listVm, setting);
        }

        private void onDB2VM(SqlDataReader reader, EventViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            vm.Name = reader.GetString(1);
            vm.StartTimePoint = reader.GetDateTime(2);
            vm.EndTimePoint = reader.GetDateTime(3);
            if (!reader.IsDBNull(4))
                vm.Content = reader.GetString(4);
            if (!reader.IsDBNull(5))
                vm.IsPublic = reader.GetBoolean(5);
            if (!reader.IsDBNull(6))
                vm.Owner = reader.GetString(6);
            if (!reader.IsDBNull(7))
                vm.RefID = reader.GetInt32(7);
            if (!reader.IsDBNull(8))
                vm.CreatedBy = reader.GetString(8);
            if (!reader.IsDBNull(9))
                vm.CreatedAt = reader.GetDateTime(9);
            if (!reader.IsDBNull(10))
                vm.UpdatedBy = reader.GetString(10);
            if (!reader.IsDBNull(11))
                vm.UpdatedAt = reader.GetDateTime(11);
        }

        // GET api/event/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            EventViewModel vm = new EventViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = @"SELECT [ID]
                          ,[Name]
                          ,[StartTime]
                          ,[EndTime]
                          ,[Content]
                          ,[IsPublic]
                          ,[Owner]
                          ,[RefID]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_event] WHERE [ID] = " + id.ToString();

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        onDB2VM(reader, vm);
                        break; // Should only one result!!!
                    }

                    reader.NextResult();
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
                conn.Close();
                conn.Dispose();
            }

            if (bNotFound)
            {
                return NotFound();
            }
            else if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(vm, setting);
        }

        // POST api/event
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]EventViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

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
            var usr = User.FindFirst(c => c.Type == "sub");
            String usrName = String.Empty;
            if (usr != null)
                usrName = usr.Value;

            try
            {
                queryString = @"SELECT [ID]
                            FROM [dbo].[t_event] WHERE [Name] = N'" + vm.Name + "'";

                await conn.OpenAsync();

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
                    queryString = @"INSERT INTO [dbo].[t_event]
                               ([Name]
                               ,[StartTime]
                               ,[EndTime]
                               ,[Content]
                               ,[IsPublic]
                               ,[Owner]
                               ,[RefID]
                               ,[CREATEDBY]
                               ,[CREATEDAT]
                               ,[UPDATEDBY]
                               ,[UPDATEDAT])
                         VALUES
                               (@NAME
                               ,@STARTTIME
                               ,@ENDTIME
                               ,@CONTENT
                               ,@ISPUBLIC
                               ,@OWNER
                               ,@REFID
                               ,@CREATEDBY
                               ,@CREATEDAT
                               ,@UPDATEDBY
                               ,@UPDATEDAT
                               ); SELECT @Identity = SCOPE_IDENTITY();";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@NAME", vm.Name);
                    cmd.Parameters.AddWithValue("@STARTTIME", vm.StartTimePoint);
                    cmd.Parameters.AddWithValue("@ENDTIME", vm.EndTimePoint);
                    if (String.IsNullOrEmpty(vm.Content))
                        cmd.Parameters.AddWithValue("@CONTENT", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
                    cmd.Parameters.AddWithValue("@ISPUBLIC", vm.IsPublic);
                    if (vm.Owner == null)
                        cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
                    if (vm.RefID == null)
                        cmd.Parameters.AddWithValue("REFID", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("REFID", vm.RefID);
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
                conn.Close();
                conn.Dispose();
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
            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(vm, setting);
        }

        // PUT api/event/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            //String queryString = @"UPDATE [dbo].[t_event]
            //       SET [Name] = <Name, nvarchar(50),>
            //          ,[StartTime] = <StartTime, datetime,>
            //          ,[EndTime] = <EndTime, datetime,>
            //          ,[Content] = <Content, nvarchar(max),>
            //          ,[IsPublic] = <IsPublic, bit,>
            //          ,[Owner] = <Owner, nvarchar(40),>
            //          ,[RefID] = <RefID, int,>
            //          ,[CREATEDBY] = <CREATEDBY, nvarchar(40),>
            //          ,[CREATEDAT] = <CREATEDAT, date,>
            //          ,[UPDATEDBY] = <UPDATEDBY, nvarchar(40),>
            //          ,[UPDATEDAT] = <UPDATEDAT, date,>
            //     WHERE <Search Conditions,,>";
        }

        // DELETE api/event/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //String queryString = @"DELETE FROM [dbo].[t_event]
            //    WHERE <Search Conditions,,>";
        }
    }
}
