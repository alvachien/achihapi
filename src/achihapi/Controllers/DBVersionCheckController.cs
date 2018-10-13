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
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/DBVersionCheck")]
    public class DBVersionCheckController : Controller
    {
        // Version 4 - 2018.07
        // Version 5 - 2018.08.02
        // Version 6 - 2018.08.05
        // Version 7 - 2018.09.11
        // Version 8 - 2018.09.15
        // Version 9 - 2018.09.16
        // Version 10 - 2018.10.13
        public static Int32 CurrentVersion = 10;

        // GET: api/DBVersionCheck
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            List<DBVersionViewModel> listVM = new List<DBVersionViewModel>();

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    queryString = @"SELECT [VersionID]
                              ,[ReleasedDate]
                              ,[AppliedDate]
                          FROM [dbo].[t_dbversion]
                          ORDER BY [VersionID] DESC";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

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
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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
                    conn.Close();
                    conn.Dispose();
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
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            DBVersionViewModel vmCurrent = new DBVersionViewModel();

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    queryString = @"SELECT TOP (1) [VersionID]
                                  ,[ReleasedDate]
                                  ,[AppliedDate]
                              FROM [dbo].[t_dbversion]
                              ORDER BY [VersionID] DESC";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

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
                        while (nver <= DBVersionCheckController.CurrentVersion)
                        {
                            tran = conn.BeginTransaction();

                            // Update the DB version
                            await updateDBVersion(conn, tran, nver++);

                            tran.Commit();
                        }
                    }
                    else if (vmCurrent.VersionID > CurrentVersion)
                    {
                        strErrMsg = "Contact system administrator";
                        throw new Exception(strErrMsg);
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;

                if (tran != null)
                {
                    tran.Rollback();
                }
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
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
