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
    public class LearnAwardController : Controller
    {
        // GET: api/learnaward
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<LearnAwardViewModel> listVm = new List<LearnAwardViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT TOP (1000) [ID]
                              ,[USERID]
                              ,[ADATE]
                              ,[SCORE]
                              ,[REASON]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_learn_award]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LearnAwardViewModel vm = new LearnAwardViewModel();
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            return new JsonResult(listVm, setting);
        }

        private void onDB2VM(SqlDataReader reader, LearnAwardViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            vm.UserID = reader.GetString(1);
            vm.AwardDate = reader.GetDateTime(2);
            vm.Score = reader.GetInt32(3);
            vm.Reason = reader.GetString(4);
            if (!reader.IsDBNull(5))
                vm.CreatedBy = reader.GetString(5);
            if (!reader.IsDBNull(6))
                vm.CreatedAt = reader.GetDateTime(6);
            if (!reader.IsDBNull(7))
                vm.UpdatedBy = reader.GetString(7);
            if (!reader.IsDBNull(8))
                vm.UpdatedAt = reader.GetDateTime(8);
        }

        // GET api/learnaward/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/learnaward
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/learnaward/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/learnaward/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
