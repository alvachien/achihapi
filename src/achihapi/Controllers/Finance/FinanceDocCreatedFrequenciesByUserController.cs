using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data.SqlClient;
using achihapi.Utilities;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceDocCreatedFrequenciesByUserController : ControllerBase
    {
        private IMemoryCache _cache;
        public FinanceDocCreatedFrequenciesByUserController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/FinanceDocCreatedFrequenciesByUser
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid = 0, Int32 nfrqtype = 1)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");

            FinanceDocCreatedFrequencyType frqtype = FinanceDocCreatedFrequencyType.Weekly;
            DateTime dtBgn = DateTime.Today;
            if (nfrqtype == 1)
            {
                frqtype = FinanceDocCreatedFrequencyType.Weekly;
                dtBgn = dtBgn.AddMonths(-1);
            }
            else if (nfrqtype == 2)
            {
                frqtype = FinanceDocCreatedFrequencyType.Monthly;
                dtBgn = dtBgn.AddYears(-1);
            }
            else
                return BadRequest("Frequence type is not supported");

            String usrName = "";
            String scopeFilter = String.Empty;
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (String.IsNullOrEmpty(usrName))
                return BadRequest("No user found");

            List<FinanceDocCreatedFrequenciesByUserViewModel> listVm = new List<FinanceDocCreatedFrequenciesByUserViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinDocCrtFrqByUser, hid);
                if (_cache.TryGetValue<List<FinanceDocCreatedFrequenciesByUserViewModel>>(cacheKey, out listVm))
                {
                    // Do nothing
                }
                else
                {
                    listVm = new List<FinanceDocCreatedFrequenciesByUserViewModel>();

                    using (conn = new SqlConnection(Startup.DBConnectionString))
                    {
                        await conn.OpenAsync();

                        // Check Home assignment with current user
                        try
                        {
                            HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                        }
                        catch (Exception)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw;
                        }

                        queryString = HIHDBUtility.getFinDocCreatedFrequenciesByUserQueryString(frqtype, hid, dtBgn, DateTime.Today);

#if DEBUG
                        System.Diagnostics.Debug.WriteLine("FinanceDocCreatedFrequenciesByUserController, SQL generated: " + queryString);
#endif

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            FinanceDocCreatedFrequenciesByUserViewModel vm = new FinanceDocCreatedFrequenciesByUserViewModel();
                            HIHDBUtility.FinDocCreatedFrequenciesByUser_DB2VM(frqtype, reader, vm);
                            listVm.Add(vm);
                        }
                    }

                    _cache.Set<List<FinanceDocCreatedFrequenciesByUserViewModel>>(cacheKey, listVm, TimeSpan.FromMinutes(20));
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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
    }
}
