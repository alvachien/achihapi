using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Get()
        {
            List<FinanceCurrencyViewModel> listVMs = new List<FinanceCurrencyViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrObj = User.FindFirst(c => c.Type == "sub");

                queryString = @"SELECT TOP (1000) [CURR]
                              ,[NAME]
                              ,[SYMBOL]
                              ,[SYSFLAG]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_fin_currency]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceCurrencyViewModel avm = new FinanceCurrencyViewModel();
                        avm.Curr = reader.GetString(0);
                        avm.Name = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.Symbol = reader.GetString(2);
                        if (!reader.IsDBNull(3))
                            avm.SysFlag = reader.GetBoolean(3);
                        if (!reader.IsDBNull(4))
                            avm.CreatedBy = reader.GetString(4);
                        if (!reader.IsDBNull(5))
                            avm.CreatedAt = reader.GetDateTime(5);
                        if (!reader.IsDBNull(6))
                            avm.UpdatedBy = reader.GetString(6);
                        if (!reader.IsDBNull(7))
                            avm.UpdatedAt = reader.GetDateTime(7);

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
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(listVMs);
        }

        // GET api/financecurrency/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/financecurrency
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financecurrency/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financecurrency/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
