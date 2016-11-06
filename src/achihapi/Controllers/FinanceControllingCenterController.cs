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
    public class FinanceControllingCenterController : Controller
    {
        // GET: api/financecontrollingcenter
        [HttpGet]
        public IEnumerable<FinanceControlCenterViewModel> Get()
        {
            List<FinanceControlCenterViewModel> listVMs = new List<FinanceControlCenterViewModel>();
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
                              ,[PARID]
                              ,[COMMENT]
                              ,[OWNER]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_fin_controlcenter]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceControlCenterViewModel avm = new FinanceControlCenterViewModel();
                        avm.ID = reader.GetInt32(0);
                        avm.Name = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.ParID = reader.GetInt32(2);
                        if (!reader.IsDBNull(3))
                            avm.Comment = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            avm.Owner = reader.GetString(4);
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

        // GET api/financecontrollingcenter/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/financecontrollingcenter
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financecontrollingcenter/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financecontrollingcenter/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
