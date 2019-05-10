using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using achihapi.Utilities;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FinanceAccountController : Controller
    {
        private IMemoryCache _cache;
        public FinanceAccountController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/financeaccount
        [HttpGet]
        [Authorize]
        [Produces(typeof(List<FinanceAccountUIViewModel>))]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Byte? status = null)
        {
            if (hid <= 0)
                return BadRequest("HID is missing");
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
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                    //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (String.IsNullOrEmpty(usrName))
                return BadRequest("No user found");

            List<FinanceAccountUIViewModel> listVm = null;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var cacheKey = String.Format(CacheKeys.FinAccountList, hid, status);
                if (_cache.TryGetValue<List<FinanceAccountUIViewModel>>(cacheKey, out listVm))
                {
                    // Do nothing
                }
                else
                {
                    listVm = new List<FinanceAccountUIViewModel>();

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

                        queryString = HIHDBUtility.getFinanceAccountHeaderQueryString(hid, status, String.Empty);

                        cmd = new SqlCommand(queryString, conn);
                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            FinanceAccountUIViewModel vm = new FinanceAccountUIViewModel();
                            HIHDBUtility.FinAccountHeader_DB2VM(reader, vm, 0);
                            listVm.Add(vm);
                        }
                    }

                    _cache.Set<List<FinanceAccountUIViewModel>>(cacheKey, listVm, TimeSpan.FromMinutes(20));
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

        // GET api/financeaccount/5
        [HttpGet("{id}")]
        [Authorize]
        [Produces(typeof(FinanceAccountUIViewModel))]
        public async Task<IActionResult> Get([FromRoute]int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

            FinanceAccountUIViewModel vmAccount = null;
            SqlConnection conn = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
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
                        //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                        //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                    }
                }
                catch
                {
                    return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
                }
                if (String.IsNullOrEmpty(usrName))
                {
                    return BadRequest("No user found");
                }

                var cacheKey = String.Format(CacheKeys.FinAccount, hid, id);
                if (_cache.TryGetValue<FinanceAccountUIViewModel>(cacheKey, out vmAccount))
                {
                    // Do nothing
                }
                else
                {
                    vmAccount = new FinanceAccountUIViewModel();
                    queryString = this.getQueryString(false, null, null, null, id, scopeFilter, null);

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

                        // Perform the reading
                        try
                        {
                            await this.readWholeAccountInfo(conn, hid, id, scopeFilter, vmAccount, true);
                        }
                        catch(Exception)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw;
                        }
                    }

                    _cache.Set<FinanceAccountUIViewModel>(cacheKey, vmAccount, TimeSpan.FromMinutes(20));
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

            // Only return the meaningful object
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vmAccount, setting);
        }

        // POST api/financeaccount
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAccountViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (vm == null 
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvanceReceive
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom
                || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
            {
                return BadRequest("No data is inputted or inputted data for Advance payment/receive/Loan/Asset");
            }

            if (!vm.IsValid())
            {
                return BadRequest("Data is invalid");
            }

            String usrName = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                    //if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                    //{
                    //    return StatusCode(401, "Current user has no authority to create account!");
                    //}
                    //else if(String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                    //{
                    //    if (String.CompareOrdinal(vm.Owner, usrName) != 0)
                    //    {
                    //        return StatusCode(401, "Current user can only create account with owner.");
                    //    }
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewAccountID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            HomeDefViewModel vmHome = null;

            try
            {
                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_account] WHERE [HID] = " + vm.HID.ToString() + " AND [Name] = N'" + vm.Name + "'";

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check HID assignment
                    if (vm.CtgyID != FinanceAccountCtgyViewModel.AccountCategory_Asset)
                    {
                        try
                        {
                            HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                        }
                        catch (Exception)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw;
                        }
                    }
                    else
                    {
                        // Check: HID, it requires more info than just check, so it implemented it 
                        String strHIDCheck = HIHDBUtility.getHomeDefQueryString() + " WHERE [ID]= @hid AND [USER] = @user";
                        cmd = new SqlCommand(strHIDCheck, conn);
                        cmd.Parameters.AddWithValue("@hid", vm.HID);
                        cmd.Parameters.AddWithValue("@user", usrName);
                        reader = await cmd.ExecuteReaderAsync();
                        if (!reader.HasRows)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw new Exception("Not home found!");
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                vmHome = new HomeDefViewModel();
                                HIHDBUtility.HomeDef_DB2VM(reader, vmHome);

                                // It shall be only one entry if found!
                                break;
                            }
                        }

                        reader.Dispose();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;

                        if (vmHome == null || String.IsNullOrEmpty(vmHome.BaseCurrency) || vmHome.ID != vm.HID)
                        {
                            errorCode = HttpStatusCode.BadRequest;
                            throw new Exception("Home Definition is invalid");
                        }
                    }

                    // Check duplicate names
                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Int32 nDuplicatedID = -1;
                        while (reader.Read())
                        {
                            nDuplicatedID = reader.GetInt32(0);
                            break;
                        }

                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Account already exists: " + nDuplicatedID.ToString());
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                        {
                            //// Create faked 'buyin' doc manually
                            //FinanceDocumentUIViewModel vmFIDOC = new FinanceDocumentUIViewModel();
                            //Int32 nNewDocID = 0;
                            //vmFIDOC.Desp = vm.Comment;
                            //vmFIDOC.DocType = FinanceDocTypeViewModel.DocType_Normal;
                            //vmFIDOC.HID = vm.HID;
                            //vmFIDOC.TranCurr = vmHome.BaseCurrency;
                            //vmFIDOC.TranDate = vm.ExtraInfo_AS.AssetStartDate.Value;
                            //vmFIDOC.CreatedAt = DateTime.Now;

                            //FinanceDocumentItemUIViewModel vmItem = new FinanceDocumentItemUIViewModel
                            //{
                            //    AccountID = -1  // For passing the check
                            //};
                            //if (vm.ExtraInfo_AS.ControlCenterID.HasValue)
                            //    vmItem.ControlCenterID = vm.ExtraInfo_AS.ControlCenterID.Value;
                            //if (vm.ExtraInfo_AS.OrderID.HasValue)
                            //    vmItem.OrderID = vm.ExtraInfo_AS.OrderID.Value;
                            //vmItem.Desp = vm.Comment;
                            //vmItem.ItemID = 1;
                            //vmItem.TranAmount = vm.ExtraInfo_AS.AssetValueInBaseCurrency.Value;
                            //vmItem.TranType = FinanceTranTypeViewModel.TranType_OpeningAsset;
                            //vmFIDOC.Items.Add(vmItem);

                            //// Do the checks
                            //try
                            //{
                            //    await FinanceDocumentController.FinanceDocumentBasicCheckAsync(vmFIDOC);
                            //}
                            //catch (Exception)
                            //{
                            //    errorCode = HttpStatusCode.BadRequest;
                            //    throw;
                            //}

                            //// Do the validation
                            //try
                            //{
                            //    await FinanceDocumentController.FinanceDocumentBasicValidationAsync(vmFIDOC, conn, -1);
                            //}
                            //catch (Exception)
                            //{
                            //    errorCode = HttpStatusCode.BadRequest;
                            //    throw;
                            //}

                            //// Begin the transaction
                            //tran = conn.BeginTransaction();

                            //// 1. Create Account Header
                            //queryString = HIHDBUtility.GetFinanceAccountHeaderInsertString();

                            //cmd = new SqlCommand(queryString, conn)
                            //{
                            //    Transaction = tran
                            //};

                            //HIHDBUtility.BindFinAccountInsertParameter(cmd, vm, usrName);
                            //SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                            //idparam.Direction = ParameterDirection.Output;

                            //Int32 nRst = await cmd.ExecuteNonQueryAsync();
                            //nNewAccountID = (Int32)idparam.Value;

                            //// 2. Create the Doc header
                            //queryString = HIHDBUtility.GetFinDocHeaderInsertString();
                            //cmd = new SqlCommand(queryString, conn)
                            //{
                            //    Transaction = tran
                            //};

                            //HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, vmFIDOC, usrName);
                            //idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                            //idparam.Direction = ParameterDirection.Output;

                            //nRst = await cmd.ExecuteNonQueryAsync();
                            //nNewDocID = (Int32)idparam.Value;
                            //vmFIDOC.ID = nNewDocID;
                            //cmd.Dispose();
                            //cmd = null;

                            //// 3. Create the Doc items
                            //foreach (FinanceDocumentItemUIViewModel ivm in vmFIDOC.Items)
                            //{
                            //    ivm.AccountID = nNewAccountID;
                            //    queryString = HIHDBUtility.GetFinDocItemInsertString();

                            //    cmd = new SqlCommand(queryString, conn)
                            //    {
                            //        Transaction = tran
                            //    };
                            //    HIHDBUtility.BindFinDocItemInsertParameter(cmd, ivm, nNewDocID);

                            //    await cmd.ExecuteNonQueryAsync();
                            //    cmd.Dispose();
                            //    cmd = null;
                            //}

                            //// 4. Create Account Ext for Asset 
                            //vm.ExtraInfo_AS.AccountID = nNewAccountID;
                            //vm.ExtraInfo_AS.RefDocForBuy = nNewDocID;
                            //queryString = HIHDBUtility.GetFinanceAccountAssetInsertString();
                            //cmd = new SqlCommand(queryString, conn)
                            //{
                            //    Transaction = tran
                            //};

                            //HIHDBUtility.BindFinAccountAssetInsertParameter(cmd, vm.ExtraInfo_AS);
                            //nRst = await cmd.ExecuteNonQueryAsync();
                            //cmd.Dispose();
                            //cmd = null;
                        }
                        else
                        {
                            // Begin the transaction
                            tran = conn.BeginTransaction();

                            // Now go ahead for the creating
                            queryString = HIHDBUtility.GetFinanceAccountHeaderInsertString();

                            cmd = new SqlCommand(queryString, conn)
                            {
                                Transaction = tran
                            };

                            HIHDBUtility.BindFinAccountInsertParameter(cmd, vm, usrName);
                            SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                            idparam.Direction = ParameterDirection.Output;

                            Int32 nRst = await cmd.ExecuteNonQueryAsync();
                            nNewAccountID = (Int32)idparam.Value;
                        }

                        // Now commit it!
                        tran.Commit();

                        // Update the buffer
                        // Account List
                        try
                        {
                            var cacheKey = String.Format(CacheKeys.FinAccountList, vm.HID, null);
                            this._cache.Remove(cacheKey);
                        }
                        catch(Exception)
                        {
                            // Do nothing here.
                        }
                        // B.S.
                        try
                        {
                            var cacheKey = String.Format(CacheKeys.FinReportBS, vm.HID);
                            this._cache.Remove(cacheKey);
                        }
                        catch (Exception)
                        {
                            // Do nothing here.
                        }
                    }
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

            vm.ID = nNewAccountID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PUT api/financeaccount/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put([FromRoute]int id, [FromBody]FinanceAccountViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (vm == null || vm.HID <= 0 || vm.ID != id)
            {
                return BadRequest("Invalid inputted data, such as miss HID");
            }

            String usrName = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);

                    //if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                    //{
                    //    return StatusCode(401, "Current user has no authority to change account!");
                    //}
                    //else if (String.CompareOrdinal(scopeObj.Value, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                    //{
                    //    if (String.CompareOrdinal(vm.Owner, usrName) != 0)
                    //    {
                    //        return StatusCode(401, "Current user can only modify account with owner.");
                    //    }
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (!vm.IsValid())
            {
                return BadRequest("Data is invalid!");
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String strErrMsg = "";
            SqlTransaction tran = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
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

                    // 0. Read out the old account
                    var vmOld = new FinanceAccountUIViewModel();
                    try
                    {
                        await this.readWholeAccountInfo(conn, vm.HID, id, usrName, vmOld, false);
                    }
                    catch(Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    // 1. Account header
                    var listsqls = new List<String>();
                    var hdrsql = FinanceAccountViewModel.WorkoutDeltaStringForUpdate(vmOld, vm, usrName);
                    if (!String.IsNullOrEmpty(hdrsql)) listsqls.Add(hdrsql);
                    if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvanceReceive)
                    {
                        if (vm.ExtraInfo_ADP.AccountID <= 0)
                            vm.ExtraInfo_ADP.AccountID = vmOld.ExtraInfo_ADP.AccountID;
                        var itemsql = FinanceAccountExtDPViewModel.WorkoutDeltaStringForUpdate(vmOld.ExtraInfo_ADP,
                            vm.ExtraInfo_ADP);
                        if (!String.IsNullOrEmpty(itemsql)) listsqls.Add(itemsql);
                    }
                    if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                    {
                        if (vm.ExtraInfo_AS.AccountID <= 0)
                            vm.ExtraInfo_AS.AccountID = vmOld.ExtraInfo_AS.AccountID;
                        var itemsql = FinanceAccountExtASViewModel.WorkoutDeltaStringForUpdate(vmOld.ExtraInfo_AS,
                            vm.ExtraInfo_AS);
                        if (!String.IsNullOrEmpty(itemsql)) listsqls.Add(itemsql);
                    }
                    if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
                    {
                        if (vm.ExtraInfo_Loan.AccountID <= 0)
                            vm.ExtraInfo_Loan.AccountID = vmOld.ExtraInfo_Loan.AccountID;
                        var itemsql = FinanceAccountExtLoanViewModel.WorkoutDeltaStringForUpdate(vmOld.ExtraInfo_Loan,
                            vm.ExtraInfo_Loan);
                        if (!String.IsNullOrEmpty(itemsql)) listsqls.Add(itemsql);
                    }
                    if (listsqls.Count == 0)
                    {
                        return BadRequest("Nothing need be updated");
                    }

                    // Then, do the updates
                    tran = conn.BeginTransaction();

                    foreach (var isql in listsqls)
                    {
                        if (!String.IsNullOrEmpty(isql))
                        {
                            cmd = new SqlCommand(isql, conn, tran);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }
                    }

                    // Now commit it!
                    tran.Commit();

                    // Update the buffer - Account List
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinAccountList, vm.HID, null);
                        this._cache.Remove(cacheKey);

                        cacheKey = String.Format(CacheKeys.FinAccount, vm.HID, id);
                        this._cache.Remove(cacheKey);
                    }
                    catch (Exception)
                    {
                        // Do nothing here.
                    }
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PATCH api/financeaccount/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch([FromRoute]int id, [FromQuery]int hid, [FromBody]JsonPatchDocument<FinanceAccountUIViewModel> patch)
        {
            if (patch == null || id <= 0)
                return BadRequest("No data is inputted");
            if (hid <= 0)
                return BadRequest("No home is inputted");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

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
            //FinanceAccountViewModel vm = new FinanceAccountViewModel();
            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();

            try
            {
                queryString = this.getQueryString(false, null, null, null, id, String.Empty, null);

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

                    // Read the account
                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HIHDBUtility.FinAccountHeader_DB2VM(reader, vmAccount, 0);

                            // It should return one entry only!
                            // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;

                            break;
                        }
                    }
                    else
                    {
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
                    }
                    reader.Close();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Optimization logic for Status change
                    if (patch.Operations.Count == 1 && patch.Operations[0].path == "/status")
                    {
                        FinanceAccountStatus newstatus = (FinanceAccountStatus)Byte.Parse((string)patch.Operations[0].value);

                        // Need check the balance
                        if (newstatus == FinanceAccountStatus.Closed)
                        {
                            queryString = @"SELECT [balance]
                                          FROM [dbo].[v_fin_report_bs] WHERE [HID] = " + hid.ToString() + " AND [ACCOUNTID] = " + id.ToString();
                            var cmdbal = new SqlCommand(queryString, conn);
                            var readerbal = await cmdbal.ExecuteReaderAsync();
                            Decimal dmcbal = 0;
                            var chkfail = false;

                            if (readerbal.HasRows)
                            {
                                while (readerbal.Read())
                                {
                                    dmcbal = readerbal.GetDecimal(0);

                                    if (Math.Abs(dmcbal) > 0.02M)
                                    {
                                        chkfail = true;
                                    }

                                    break;
                                }
                            }
                            else
                            {
                                chkfail = true;
                            }

                            readerbal.Close();
                            readerbal = null;
                            cmdbal.Dispose();
                            cmdbal = null;

                            if (chkfail)
                            {
                                throw new Exception("Balance is not zero!");
                            }
                        }

                        // Only update the status
                        tran = conn.BeginTransaction();

                        queryString = HIHDBUtility.GetFinanceAccountStatusUpdateString();
                        cmd = new SqlCommand(queryString, conn, tran);

                        vmAccount.Status = newstatus;
                        HIHDBUtility.BindFinAccountStatusUpdateParameter(cmd, newstatus, id, hid, usrName);
                        await cmd.ExecuteNonQueryAsync();

                        if (newstatus == FinanceAccountStatus.Closed)
                        {
                            // Close account.
                            if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment)
                            {
                                // It have to stop all unposted advance payment
                                queryString = HIHDBUtility.GetFinanceDocADPDeleteString(hid, vmAccount.ID, true);
                                SqlCommand cmdTmpDoc = new SqlCommand(queryString, conn, tran);
                                await cmdTmpDoc.ExecuteNonQueryAsync();
                            }
                            else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvanceReceive)
                            {
                                // It have to stop all unposted advance payment
                                queryString = HIHDBUtility.GetFinanceDocADPDeleteString(hid, vmAccount.ID, true);
                                SqlCommand cmdTmpDoc = new SqlCommand(queryString, conn, tran);
                                await cmdTmpDoc.ExecuteNonQueryAsync();
                            }
                            else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                            {
                                // For asset
                            }
                            else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom
                                || vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
                            {
                                // It have to stop all unposted advance payment
                                queryString = HIHDBUtility.GetFinanceDocLoanDeleteString(hid, vmAccount.ID, true);
                                SqlCommand cmdTmpDoc = new SqlCommand(queryString, conn, tran);
                                await cmdTmpDoc.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                // Normal case
                            }
                        }

                        tran.Commit();

                        // Update the buffer
                        // Account List
                        try
                        {
                            var cacheKey = String.Format(CacheKeys.FinAccountList, hid, null);
                            this._cache.Remove(cacheKey);

                            cacheKey = String.Format(CacheKeys.FinAccount, hid, id);
                            this._cache.Remove(cacheKey);
                        }
                        catch (Exception)
                        {
                            // Do nothing here.
                        }
                    }
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vmAccount, setting);
        }

        // DELETE api/financeaccount/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, [FromQuery]Int32 hid = 0)
        {
            String usrName = "";
            String scopeValue = "";
            try
            {
                if (Startup.UnitTestMode)
                    usrName = UnitTestUtility.UnitTestUser;
                else
                {
                    var usrObj = HIHAPIUtility.GetUserClaim(this);
                    usrName = usrObj.Value;
                    //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.FinanceAccountScope);
                    //scopeValue = scopeObj.Value;

                    //if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
                    //{
                    //    return StatusCode(401, "Current user has no authority to create account!");
                    //}
                }
            }
            catch
            {
                return BadRequest("Not valid HTTP HEAD: User and Scope Failed!");
            }

            if (String.IsNullOrEmpty(usrName))
                return BadRequest("No user found");
            if (hid == 0)
                return BadRequest("No HID inputted!");

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            SqlTransaction tran = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                // Check owner and the existence
                queryString = @"SELECT [OWNER] FROM [dbo].[t_fin_account] WHERE [ID] = " + id.ToString() + " AND [HID] = " + hid.ToString();
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
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            String strOwner = reader.GetString(0);
                            if (String.CompareOrdinal(scopeValue, HIHAPIConstants.OnlyOwnerFullControl) == 0)
                            {
                                if (String.CompareOrdinal(strOwner, usrName) != 0)
                                {
                                    errorCode = HttpStatusCode.BadRequest;
                                    throw new Exception("Current user can only delete the account with owner");
                                }
                            }

                            break;
                        }
                    }
                    else
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Account not exist!");
                    }
                    reader.Dispose();
                    cmd.Dispose();

                    // Deletion
                    queryString = @"DELETE FROM [dbo].[t_fin_account] WHERE [ID] = @ID";

                    tran = conn.BeginTransaction();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();

                    // Ext. info
                    queryString = @"DELETE FROM [dbo].[t_fin_account_ext_dp] WHERE [ACCOUNTID] = @ACCOUNTID";
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    cmd.Parameters.AddWithValue("@ACCOUNTID", id);
                    await cmd.ExecuteNonQueryAsync();

                    // Now commit it!
                    tran.Commit();

                    // Update the buffer
                    // Account List
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinAccountList, hid, null);
                        this._cache.Remove(cacheKey);

                        cacheKey = String.Format(CacheKeys.FinAccount, hid, id);
                        this._cache.Remove(cacheKey);
                    }
                    catch (Exception)
                    {
                        // Do nothing here.
                    }
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

            return Ok();
        }

        #region Implemented methods
        private string getQueryString(Boolean bListMode, Byte? status, Int32? nTop, Int32? nSkip, Int32? nSearchID, String strOwner, Int32? hid)
        {
            
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_account] WHERE [hid] = " + hid.Value.ToString();
                if (status.HasValue)
                {
                    if (status.Value == 0)
                        strSQL += " AND ( [STATUS] = 0 OR [STATUS] IS NULL) ";
                    else
                        strSQL += " AND [STATUS] = " + status.Value.ToString();
                }
                    
                if (!String.IsNullOrEmpty(strOwner))
                    strSQL += " AND [OWNER] = N'" + strOwner + "'";
                strSQL += ";";
            }

            //strSQL += HIHDBUtility.getFinanceAccountQueryString(hid, status, strOwner);
            strSQL += HIHDBUtility.getFinanceAccountHeaderQueryString(hid, status, strOwner);

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                if (!String.IsNullOrEmpty(strOwner))
                {
                    strSQL += @" AND [t_fin_account].[ID] = " + nSearchID.Value.ToString();
                }
                else
                {
                    strSQL += @" WHERE [t_fin_account].[ID] = " + nSearchID.Value.ToString();
                }
            }

            return strSQL;
        }

        private async Task<IActionResult> readWholeAccountInfo(SqlConnection conn,
            Int32 hid, Int32 acntid, string usr,
            FinanceAccountUIViewModel vmAccount, Boolean withTmpDocs = true)
        {
            var queryString = this.getQueryString(false, null, null, null, acntid, usr, null);
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            try
            {
                // 1. Read header
                cmd = new SqlCommand(queryString, conn);
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        HIHDBUtility.FinAccountHeader_DB2VM(reader, vmAccount, 0);

                        // It should return one entry only!
                        // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;

                        break;
                    }
                }
                else
                {
                    throw new Exception();
                }
                reader.Close();
                reader = null;
                cmd.Dispose();
                cmd = null;

                // Depends on the category
                if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment)
                {
                    vmAccount.ExtraInfo_ADP = new FinanceAccountExtDPViewModel();
                    queryString = HIHDBUtility.getFinanceAccountADPQueryString(vmAccount.ID);

                    cmd = new SqlCommand(queryString, conn);
                    reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HIHDBUtility.FinAccountADP_DB2VM(reader, vmAccount.ExtraInfo_ADP, 0);
                            break; // Shall only one entry
                        };
                    }
                    else
                    {
                        //errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Failed to read Account DP");
                    }

                    reader.Close();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Read out the template docs.
                    if (withTmpDocs)
                    {
                        queryString = HIHDBUtility.getFinanceDocADPListQueryString() + " WHERE [ACCOUNTID] = " + vmAccount.ID.ToString() + " AND [HID] = " + hid.ToString();
                        cmd = new SqlCommand(queryString, conn);
                        reader = await cmd.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var vmTmpADP = new FinanceTmpDocDPViewModel();
                                HIHDBUtility.FinTmpDocADP_DB2VM(reader, vmTmpADP);
                                vmAccount.ExtraInfo_ADP.DPTmpDocs.Add(vmTmpADP);
                            }
                        }
                        reader.Close();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;
                    }
                }
                else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvanceReceive)
                {
                    // Advance receive
                    vmAccount.ExtraInfo_ADP = new FinanceAccountExtDPViewModel();
                    queryString = HIHDBUtility.getFinanceAccountADPQueryString(vmAccount.ID);

                    cmd = new SqlCommand(queryString, conn);
                    reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HIHDBUtility.FinAccountADP_DB2VM(reader, vmAccount.ExtraInfo_ADP, 0);
                            break; // Shall only one entry
                        };
                    }
                    else
                    {
                        //errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Failed to read Account DP");
                    }

                    reader.Close();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Read out the template docs.
                    if (withTmpDocs)
                    {
                        queryString = HIHDBUtility.getFinanceDocADPListQueryString() + " WHERE [ACCOUNTID] = " + vmAccount.ID.ToString() + " AND [HID] = " + hid.ToString();
                        cmd = new SqlCommand(queryString, conn);
                        reader = await cmd.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var vmTmpADP = new FinanceTmpDocDPViewModel();
                                HIHDBUtility.FinTmpDocADP_DB2VM(reader, vmTmpADP);
                                vmAccount.ExtraInfo_ADP.DPTmpDocs.Add(vmTmpADP);
                            }
                        }
                        reader.Close();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;
                    }
                }
                else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                {
                    vmAccount.ExtraInfo_AS = new FinanceAccountExtASViewModel();
                    queryString = HIHDBUtility.getFinanceAccountAssetQueryString(vmAccount.ID);

                    cmd = new SqlCommand(queryString, conn);
                    reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HIHDBUtility.FinAccountAsset_DB2VM(reader, vmAccount.ExtraInfo_AS, 0);
                            break; // Shall only one entry
                        };
                    }
                    else
                    {
                        //errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Failed to read Account Asset");
                    }

                    reader.Close();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;
                }
                else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom
                    || vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
                {
                    vmAccount.ExtraInfo_Loan = new FinanceAccountExtLoanViewModel();
                    queryString = HIHDBUtility.getFinanceAccountLoanQueryString(vmAccount.ID);

                    cmd = new SqlCommand(queryString, conn);
                    reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HIHDBUtility.FinAccountLoan_DB2VM(reader, vmAccount.ExtraInfo_Loan, 0);
                            break; // Shall only one entry
                        };
                    }
                    else
                    {
                        //errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Failed to read Account DP");
                    }

                    reader.Close();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Template docs.
                    if (withTmpDocs)
                    {
                        queryString = HIHDBUtility.GetFinanceDocLoanListQueryString() + " WHERE [ACCOUNTID] = " + vmAccount.ID.ToString() + " AND [HID] = " + hid.ToString();
                        cmd = new SqlCommand(queryString, conn);
                        reader = await cmd.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var vmTmpLoan = new FinanceTmpDocLoanViewModel();
                                HIHDBUtility.FinTmpDocLoan_DB2VM(reader, vmTmpLoan);
                                vmAccount.ExtraInfo_Loan.LoanTmpDocs.Add(vmTmpLoan);
                            }
                        }
                        reader.Close();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;
                    }
                }
            }
            catch (Exception)
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }

                throw;
            }

            return Ok();
        }
        #endregion
    }
}
