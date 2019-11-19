using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LanguageController : Controller
    {
        // GET: api/language
        [HttpGet]
        [Produces(typeof(List<LanguageViewModel>))]
        [ResponseCache(Duration = 18000)]
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
                        LanguageViewModel avm = new LanguageViewModel
                        {
                            Lcid = reader.GetInt32(0),
                            ISOName = reader.GetString(1),
                            EnglishName = reader.GetString(2),
                            NativeName = reader.GetString(3)
                        };
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
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listVMs, setting);
        }

        // GET api/language/5
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            return Forbid();
        }

        // POST api/language
        [HttpPost]
        public ActionResult Post([FromBody]string value)
        {
            return Forbid();
        }

        // PUT api/language/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }

        // DELETE api/language/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
