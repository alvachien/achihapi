using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LearnAwardController : Controller
    {
        // GET: api/learnaward
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<LearnAwardViewModel> listVm = new List<LearnAwardViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

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

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();
                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
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
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest(strErrMsg);
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

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
        public IActionResult Get(int id)
        {
            return Forbid();
        }

        // POST api/learnaward
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return Forbid();
        }

        // PUT api/learnaward/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }

        // DELETE api/learnaward/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
