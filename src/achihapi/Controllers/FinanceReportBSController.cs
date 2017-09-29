using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceReportBSController : Controller
    {
        // GET: api/financereportbs
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid)
        {
            List<FinanceReportBSViewModel> listVm = new List<FinanceReportBSViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [accountid]
                          ,[ACCOUNTNAME]
                          ,[ACCOUNTCTGYID]
                          ,[ACCOUNTCTGYNAME]
                          ,[debit_balance]
                          ,[credit_balance]
                          ,[balance]
                      FROM [dbo].[v_fin_report_bs]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceReportBSViewModel avm = new FinanceReportBSViewModel
                        {
                            AccountID = reader.GetInt32(0),
                            AccountName = reader.GetString(1),
                            AccountCategoryID = reader.GetInt32(2),
                            AccountCategoryName = reader.GetString(3)
                        };
                        if (reader.IsDBNull(4))
                            avm.DebitBalance = 0;
                        else
                            avm.DebitBalance = reader.GetDecimal(4);
                        if (reader.IsDBNull(5))
                            avm.CreditBalance = 0;
                        else
                            avm.CreditBalance = reader.GetDecimal(5);
                        if (reader.IsDBNull(6))
                            avm.Balance = 0;
                        else
                            avm.Balance = reader.GetDecimal(6);
                        listVm.Add(avm);
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
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(listVm, setting);
        }
    }
}
