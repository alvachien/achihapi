using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using achihapi.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class UserDetailController : Controller
    {
        // GET: api/userdetail
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            // It is not allowed
            return BadRequest();
        }

        // GET api/userdetail/id5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(String id)
        {
#if DEBUG
            foreach (var clm in User.Claims.AsEnumerable())
            {
                System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
            }
#endif
            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            if (String.CompareOrdinal(id, usrName) != 0)
                return BadRequest();

            UserDetailViewModel vm = new UserDetailViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bUserNotExist = false;

            try
            {
                queryString = @"SELECT [USERID]
                      ,[DISPLAYAS]
                      ,[EMAIL]
                      ,[OTHERS]
                  FROM [dbo].[t_userdetail]
                  WHERE [USERID] = N'" + id + "'";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        vm.UserID = reader.GetString(0);
                        vm.DisplayAs = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            vm.Email = reader.GetString(2);
                        if (!reader.IsDBNull(3))
                            vm.Others = reader.GetString(3);

                        // Ensure only one record is parsed
                        break;
                    }
                }
                else
                {
                    bUserNotExist = true;
                }

                reader.Dispose();
                cmd.Dispose();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (bUserNotExist)
            {
                return NotFound();
            }

            return new ObjectResult(vm);
        }

        // POST api/userdetail
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]UserDetailViewModel value)
        {
            if (value == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(value))
            {
                // Additional checks
            }
            else
            {
                return BadRequest();
            }

#if DEBUG
            foreach (var clm in User.Claims.AsEnumerable())
            {
                System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
            }
#endif
            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            if (String.IsNullOrEmpty(usrName) || String.Compare(usrName, value.UserID) != 0)
            {
                return BadRequest();
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nRst = -1;

            try
            {
                queryString = @"INSERT INTO [dbo].[t_userdetail]
                       ([USERID]
                       ,[DISPLAYAS]
                       ,[EMAIL]
                       ,[OTHERS])
                 VALUES
                       (@USERID
                       ,@DISPLAYAS
                       ,@EMAIL
                       ,@OTHERS
                        )";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", value.UserID);
                cmd.Parameters.AddWithValue("@DISPLAYAS", value.DisplayAs);
                if (value.Email != null)
                    cmd.Parameters.AddWithValue("@EMAIL", value.Email);
                else
                    cmd.Parameters.AddWithValue("@EMAIL", DBNull.Value);
                if (value.Others != null)
                    cmd.Parameters.AddWithValue("@OTHERS", value.Others);
                else
                    cmd.Parameters.AddWithValue("@OTHERS", DBNull.Value);
                nRst = await cmd.ExecuteNonQueryAsync();

                cmd.Dispose();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (nRst != 1)
            {
                return StatusCode(500, "Failed in DB operation");
            }

            return new ObjectResult(value);
        }

        // PUT api/userdetail/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(String id, [FromBody]UserDetailViewModel value)
        {
            if (value == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(value))
            {
                // Additional checks
            }
            else
            {
                return BadRequest();
            }

#if DEBUG
            foreach (var clm in User.Claims.AsEnumerable())
            {
                System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
            }
#endif
            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            if (String.IsNullOrEmpty(usrName) || String.Compare(usrName, value.UserID) != 0)
            {
                return BadRequest();
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nRst = -1;

            try
            {
                queryString = @"UPDATE [dbo].[t_userdetail]
                       SET [DISPLAYAS] = @DISPLAYAS
                          ,[EMAIL] = @EMAIL
                          ,[OTHERS] = @OTHERS
                     WHERE [USERID] = @USERID";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", value.UserID);
                cmd.Parameters.AddWithValue("@DISPLAYAS", value.DisplayAs);
                if (value.Email != null)
                    cmd.Parameters.AddWithValue("@EMAIL", value.Email);
                else
                    cmd.Parameters.AddWithValue("@EMAIL", DBNull.Value);
                if (value.Others != null)
                    cmd.Parameters.AddWithValue("@OTHERS", value.Others);
                else
                    cmd.Parameters.AddWithValue("@OTHERS", DBNull.Value);
                nRst = await cmd.ExecuteNonQueryAsync();

                cmd.Dispose();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (nRst != 1)
            {
                return StatusCode(500, "Failed in DB operation");
            }

            return new ObjectResult(value);
        }

        // DELETE api/userdetail/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(String id)
        {
#if DEBUG
            foreach (var clm in User.Claims.AsEnumerable())
            {
                System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
            }
#endif
            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            if (String.IsNullOrEmpty(usrName) || String.Compare(usrName, id) != 0)
            {
                return BadRequest();
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nRst = -1;

            try
            {
                queryString = @"DELETE FROM [dbo].[t_userdetail]
                                    WHERE [UserID] = @USERID";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", id);
                nRst = await cmd.ExecuteNonQueryAsync();

                cmd.Dispose();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (nRst != 1)
            {
                return StatusCode(500, "Failed in DB operation");
            }

            return new ObjectResult(nRst);
        }
    }
}
