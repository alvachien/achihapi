using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LibBookCategory")]
    public class LibBookCategoryController : Controller
    {
        // GET: api/LibBookCategory
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

            BaseListViewModel<LibBookCategoryViewModel> listVm = new BaseListViewModel<LibBookCategoryViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getQueryString(true, top, skip, null, hid);

                await conn.OpenAsync();

                // Check Home assignment with current user
                if (hid > 0)
                {
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception exp)
                    {
                        return BadRequest(exp.Message);
                    }
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
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            LibBookCategoryViewModel vm = new LibBookCategoryViewModel();
                            SqlUtility.LibBookCategory_DB2VM(reader, vm);
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

            return new JsonResult(listVm, setting);
        }

        // GET: api/LibBookCategory/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return BadRequest();
        }

        // POST: api/LibBookCategory
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return BadRequest();
        }

        // PUT: api/LibBookCategory/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return BadRequest();
        }

        #region Implementation methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {

            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_lib_book_ctgy] WHERE [HID] IS NULL ";
                if (hid.HasValue && hid.Value != 0)
                    strSQL += " OR [HID] = " + hid.Value.ToString() + ";";
            }

            strSQL += SqlUtility.getLibBookCategoryQueryString();

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
                strSQL += @" AND [t_lib_book_ctgy].[ID] = " + nSearchID.Value.ToString();
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("LibBookCategoryController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }
        #endregion
    }
}
