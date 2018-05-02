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

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceDocItemSearch")]
    [Authorize]
    public class FinanceDocItemSearchController : Controller
    {
        // GET: api/FinanceDocItemSearch
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, [FromBody]FinanceDocItemSearchFilterViewModel filters)
        {
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

            List<FinanceDocumentItemUIViewModel> listVMs = new List<FinanceDocumentItemUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                String subqueries = "";
                for(Int32 i = 0; i < filters.FieldList.Count; i ++)
                {
                    subqueries += filters.FieldList[i].GenerateSql();
                    if (i != filters.FieldList.Count - 1)
                        subqueries += " AND ";
                }
                
                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.HasRows)
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceDocumentItemViewModel avm = new FinanceDocumentItemViewModel();
                            SqlUtility.FinDocItemWithBalanceList_DB2VM(reader, avm);

                            listVMs.Add(avm);
                        }
                    }
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

            return new JsonResult(listVMs, setting);
        }
    }
}
