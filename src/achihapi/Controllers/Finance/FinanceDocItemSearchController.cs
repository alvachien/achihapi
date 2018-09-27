using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data.SqlClient;
using achihapi.Utilities;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceDocItemSearch")]
    [Authorize]
    public class FinanceDocItemSearchController : Controller
    {
        // GET: api/FinanceDocItemSearch
        [HttpPost]
        public async Task<IActionResult> Search([FromBody]FinanceDocItemSearchFilterViewModel filters, [FromQuery]Int32 hid = 0, Int32 top = 100, Int32 skip = 0)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (hid <= 0)
                return BadRequest("No Home Inputted");
            if (filters == null || filters.FieldList.Count <= 0)
                return BadRequest("No filter Inputted");

            // Do the basic checks
            foreach(var filter in filters.FieldList)
            {
                if (!filter.IsValid())
                    return BadRequest("Invalid filter found");
            }

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

            FinanceDocItemSearchResultListViewModel listVMs = new FinanceDocItemSearchResultListViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                String subqueries = " HID = " + hid.ToString() + " AND " + filters.GetFullWhereClause();
                queryString = HIHDBUtility.getFinDocItemSearchView(subqueries, top, skip);

                using (conn = new SqlConnection(Startup.DBConnectionString))
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
                    reader = cmd.ExecuteReader();

                    // 1. Count
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            listVMs.TotalCount = reader.GetInt32(0);
                            break;
                        }
                        reader.NextResult();
                    }

                    // 2. Items
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceDocItemSearchResultViewModel avm = new FinanceDocItemSearchResultViewModel();
                            HIHDBUtility.FinDocItem_SearchView2VM(reader, avm);

                            listVMs.Add(avm);
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

            return new JsonResult(listVMs, setting);
        }
    }
}
