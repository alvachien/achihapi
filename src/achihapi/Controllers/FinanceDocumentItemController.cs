using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceDocumentItemController : Controller
    {
        // GET: api/financedocumentitems
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0, Int32? acntid = null, Int32? ccid = null, Int32? ordid = null)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            List<FinanceDocumentItemWithBalanceUIViewModel> listVMs = new List<FinanceDocumentItemWithBalanceUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            if (!acntid.HasValue
                && !ccid.HasValue
                && !ordid.HasValue)
            {
                return BadRequest("No inputs!");
            }

            try
            {
                if (acntid.HasValue)
                    queryString = SqlUtility.getFinDocItemAccountView(acntid.Value);
                else if (ccid.HasValue)
                    queryString = SqlUtility.getFinDocItemControlCenterView(ccid.Value);
                else if (ordid.HasValue)
                    queryString = SqlUtility.getFinDocItemOrderView(ordid.Value);

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
                            FinanceDocumentItemWithBalanceUIViewModel avm = new FinanceDocumentItemWithBalanceUIViewModel();
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
