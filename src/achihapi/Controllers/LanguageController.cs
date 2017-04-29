using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class LanguageController : Controller
    {
        // GET: api/language
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<LanguageViewModel> listVMs = new List<LanguageViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = @"SELECT [LCID]
                              ,[ISONAME]
                              ,[ENNAME]
                              ,[NAVNAME]
                              ,[APPFLAG]
                          FROM [dbo].[t_language]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LanguageViewModel avm = new LanguageViewModel();
                        avm.Lcid = reader.GetInt32(0);
                        avm.ISOName = reader.GetString(1);
                        avm.EnglishName = reader.GetString(2);
                        avm.NativeName = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            avm.AppFlag = reader.GetBoolean(4);

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

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            return new JsonResult(listVMs, setting);
        }

        // GET api/language/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/language
        [HttpPost]
        public void Post([FromBody]string value)
        {
           
        }

        // PUT api/language/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/language/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
