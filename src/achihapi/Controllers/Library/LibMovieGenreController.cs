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
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LibMovieGenre")]
    public class LibMovieGenreController : Controller
    {
        // GET: api/LibMovieGenre
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0, Int32 top = 100, Int32 skip = 0)
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

            BaseListViewModel<LibMovieGenreViewModel> listVm = new BaseListViewModel<LibMovieGenreViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = this.getQueryString(true, top, skip, null, hid);

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    if (hid > 0)
                    {
                        try
                        {
                            HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                        }
                        catch (Exception)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw;
                        }
                    }

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    reader.NextResult();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LibMovieGenreViewModel vm = new LibMovieGenreViewModel();
                            HIHDBUtility.LibMovieGenre_DB2VM(reader, vm);
                            listVm.Add(vm);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);

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
                        return BadRequest(strErrMsg);
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listVm, setting);
        }

        // GET: api/LibMovieGenre/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Forbid();
        }
        
        // POST: api/LibMovieGenre
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return Forbid();
        }
        
        // PUT: api/LibMovieGenre/5
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

        #region Implementation methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {

            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_lib_movie_genre] WHERE [HID] IS NULL ";
                if (hid.HasValue && hid.Value != 0)
                    strSQL += " OR [HID] = " + hid.Value.ToString() + ";";
            }

            strSQL += HIHDBUtility.getLibMovieGenreQueryString();

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += " WHERE [HID] IS NULL ";
                if (hid.HasValue && hid.Value != 0)
                    strSQL += " OR [HID] = " + hid.Value.ToString();
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += @" AND [t_lib_movie_genre].[ID] = " + nSearchID.Value.ToString();
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LibMovieGenreController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }
        #endregion
    }
}
