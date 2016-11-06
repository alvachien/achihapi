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
    public class FinanceOrderController : Controller
    {
        // GET: api/financeorder
        [HttpGet]
        public IEnumerable<FinanceOrderViewModel> Get()
        {
            List<FinanceOrderViewModel> listVMs = new List<FinanceOrderViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrObj = User.FindFirst(c => c.Type == "sub");

                queryString = @"SELECT TOP (1000)[ID]
                              ,[NAME]
                              ,[VALID_FROM]
                              ,[VALID_TO]
                              ,[COMMENT]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_fin_order]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceOrderViewModel avm = new FinanceOrderViewModel();
                        avm.ID = reader.GetInt32(0);
                        avm.Name = reader.GetString(1);
                        avm.Valid_From = reader.GetDateTime(2);
                        avm.Valid_To = reader.GetDateTime(3);
                        if (!reader.IsDBNull(4))
                            avm.Comment = reader.GetString(4);
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
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return listVMs;
        }

        // GET api/financeorder/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/financeorder
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financeorder/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financeorder/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
