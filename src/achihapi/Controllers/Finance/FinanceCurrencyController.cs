using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data.SqlClient;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceCurrencyController : Controller
    {
        private IMemoryCache _cache;

        public FinanceCurrencyController(IMemoryCache cache)
        {
            this._cache = cache;
        }

        // GET: api/financecurrency
        [HttpGet]
        [Authorize]
        [Produces(typeof(List<FinanceCurrencyViewModel>))]
        public async Task<IActionResult> Get()
        {
            // For Currency, the paging is not supported.
            // It is full buffered;

            List<FinanceCurrencyViewModel> listVMs = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                if (_cache.TryGetValue<List<FinanceCurrencyViewModel>>(CacheKeys.FinCurrency, out listVMs))
                { 
                    // DO nothing!
                }
                else
                {
                    queryString = @"SELECT[CURR]
                              ,[NAME]
                              ,[SYMBOL]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                            FROM[dbo].[t_fin_currency] ";

                    listVMs = new List<FinanceCurrencyViewModel>();
                    using (conn = new SqlConnection(Startup.DBConnectionString))
                    {
                        await conn.OpenAsync();
                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();

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

                    // Buffer it for 20 minutes
                    _cache.Set<List<FinanceCurrencyViewModel>>(CacheKeys.FinCurrency, listVMs, TimeSpan.FromSeconds(1200));
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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

        // GET api/financecurrency/5
        [HttpGet("{id}")]
        public IActionResult Get([FromRoute]string sid)
        {
            return BadRequest();
        }

        // POST api/financecurrency
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]string value)
        {
            return BadRequest();
        }

        // PUT api/financecurrency/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put([FromRoute] int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/financecurrency/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }

        #region Implement methods
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
                strSQL += " WHERE [t_fin_currency].[CURR] = N'" + strCurr + "'";
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
        #endregion
    }
}
