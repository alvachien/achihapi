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
    public class LearnCategoryController : Controller
    {
        // GET: api/learncategory
        [HttpGet]
        public IEnumerable<LearnCategoryViewModel> Get()
        {
            List<LearnCategoryViewModel> listVm = new List<LearnCategoryViewModel>();
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

                queryString = @"SELECT TOP (100) [ID]
                      ,[CTGYID]
                      ,[NAME]
                      ,[COMMENT]
                      ,[OWNER]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [achihdb].[dbo].[t_fin_account]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnCategoryViewModel vm = new LearnCategoryViewModel();
                        //onDB2VM(reader, vm);
                        listVm.Add(vm);
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

            return listVm;
        }

        // GET api/learncategory/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/learncategory
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/learncategory/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/learncategory/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
