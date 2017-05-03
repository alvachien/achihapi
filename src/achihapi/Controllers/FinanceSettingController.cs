using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceSettingController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<FinanceSettingViewModel> listVm = new List<FinanceSettingViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrObj = User.FindFirst(c => c.Type == "sub");

                queryString = @"SELECT [SETID]
                        ,[SETVALUE]
                        ,[COMMENT]
                        ,[CREATEDBY]
                        ,[CREATEDAT]
                        ,[UPDATEDBY]
                        ,[UPDATEDAT]
                    FROM [dbo].[t_fin_setting]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceSettingViewModel avm = new FinanceSettingViewModel();
                        avm.SetID = reader.GetString(0);
                        avm.SetValue = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.Comment = reader.GetString(2);
                        if (!reader.IsDBNull(3))
                            avm.CreatedBy = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            avm.CreatedAt = reader.GetDateTime(4);
                        if (!reader.IsDBNull(5))
                            avm.UpdatedBy = reader.GetString(5);
                        if (!reader.IsDBNull(6))
                            avm.UpdatedAt = reader.GetDateTime(6);

                        listVm.Add(avm);
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
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(listVm, setting);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEntry([FromBody]FinanceSettingViewModel vm)
        {
            return NoContent();
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateEntry([FromBody]FinanceSettingViewModel vm)
        {
            return NoContent();
        }
    }
}
