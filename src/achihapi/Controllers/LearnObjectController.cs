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
    public class LearnObjectController : Controller
    {
        // GET: api/learnobject
        [HttpGet]
        public IEnumerable<LearnObjectViewModel> Get()
        {
            List<LearnObjectViewModel> listVm = new List<LearnObjectViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";

            try
            {
                queryString = @"SELECT TOP(1000)[ID]
                      ,[CATEGORY]
                      ,[NAME]
                      ,[CONTENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                        FROM [dbo].[t_learn_obj]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnObjectViewModel vm = new LearnObjectViewModel();
                        onDB2VM(reader, vm);
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

        private void onDB2VM(SqlDataReader reader, LearnObjectViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            vm.CategoryID = reader.GetInt32(1);
            vm.Name = reader.GetString(2);
            if (!reader.IsDBNull(3))
                vm.Content = reader.GetString(3);
            if (!reader.IsDBNull(4))
                vm.CreatedBy = reader.GetString(4);
            if (!reader.IsDBNull(5))
                vm.CreatedAt = reader.GetDateTime(5);
            if (!reader.IsDBNull(6))
                vm.UpdatedBy = reader.GetString(6);
            if (!reader.IsDBNull(7))
                vm.UpdatedAt = reader.GetDateTime(7);
        }

        // GET api/learnobject/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/learnobject
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/learnobject/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/learnobject/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
