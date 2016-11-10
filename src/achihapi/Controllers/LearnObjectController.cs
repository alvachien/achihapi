using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class LearnObjectController : Controller
    {
        // GET: api/learnobject
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<LearnObjectViewModel> listVm = new List<LearnObjectViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT TOP(1000)[ID]
                      ,[CATEGORY]
                      ,[NAME]
                      ,[CONTENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                        FROM [dbo].[t_learn_obj]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnObjectViewModel vm = new LearnObjectViewModel();
                        onDB2VM(reader, vm);
                        listVm.Add(vm);
                    }
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

            return new ObjectResult(listVm);
        }

        private void onDB2VM(SqlDataReader reader, LearnObjectViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            vm.CategoryID = reader.GetInt32(1);
            vm.Name = reader.GetString(2);
            if (!reader.IsDBNull(3))
                vm.Content = reader.GetString(3);
            if (!reader.IsDBNull(4))
                vm.CreatedBy = reader.GetString(4);
            if (!reader.IsDBNull(5))
                vm.CreatedAt = reader.GetDateTime(5);
            if (!reader.IsDBNull(6))
                vm.UpdatedBy = reader.GetString(6);
            if (!reader.IsDBNull(7))
                vm.UpdatedAt = reader.GetDateTime(7);
        }

        // GET api/learnobject/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            LearnObjectViewModel vm = new LearnObjectViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = @"SELECT [ID]
                      ,[CATEGORY]
                      ,[NAME]
                      ,[CONTENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                        FROM [dbo].[t_learn_obj] WHERE [ID] = " + id.ToString();

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
            else if(bError)
            {
                return StatusCode(500, strErrMsg);
            }

            return new ObjectResult(vm);
        }

        // POST api/learnobject
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LearnObjectViewModel vm)
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
                            FROM [dbo].[t_learn_obj] WHERE [Name] = N'" + vm.Name + "'";

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
                    queryString = @"INSERT INTO [dbo].[t_learn_obj]
                                       ([CATEGORY]
                                       ,[NAME]
                                       ,[CONTENT]
                                       ,[CREATEDBY]
                                       ,[CREATEDAT]
                                       ,[UPDATEDBY]
                                       ,[UPDATEDAT])
                                 VALUES
                                       (@CTGY
                                       ,@NAME
                                       ,@CONTENT
                                       ,@CREATEDBY
                                       ,@CREATEDAT
                                       ,@UPDATEDBY
                                       ,@UPDATEDAT
                                    ); SELECT @Identity = SCOPE_IDENTITY();";

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@CTGY", vm.CategoryID);
                    cmd.Parameters.AddWithValue("@NAME", vm.Name);
                    cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
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
            return new ObjectResult(vm);
        }

        // PUT api/learnobject/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/learnobject/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
