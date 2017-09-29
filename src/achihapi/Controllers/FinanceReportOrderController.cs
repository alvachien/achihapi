﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceReportOrderController : Controller
    {
        // GET: api/financereportorder
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid)
        {
            List<FinanceReportOrderViewModel> listVm = new List<FinanceReportOrderViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            if (hid == 0)
                return BadRequest("No HID inputted");

            try
            {
                queryString = @"SELECT [ORDERID]
                          ,[ORDERNAME]
                          ,[debit_balance]
                          ,[credit_balance]
                          ,[balance]
                      FROM [dbo].[v_fin_report_order] WHERE [HID] = " + hid.ToString();

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceReportOrderViewModel avm = new FinanceReportOrderViewModel
                        {
                            OrderID = reader.GetInt32(0),
                            OrderName = reader.GetString(1)
                        };
                        if (reader.IsDBNull(2))
                            avm.DebitBalance = 0;
                        else
                            avm.DebitBalance = reader.GetDecimal(2);
                        if (reader.IsDBNull(3))
                            avm.CreditBalance = 0;
                        else
                            avm.CreditBalance = reader.GetDecimal(3);
                        if (reader.IsDBNull(4))
                            avm.Balance = 0;
                        else
                            avm.Balance = reader.GetDecimal(4);
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
