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
    public class LearnHistoryController : Controller
    {
        // GET: api/learnhistory
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            List<LearnHistoryViewModel> listVm = new List<LearnHistoryViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT TOP (1000) [USERID]
                          ,[OBJECTID]
                          ,[LEARNDATE]
                          ,[COMMENT]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_learn_hist]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnHistoryViewModel vm = new LearnHistoryViewModel();
                        onDB2VM(reader, vm);
                        listVm.Add(vm);
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

            return new ObjectResult(listVm);
        }

        private void onDB2VM(SqlDataReader reader, LearnHistoryViewModel vm)
        {
            vm.UserID = reader.GetString(0);
            vm.ObjectID = reader.GetInt32(1);
            vm.LearnDate = reader.GetDateTime(2);
            if (!reader.IsDBNull(3))
                vm.Comment = reader.GetString(3);
            if (!reader.IsDBNull(4))
                vm.CreatedBy = reader.GetString(4);
            if (!reader.IsDBNull(5))
                vm.CreatedAt = reader.GetDateTime(5);
            if (!reader.IsDBNull(6))
                vm.UpdatedBy = reader.GetString(6);
            if (!reader.IsDBNull(7))
                vm.UpdatedAt = reader.GetDateTime(7);
        }

        // GET api/learnhistory/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/learnhistory
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/learnhistory/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/learnhistory/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
