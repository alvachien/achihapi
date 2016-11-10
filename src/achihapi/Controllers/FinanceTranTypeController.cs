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
    public class FinanceTranTypeController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<FinanceTranTypeViewModel> listVM = new List<ViewModels.FinanceTranTypeViewModel>();
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

                queryString = @"SELECT TOP (1000) [ID]
                              ,[NAME]
                              ,[EXPENSE]
                              ,[PARID]
                              ,[COMMENT]
                              ,[SYSFLAG]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_fin_tran_type]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceTranTypeViewModel avm = new FinanceTranTypeViewModel();
                        avm.ID = reader.GetInt32(0);
                        avm.Name = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.Expense = reader.GetBoolean(2);
                        if (!reader.IsDBNull(3))
                            avm.ParID = reader.GetInt32(3);
                        if (!reader.IsDBNull(4))
                            avm.Comment = reader.GetString(4);
                        if (!reader.IsDBNull(5))
                            avm.SysFlag = reader.GetBoolean(5);
                        if (!reader.IsDBNull(6))
                            avm.CreatedBy = reader.GetString(6);
                        if (!reader.IsDBNull(7))
                            avm.CreatedAt = reader.GetDateTime(7);
                        if (!reader.IsDBNull(8))
                            avm.UpdatedBy = reader.GetString(8);
                        if (!reader.IsDBNull(9))
                            avm.UpdatedAt = reader.GetDateTime(9);

                        listVM.Add(avm);
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

            return new ObjectResult(listVM);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
