using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/DBVersionCheck")]
    public class DBVersionCheckController : Controller
    {
        // GET: api/DBVersionCheck
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            List<DBVersionViewModel> listVM = new List<DBVersionViewModel>();

            try
            {
                await conn.OpenAsync();

                queryString = @"SELECT [VersionID]
                              ,[ReleasedDate]
                              ,[AppliedDate]
                          FROM [dbo].[t_dbversion]
                          ORDER BY [VersionID] DESC";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DBVersionViewModel vm = new DBVersionViewModel();
                        vm.VersionID = reader.GetInt32(0);
                        vm.ReleasedDate = reader.GetDateTime(1);
                        vm.AppliedDate = reader.GetDateTime(2);
                        listVM.Add(vm);
                    }
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
            {
                return StatusCode(500, strErrMsg);
            }

            return new JsonResult(listVM);
        }

        // GET: api/DBVersionCheck/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Forbid();
        }
        
        // POST: api/DBVersionCheck
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Int32 reqver)
        {
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            DBVersionViewModel vmCurrent = new DBVersionViewModel();

            try
            {
                await conn.OpenAsync();

                queryString = @"SELECT TOP (1) [VersionID]
                                  ,[ReleasedDate]
                                  ,[AppliedDate]
                              FROM [dbo].[t_dbversion]
                              ORDER BY [VersionID] DESC";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        vmCurrent.VersionID = reader.GetInt32(0);
                        vmCurrent.ReleasedDate = reader.GetDateTime(1);
                        vmCurrent.AppliedDate = reader.GetDateTime(2);

                        // Only 1 row
                        break;
                    }
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
            {
                return StatusCode(500, strErrMsg);
            }

            return new JsonResult(vmCurrent);
        }

        // PUT: api/DBVersionCheck/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
