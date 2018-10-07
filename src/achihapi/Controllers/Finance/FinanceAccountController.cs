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
                        return BadRequest();
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
            SqlCommand cmd = null;
            SqlDataReader reader = null;
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
                            errorCode = HttpStatusCode.NotFound;
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
                                errorCode = HttpStatusCode.BadRequest;
                                throw new Exception("Failed to read Account DP");
                            }

                            reader.Close();
                            reader = null;
                            cmd.Dispose();
                            cmd = null;

                            // Read out the template docs.
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
                                errorCode = HttpStatusCode.BadRequest;
                                throw new Exception("Failed to read Account DP");
                            }

                            reader.Close();
                            reader = null;
                            cmd.Dispose();
                            cmd = null;

                            // Read out the template docs.
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
                                errorCode = HttpStatusCode.BadRequest;
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
                                errorCode = HttpStatusCode.BadRequest;
                                throw new Exception("Failed to read Account DP");
                            }

                            reader.Close();
                            reader = null;
                            cmd.Dispose();
                            cmd = null;

                            // Template docs.
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
                        return BadRequest();
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
                return BadRequest("No data is inputted or inputted data for Advance payment/receive/Asset/Loan");
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

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

            if (vm.HID <= 0)
                return BadRequest("No HID inputted!");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_account] WHERE [HID] = " + vm.HID.ToString() + " AND [Name] = N'" + vm.Name + "'";

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
                        nNewID = (Int32)idparam.Value;

                        // Now commit it!
                        tran.Commit();
                    }
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();
                System.Diagnostics.Debug.WriteLine(exp.Message);
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
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            vm.ID = nNewID;
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

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name) || vm.ID != id)
            {
                return BadRequest("Name is a must!");
            }
            if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvanceReceive)
            {
                if (vm.ExtraInfo_ADP == null)
                {
                    return BadRequest("Advance payment info is missing");
                }
            }
            else if(vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
            {
                if (vm.ExtraInfo_AS == null)
                {
                    return BadRequest("Asset info is missing");
                }
            }
            else if(vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
            {
                if (vm.ExtraInfo_Loan == null)
                {
                    return BadRequest("Loan info is missing");
                }
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            SqlTransaction tran = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = @"UPDATE [dbo].[t_fin_account]
                       SET [CTGYID] = @CTGYID
                          ,[NAME] = @NAME
                          ,[COMMENT] = @COMMENT
                          ,[OWNER] = @OWNER
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";

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

                    tran = conn.BeginTransaction();

                    // 1. Account header
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
                    cmd.Parameters.AddWithValue("@NAME", vm.Name);
                    cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
                    if (String.IsNullOrEmpty(vm.Owner))
                    {
                        cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
                    }
                    cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                    cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                    cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                    vm.UpdatedAt = DateTime.Now;
                    cmd.Parameters.AddWithValue("@UPDATEDAT", vm.UpdatedAt);
                    cmd.Parameters.AddWithValue("@ID", vm.ID);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // 2. For extend attributes
                    if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvanceReceive)
                    {
                        // 2.1 Extend attributes
                        queryString = HIHDBUtility.GetFinanceAccountADPUpdateString();
                        cmd = new SqlCommand(queryString, conn, tran);
                        HIHDBUtility.BindFinAccountADPUpdateParamter(cmd, vm.ExtraInfo_ADP);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        // 2.2 Tmplate docs
                        queryString = HIHDBUtility.GetFinanceDocADPDeleteString(vm.HID, vm.ID, true);
                        cmd = new SqlCommand(queryString, conn, tran);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        foreach (var tdoc in vm.ExtraInfo_ADP.DPTmpDocs)
                        {
                            if (tdoc.RefDocID != null)
                                continue;

                            queryString = HIHDBUtility.getFinanceTmpDocADPInsertString();
                            cmd = new SqlCommand(queryString, conn, tran);
                            HIHDBUtility.bindFinTmpDocADPParameter(cmd, tdoc, vm.ID, usrName);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }
                    }
                    else if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                    {
                        // 2.1 Extend attributes
                        queryString = HIHDBUtility.GetFinanceAccountAssetUpdateString();
                        cmd = new SqlCommand(queryString, conn, tran);
                        HIHDBUtility.BindFinAccountAssetUpdateParameter(cmd, vm.ExtraInfo_AS);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;
                    }
                    else if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
                    {
                        // 2.0 History table
                        // TBD.

                        // 2.1 Extend attributes
                        queryString = HIHDBUtility.GetFinanceAccountLoanUpdateString();
                        cmd = new SqlCommand(queryString, conn, tran);
                        HIHDBUtility.BindFinAccountLoanUpdateParameter(cmd, vm.ExtraInfo_Loan);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        // 2.2 Tmplate docs
                        queryString = HIHDBUtility.GetFinanceDocLoanDeleteString(vm.HID, vm.ID, true);
                        cmd = new SqlCommand(queryString, conn, tran);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        foreach (var tdoc in vm.ExtraInfo_Loan.LoanTmpDocs)
                        {
                            if (tdoc.RefDocID != null)
                                continue;

                            queryString = HIHDBUtility.GetFinanceTmpDocLoanInsertString();
                            cmd = new SqlCommand(queryString, conn, tran);
                            HIHDBUtility.BindFinTmpDocLoanParameter(cmd, tdoc, vm.ID, usrName);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }
                    }

                    // Now commit it!
                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();
                System.Diagnostics.Debug.WriteLine(exp.Message);
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
                        return BadRequest();
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
                            HIHDBUtility.FinAccount_DB2VM(reader, vmAccount);

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
                        // Only update the status
                        tran = conn.BeginTransaction();

                        queryString = HIHDBUtility.GetFinanceAccountStatusUpdateString();
                        cmd = new SqlCommand(queryString, conn, tran);

                        FinanceAccountStatus newstatus = (FinanceAccountStatus)Byte.Parse((string)patch.Operations[0].value);
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
                    }
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();

                System.Diagnostics.Debug.WriteLine(exp.Message);
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
                        return BadRequest();
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
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();
                System.Diagnostics.Debug.WriteLine(exp.Message);
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
                        return BadRequest();
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
        #endregion
    }
}
