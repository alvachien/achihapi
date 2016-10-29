using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class UserHistoryController : Controller
    {
        // GET: api/userhistory
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            List<UserHistoryViewModel> listVMs = new List<UserHistoryViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bUserNotExist = false;

            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;

                queryString = @"SELECT [USERID]
                              ,[SEQNO]
                              ,[HISTTYP]
                              ,[TIMEPOINT]
                              ,[OTHERS]
                          FROM [dbo].[t_userhist]
                        WHERE [USERID] = N'" + usrName + "'";

                conn.Open();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        UserHistoryViewModel vm = new UserHistoryViewModel();
                        vm.UserID = reader.GetString(0);
                        vm.SeqNo = reader.GetInt32(1);
                        vm.HistType = reader.GetByte(2);
                        vm.TimePoint = reader.GetDateTime(3);
                        if (!reader.IsDBNull(4))
                            vm.Others = reader.GetString(4);

                        listVMs.Add(vm);
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

            return new ObjectResult(listVMs);
        }

        // GET api/userhistory/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(String usrid)
        {
            return Forbid();
        }

        // POST api/userhistory
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]UserHistoryViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
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
            if (String.IsNullOrEmpty(usrName) || String.Compare(usrName, vm.UserID) != 0)
            {
                return BadRequest();
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nRst = -1;

            try
            {
                queryString = @"INSERT INTO [dbo].[t_userhist]
                                   ([USERID]
                                   ,[SEQNO]
                                   ,[HISTTYP]
                                   ,[TIMEPOINT]
                                   ,[OTHERS])
                             select @USERID as [USERID], COUNT(SEQNO) + 1 AS SEQNO, @HISTTYP as HISTTYPE,@TIMEPOINT
                                   ,@OTHERS FROM [dbo].[t_userhist] WHERE [USERID] = @USERID 
                                ";

                conn.Open();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                cmd.Parameters.AddWithValue("@SEQNO", vm.SeqNo);
                cmd.Parameters.AddWithValue("@HISTTYP", vm.HistType);
                cmd.Parameters.AddWithValue("@TIMEPOINT", vm.TimePoint);
                if (!String.IsNullOrEmpty(vm.Others))
                    cmd.Parameters.AddWithValue("@OTHERS", vm.Others);
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

            return new ObjectResult(vm);
        }

        // PUT api/userhistory/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
            //String cmdText = @"UPDATE [dbo].[t_userhist]
            //       SET [USERID] = <USERID, nvarchar(40),>
            //          ,[SEQNO] = <SEQNO, int,>
            //          ,[HISTTYP] = <HISTTYP, tinyint,>
            //          ,[TIMEPOINT] = <TIMEPOINT, datetime,>
            //          ,[OTHERS] = <OTHERS, nvarchar(50),>
            //     WHERE <Search Conditions,,>";
        }

        // DELETE api/userhistory/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
      //      String cmdText = @"DELETE FROM [dbo].[t_userhist]
      //WHERE <Search Conditions,,>";
        }
    }
}
