using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceAccountCategoryController : Controller
    {
        // GET: api/financeaccountcategory
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<FinanceAccountCtgyViewModel> listVMs = new List<FinanceAccountCtgyViewModel>();
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

                queryString = @"SELECT [ID]
                          ,[NAME]
                          ,[ASSETFLAG]
                          ,[COMMENT]
                          ,[SYSFLAG]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_fin_account_ctgy]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceAccountCtgyViewModel avm = new FinanceAccountCtgyViewModel();
                        avm.ID = reader.GetInt32(0);
                        avm.Name = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.AssetFlag = reader.GetBoolean(2);
                        if (!reader.IsDBNull(3))
                            avm.Comment = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            avm.SysFlag = reader.GetBoolean(4);
                        if (!reader.IsDBNull(5))
                            avm.CreatedBy = reader.GetString(5);
                        if (!reader.IsDBNull(6))
                            avm.CreatedAt = reader.GetDateTime(6);
                        if (!reader.IsDBNull(7))
                            avm.UpdatedBy = reader.GetString(7);
                        if (!reader.IsDBNull(8))
                            avm.UpdatedAt = reader.GetDateTime(8);

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

        // GET api/financeaccountcateogry/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/financeaccountcateogry
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financeaccountcateogry/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financeaccountcateogry/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
