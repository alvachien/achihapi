using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/HomeMsg")]
    [Authorize]
    public class HomeMsgController : Controller
    {
        // GET: api/HomeMsg
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<HomeMsgViewModel> listVm = new BaseListViewModel<HomeMsgViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                if (usrObj == null)
                    return BadRequest();
                var usrName = usrObj.Value;
                if (String.IsNullOrEmpty(usrName))
                    return BadRequest();

                queryString = this.getQueryString(true, top, skip, hid, usrName);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                HomeMsgViewModel vm = new HomeMsgViewModel();
                                SqlUtility.HomeMsg_DB2VM(reader, vm);
                                listVm.Add(vm);
                            }
                        }
                    }
                    ++nRstBatch;

                    reader.NextResult();
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
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.DateFormatString = HIHAPIConstants.DateFormatPattern;
            setting.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(); ;
            return new JsonResult(listVm, setting);
        }

        // GET: api/HomeMsg/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(int id)
        {
            return BadRequest();
        }
        
        // POST: api/HomeMsg
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]string value)
        {
            return BadRequest();
        }

        // PUT: api/HomeMsg/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }

        #region Implementation methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32 hid, String usrName)
        {

            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_homemsg] WHERE [HID] = " + hid.ToString() + " AND [USERTO] = N'" + usrName + "'; ";
            }

            strSQL += SqlUtility.getHomeMsgQueryString(usrName, hid);

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && !String.IsNullOrEmpty(usrName))
            {
                strSQL += @" AND [t_homemsg].[USERTO] = " + usrName;
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("HomeMsgController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }
        #endregion
    }
}
