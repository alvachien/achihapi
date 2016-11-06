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
    public class FinanceDocTypeController : Controller
    {
        // GET: api/financedoctype
        [HttpGet]
        public IEnumerable<FinanceDocTypeViewModel> Get()
        {
            List<FinanceDocTypeViewModel> listVMs = new List<FinanceDocTypeViewModel>();
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

                queryString = @"SELECT TOP (1000) [ID]
                              ,[NAME]
                              ,[COMMENT]
                              ,[SYSFLAG]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_fin_doc_type]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceDocTypeViewModel avm = new FinanceDocTypeViewModel();
                        avm.ID = reader.GetInt16(0);
                        avm.Name = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.Comment = reader.GetString(2);
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
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return listVMs;
        }

        // GET api/financedoctype/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/financedoctype
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financedoctype/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financedoctype/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
