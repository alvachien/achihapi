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
    public class ModuleController : Controller
    {
        // GET: api/module
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<ModuleViewModel> listVMs = new List<ModuleViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT TOP (1000) [MODULE]
                          ,[NAME]
                          ,[AUTHFLAG]
                          ,[TAGFLAG]
                      FROM [dbo].[t_module]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ModuleViewModel avm = new ModuleViewModel
                        {
                            Module = reader.GetString(0),
                            Name = reader.GetString(1)
                        };
                        if (!reader.IsDBNull(2))
                            avm.AuthFlag = reader.GetBoolean(2);
                        if (!reader.IsDBNull(3))
                            avm.TagFlag = reader.GetBoolean(3);

                        listVMs.Add(avm);
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(listVMs, setting);
        }

        // GET api/module/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/module
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/module/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/module/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
