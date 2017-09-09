using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class HomeMemberController : Controller
    {
        // GET: api/homemember
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0, Int32? hid = null)
        {
            BaseListViewModel<HomeMemViewModel> listVm = new BaseListViewModel<HomeMemViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                //String scopeFilter = String.Empty;
                //String usrName = "";
                //try
                //{
                //    var usrObj = HIHAPIUtility.GetUserClaim(this);
                //    usrName = usrObj.Value;
                //    var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                //    scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                //}
                //catch
                //{
                //    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                //}

                queryString = this.getQueryString(true, top, skip, null, hid);

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
                                HomeMemViewModel vm = new HomeMemViewModel();
                                SqlUtility.HomeMem_DB2VM(reader, vm);
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(listVm, setting);
        }

        // GET api/homemember/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int hid, String struser)
        {
            HomeMemViewModel vm = new HomeMemViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bExist = false;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                String scopeFilter = String.Empty;
                //String usrName = "";
                //try
                //{
                //    var usrObj = HIHAPIUtility.GetUserClaim(this);
                //    usrName = usrObj.Value;
                //    var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);


                //    scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                //}
                //catch
                //{
                //    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                //}

                queryString = this.getQueryString(false, null, null, struser, hid);

                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bExist = true;
                    while (reader.Read())
                    {
                        SqlUtility.HomeMem_DB2VM(reader, vm);

                        // It should return one entry only!
                        // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;

                        break;
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

            // In case not found, return a 404
            if (!bExist)
                return NotFound();
            else if (bError)
                return StatusCode(500, strErrMsg);

            // Only return the meaningful object
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
        }

        // POST api/homemember
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/homemember/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/homemember/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        #region Implementation methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, String strSearchUser, Int32? hid)
        {

            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_homemem] ";
                if (hid.HasValue)
                {
                    strSQL += " WHERE [HID] = " + hid.Value.ToString();
                }
                strSQL += " ;";
            }

            strSQL += SqlUtility.getHomeMemQueryString(hid);

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && !String.IsNullOrEmpty(strSearchUser))
            {
                if (hid.HasValue)
                {
                    strSQL += @" AND [t_homemem].[USER] = " + strSearchUser;
                }
                else
                {
                    strSQL += @" WHERE [t_homemem].[USER] = " + strSearchUser;
                }
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("HomeMemberController, SQL generated: " + strSQL);
#endif

            return strSQL;
        }
        #endregion
    }
}
