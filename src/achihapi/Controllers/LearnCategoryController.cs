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
                queryString = @"SELECT TOP (1000) [ID]
                              ,[PARID]
                              ,[NAME]
                              ,[COMMENT]
                              ,[SYSFLAG]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_learn_ctgy]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnCategoryViewModel vm = new LearnCategoryViewModel();
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

        private void onDB2VM(SqlDataReader reader, LearnCategoryViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            if (!reader.IsDBNull(1))
                vm.ParID = reader.GetInt32(1);
            vm.Name = reader.GetString(2);
            if (!reader.IsDBNull(3))
                vm.Comment = reader.GetString(3);
            if (!reader.IsDBNull(4))
                vm.SysFlag = reader.GetBoolean(4);
            if (!reader.IsDBNull(5))
                vm.CreatedBy = reader.GetString(5);
            if (!reader.IsDBNull(6))
                vm.CreatedAt = reader.GetDateTime(6);
            if (!reader.IsDBNull(7))
                vm.UpdatedBy = reader.GetString(7);
            if (!reader.IsDBNull(8))
                vm.UpdatedAt = reader.GetDateTime(8);
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
