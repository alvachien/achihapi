using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using achihapi.Utilities;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceLoanTmpDoc")]
    public class FinanceLoanTmpDocController : Controller
    {
        // GET: api/FinanceLoanTmpDoc
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean skipPosted = true, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            if (hid <= 0)
                return BadRequest("No HID inputted");

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

            List<FinanceTmpDocLoanViewModel> listVm = new List<FinanceTmpDocLoanViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = HIHDBUtility.GetFinanceDocLoanListQueryString() + " WHERE [HID] = @hid ";
                if (skipPosted)
                    queryString += " AND [REFDOCID] IS NULL ";
                if (dtbgn.HasValue)
                    queryString += " AND [TRANDATE] >= @dtbgn ";
                if (dtend.HasValue)
                    queryString += " AND [TRANDATE] <= @dtend ";
                queryString += " ORDER BY [TRANDATE] DESC";

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@hid", hid);
                    if (dtbgn.HasValue)
                        cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                    if (dtbgn.HasValue)
                        cmd.Parameters.AddWithValue("@dtend", dtend.Value);

                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceTmpDocLoanViewModel dpvm = new FinanceTmpDocLoanViewModel();
                            HIHDBUtility.FinTmpDocLoan_DB2VM(reader, dpvm);
                            listVm.Add(dpvm);
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
                        return BadRequest();
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

        // GET: api/FinanceLoanTmpDoc/5
        [HttpGet("{id}")]
        public IActionResult Get([FromRoute]int id)
        {
            return Forbid();
        }
        
        // POST: api/FinanceLoanTmpDoc
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromQuery]Int32 hid, Int32 tmpdocid, [FromBody]FinanceDocumentUIViewModel repaydoc)
        {
            return Forbid();
        }

        // PUT: api/FinanceLoanTmpDoc/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put([FromRoute]int id, [FromBody]FinanceTmpDocDPViewModel vm)
        {
            return BadRequest();
        }

        // DELETE: api/FinanceLoanTmpDoc/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }
    }
}
