using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Text;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/DBVersionCheck")]
    public class DBVersionCheckController : Controller
    {
        // Version 4 - 2018.07
        // Version 5 - 2018.08.02
        public static Int32 CurrentVersion = 5;

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
        public async Task<IActionResult> Post()
        {
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            DBVersionViewModel vmCurrent = new DBVersionViewModel();
            SqlTransaction tran = null;

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
                else
                    vmCurrent.VersionID = 0;
                reader.Close();
                reader = null;
                cmd.Dispose();
                cmd = null;

                if (vmCurrent.VersionID < DBVersionCheckController.CurrentVersion)
                {
                    var nver = vmCurrent.VersionID + 1;
                    while(nver <= DBVersionCheckController.CurrentVersion)
                    {
                        tran = conn.BeginTransaction();

                        // Update the DB version
                        await updateDBVersion(conn, tran, nver++);

                        tran.Commit();
                    }
                }
                else if (vmCurrent.VersionID > CurrentVersion)
                {
                    bError = true;
                    strErrMsg = "Contact system administrator";
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                if (tran != null)
                {
                    tran.Rollback();
                    tran.Dispose();
                    tran = null;
                }
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
            {
                return StatusCode(500, strErrMsg);
            }

            return Ok();
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

        private async Task updateDBVersion(SqlConnection conn, SqlTransaction tran, Int32 nversion)
        {
            var sqlfile = "achihapi.Sqls.Delta.v" + nversion.ToString() + ".sql";
            try
            {
                using (var stream = typeof(DBVersionCheckController).GetTypeInfo().Assembly.GetManifestResourceStream(sqlfile))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        String strcontent = reader.ReadToEnd();

                        SqlCommand cmd = new SqlCommand(strcontent, conn, tran);
                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                // ApplicationProvider.WriteToLog<EmbeddedResource>().Error(exception.Message);
                throw new Exception($"Failed to read Embedded Resource {sqlfile}, reason: {exception.Message}");
            }
        }
    }
}
