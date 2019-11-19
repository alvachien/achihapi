using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using achihapi.Utilities;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceADPDocumentController : Controller
    {
        private IMemoryCache _cache;
        public FinanceADPDocumentController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/financeadpdocument
        [HttpGet]
        public IActionResult Get([FromQuery]Int32 hid, Boolean skipPosted = true, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            return BadRequest();
        }

        // GET api/financeadpdocument/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get([FromRoute]int id, [FromQuery]Boolean isADP = true, Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

            String usrName = String.Empty;
            if (Startup.UnitTestMode)
                usrName = UnitTestUtility.UnitTestUser;
            else
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            FinanceADPDocumentUIViewModel vm = new FinanceADPDocumentUIViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = HIHDBUtility.getFinanceDocADPQueryString(id, hid, isADP);

                using(conn = new SqlConnection(Startup.DBConnectionString))
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

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
                    }

                    // Header
                    while (reader.Read())
                    {
                        HIHDBUtility.FinDocHeader_DB2VM(reader, vm);
                    }
                    reader.NextResult();

                    // Items
                    while (reader.Read())
                    {
                        FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                        HIHDBUtility.FinDocItem_DB2VM(reader, itemvm);

                        vm.Items.Add(itemvm);
                    }
                    reader.NextResult();

                    // Account
                    while (reader.Read())
                    {
                        FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
                        Int32 aidx = 0;
                        aidx = HIHDBUtility.FinAccountHeader_DB2VM(reader, vmAccount, aidx);
                        vmAccount.ExtraInfo_ADP = new FinanceAccountExtDPViewModel();
                        HIHDBUtility.FinAccountADP_DB2VM(reader, vmAccount.ExtraInfo_ADP, aidx);

                        vm.AccountVM = vmAccount;
                    }
                    reader.NextResult();

                    // Tmp docs
                    while (reader.Read())
                    {
                        FinanceTmpDocDPViewModel dpvm = new FinanceTmpDocDPViewModel();
                        HIHDBUtility.FinTmpDocADP_DB2VM(reader, dpvm);
                        vm.AccountVM.ExtraInfo_ADP.DPTmpDocs.Add(dpvm);
                    }
                    reader.NextResult();

                    // Tag
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Int32 itemID = reader.GetInt32(0);
                            String sterm = reader.GetString(1);

                            foreach (var vitem in vm.Items)
                            {
                                if (vitem.ItemID == itemID)
                                {
                                    vitem.TagTerms.Add(sterm);
                                }
                            }
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

            return new JsonResult(vm, setting);
        }

        // POST api/financeadpdocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceADPDocumentUIViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (vm == null || !(vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment
                || vm.DocType == FinanceDocTypeViewModel.DocType_AdvanceReceive))
            {
                return BadRequest("No data is inputted");
            }
            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");

            String usrName = String.Empty;
            if (Startup.UnitTestMode)
                usrName = UnitTestUtility.UnitTestUser;
            else
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;
            }
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            // Check the items
            if (vm.Items.Count <= 0 || vm.AccountVM == null || vm.AccountVM.ExtraInfo_ADP == null
                || vm.AccountVM.ExtraInfo_ADP.DPTmpDocs.Count <= 0 || vm.Items.Count != 1)
            {
                return BadRequest("No item or no account or no template docs");
            }
            // Check on the data
            if (vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment)
            {
                if (vm.Items[0].TranType != FinanceTranTypeViewModel.TranType_AdvancePaymentOut)
                    return BadRequest("Invalid tran. type for advance payment");
            }
            else if(vm.DocType == FinanceDocTypeViewModel.DocType_AdvanceReceive)
            {
                if (vm.Items[0].TranType != FinanceTranTypeViewModel.TranType_AdvanceReceiveIn)
                    return BadRequest("Invalid tran. type for advance receive");
            }
            foreach(var tmpdocitem in vm.AccountVM.ExtraInfo_ADP.DPTmpDocs)
            {
                if (!tmpdocitem.ControlCenterID.HasValue && !tmpdocitem.OrderID.HasValue)
                {
                    return BadRequest("Tmp Doc Item miss control center or order");
                }
                if (tmpdocitem.TranAmount == 0)
                {
                    return BadRequest("Tmp Doc Item miss amount");
                }
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewDocID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    tran = conn.BeginTransaction();

                    // First, craete the doc header => nNewDocID
                    queryString = HIHDBUtility.GetFinDocHeaderInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    cmd.Dispose();
                    cmd = null;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = HIHDBUtility.GetFinDocItemInsertString();
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinDocItemInsertParameter(cmd, ivm, nNewDocID);

                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;

                        // Tags
                        if (ivm.TagTerms.Count > 0)
                        {
                            // Create tags
                            foreach (var term in ivm.TagTerms)
                            {
                                queryString = HIHDBUtility.GetTagInsertString();

                                cmd = new SqlCommand(queryString, conn, tran);

                                HIHDBUtility.BindTagInsertParameter(cmd, vm.HID, HIHTagTypeEnum.FinanceDocumentItem, nNewDocID, term, ivm.ItemID);

                                await cmd.ExecuteNonQueryAsync();

                                cmd.Dispose();
                                cmd = null;
                            }
                        }
                    }

                    // Third, go to the account creation => nNewAccountID
                    queryString = HIHDBUtility.GetFinanceAccountHeaderInsertString();

                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    HIHDBUtility.BindFinAccountInsertParameter(cmd, vm.AccountVM, usrName);

                    SqlParameter idparam2 = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam2.Direction = ParameterDirection.Output;

                    nRst = await cmd.ExecuteNonQueryAsync();
                    Int32 nNewAccountID = (Int32)idparam2.Value;
                    cmd.Dispose();
                    cmd = null;

                    // 3a. Create another item to loan document
                    var nMaxItemID = vm.Items.Max(item => item.ItemID);
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        ivm.ItemID = ++nMaxItemID;
                        ivm.AccountID = nNewAccountID;
                        if (vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment)
                            ivm.TranType = FinanceTranTypeViewModel.TranType_OpeningAsset;
                        else if (vm.DocType == FinanceDocTypeViewModel.DocType_AdvanceReceive)
                            ivm.TranType = FinanceTranTypeViewModel.TranType_OpeningLiability;

                        queryString = HIHDBUtility.GetFinDocItemInsertString();
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinDocItemInsertParameter(cmd, ivm, nNewDocID);

                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;
                        break;
                    }

                    // Fourth, creat the ADP part
                    queryString = HIHDBUtility.GetFinanceAccountADPInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    HIHDBUtility.BindFinAccountADPInsertParameter(cmd, vm.AccountVM.ExtraInfo_ADP, nNewDocID, nNewAccountID, usrName);
                    nRst = await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Sixth, create template docs
                    foreach (FinanceTmpDocDPViewModel avm in vm.AccountVM.ExtraInfo_ADP.DPTmpDocs)
                    {
                        queryString = HIHDBUtility.getFinanceTmpDocADPInsertString();

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };

                        HIHDBUtility.bindFinTmpDocADPParameter(cmd, avm, nNewAccountID, usrName);
                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;
                    }

                    tran.Commit();

                    // Update the buffer - Account list
                    var cacheKey = String.Format(CacheKeys.FinAccountList, vm.HID, null);
                    this._cache.Remove(cacheKey);
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif

                if (tran != null)
                    tran.Rollback();

                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
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

            vm.ID = nNewDocID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // PUT api/financeadpdocument/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put([FromRoute]int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/financeadpdocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute]int id)
        {
            return BadRequest();
        }
    }
}
