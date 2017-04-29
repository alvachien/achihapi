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
    public class FinanceCurrencyController : Controller
    {
        // GET: api/financecurrency
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceCurrencyViewModel> listVMs = new BaseListViewModel<FinanceCurrencyViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getQueryString(true, top, skip, null);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        while (reader.Read())
                        {
                            listVMs.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceCurrencyViewModel avm = new FinanceCurrencyViewModel();
                                this.onDB2VM(reader, avm);

                                listVMs.Add(avm);
                            }
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
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(listVMs, setting);
        }

        // GET api/financecurrency/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string sid)
        {
            FinanceCurrencyViewModel vm = new FinanceCurrencyViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = this.getQueryString(false, null, null, sid);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        onDB2VM(reader, vm);
                        break; // Should only one result!!!
                    }
                }
                else
                {
                    bNotFound = true;
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

            if (bNotFound)
            {
                return NotFound();
            }
            else if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(vm, setting);
        }

        // POST api/financecurrency
        [HttpPost]
        [Authorize]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financecurrency/5
        [HttpPut("{id}")]
        [Authorize]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financecurrency/5
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, String strCurr)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_currency];";
            }

            strSQL += @"SELECT [CURR]
                              ,[NAME]
                              ,[SYMBOL]
                              ,[SYSFLAG]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_fin_currency] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && !String.IsNullOrEmpty(strCurr))
            {
                strSQL += " WHERE [t_fin_account_ctgy].[CURR] = N'" + strCurr + "'";
            }

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, FinanceCurrencyViewModel vm)
        {
            Int32 idx = 0;
            vm.Curr = reader.GetString(idx++);
            vm.Name = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.Symbol = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.SysFlag = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
        }
    }
}
