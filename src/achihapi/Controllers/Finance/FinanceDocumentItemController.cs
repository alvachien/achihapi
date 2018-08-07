using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data.SqlClient;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceDocumentItemController : Controller
    {
        // GET: api/financedocumentitems
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0, Int32? top = null, Int32? skip = null, String sort = null, Int32? acntid = null, Int32? ccid = null, Int32? ordid = null,
            DateTime? dtbgn = null, DateTime? dtend = null)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

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
            if (!acntid.HasValue
                && !ccid.HasValue
                && !ordid.HasValue)
            {
                return BadRequest("Choose one of source: Account, Control Center or order!");
            }

            FinanceDocumentItemWithBalanceUIListViewModel listVMs = new FinanceDocumentItemWithBalanceUIListViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                if (acntid.HasValue)
                    queryString = HIHDBUtility.getFinDocItemAccountView(acntid.Value, top, skip, dtbgn, dtend);
                else if (ccid.HasValue)
                    queryString = HIHDBUtility.getFinDocItemControlCenterView(ccid.Value, top, skip, dtbgn, dtend);
                else if (ordid.HasValue)
                    queryString = HIHDBUtility.getFinDocItemOrderView(ordid.Value, top, skip, dtbgn, dtend);

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

                // 1. Total amount
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
                        FinanceDocumentItemWithBalanceUIViewModel avm = new FinanceDocumentItemWithBalanceUIViewModel();
                        HIHDBUtility.FinDocItemWithBalanceList_DB2VM(reader, avm);

                        listVMs.Add(avm);
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
