using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using achihapi.Utilities;
using System.Net;
using achihapi.ViewModels;
using System.Data.SqlClient;
using System.Data;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceAssetValueChangeController : ControllerBase
    {
        private IMemoryCache _cache;
        public FinanceAssetValueChangeController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // GET: api/FinanceAssetValueChange
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return BadRequest();
        }

        // GET: api/FinanceAssetValueChange/5
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(int id)
        {
            return BadRequest();
        }

        // POST: api/FinanceAssetValueChange
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] FinanceAssetValueChangeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Perform checks
            if (vm.HID <= 0)
                return BadRequest("Not HID inputted");
            if (vm.AssetAccountID <= 0)
                return BadRequest("Asset Account is invalid");
            if (vm.Items.Count != 1)
                return BadRequest("Items count is not correct");

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

            // Construct the Doc.
            var vmFIDoc = new FinanceDocumentUIViewModel();
            vmFIDoc.DocType = FinanceDocTypeViewModel.DocType_AssetValChg;
            vmFIDoc.Desp = vm.Desp;
            vmFIDoc.TranDate = vm.TranDate;
            vmFIDoc.HID = vm.HID;
            vmFIDoc.TranCurr = vm.TranCurr;

            Decimal totalAmount = 0;
            Int32? rlTranType = null;
            foreach (var di in vm.Items)
            {
                if (di.ItemID <= 0 || di.TranAmount == 0 
                    || di.AccountID != vm.AssetAccountID
                    || (di.TranType != FinanceTranTypeViewModel.TranType_AssetValueIncrease
                        && di.TranType != FinanceTranTypeViewModel.TranType_AssetValueDecrease)
                    || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");

                if (rlTranType == null)
                {
                    rlTranType = di.TranType;
                }
                else
                {
                    if (rlTranType.Value != di.TranType)
                    {
                        return BadRequest("Cannot support different trantype");
                    }
                }

                totalAmount += di.TranAmount;

                vmFIDoc.Items.Add(di);
            }

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewDocID = -1;
            Decimal dCurrBalance = 0;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                // Basic check again
                FinanceDocumentController.FinanceDocumentBasicCheck(vmFIDoc);

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

                    // Perfrom the doc. validation
                    await FinanceDocumentController.FinanceDocumentBasicValidationAsync(vmFIDoc, conn);

                    // Additional checks
                    SqlCommand cmdAddCheck = null;
                    SqlDataReader readerAddCheck = null;
                    try
                    {
                        // Check 1: check account is a valid asset?
                        String strsqls = @"SELECT 
                                      [t_fin_account].[STATUS]
                                      ,[t_fin_account_ext_as].[REFDOC_SOLD] AS [ASREFDOC_SOLD]
                                  FROM [dbo].[t_fin_account]
                                  INNER JOIN [dbo].[t_fin_account_ext_as]
                                       ON [t_fin_account].[ID] = [t_fin_account_ext_as].[ACCOUNTID]
                                  WHERE [t_fin_account].[ID] = " + vm.AssetAccountID.ToString()
                                   + " AND [t_fin_account].[HID] = " + vm.HID.ToString();
                        cmdAddCheck = new SqlCommand(strsqls, conn);
                        readerAddCheck = await cmdAddCheck.ExecuteReaderAsync();

                        if (readerAddCheck.HasRows)
                        {
                            while (readerAddCheck.Read())
                            {
                                if (!readerAddCheck.IsDBNull(0))
                                {
                                    var acntStatus = (FinanceAccountStatus)readerAddCheck.GetByte(0);
                                    if (acntStatus != FinanceAccountStatus.Normal)
                                    {
                                        throw new Exception("Account status is not normal");
                                    }
                                }
                                else
                                {
                                    // Status is NULL stands for Active Status
                                    // throw new Exception("Account status is not normal");
                                }

                                if (!readerAddCheck.IsDBNull(1))
                                {
                                    throw new Exception("Account has soldout doc already");
                                }

                                break;
                            }
                        }
                        readerAddCheck.Close();
                        readerAddCheck = null;
                        cmdAddCheck.Dispose();
                        cmdAddCheck = null;

                        // Check 2: check the inputted date is valid > must be the later than all existing transactions;
                        strsqls = @"SELECT MAX(t_fin_document.TRANDATE)
                                    FROM [dbo].[t_fin_document_item]
	                                INNER JOIN [dbo].[t_fin_document] 
                                       ON [dbo].[t_fin_document_item].[DOCID] = [dbo].[t_fin_document].[ID]
                                    WHERE [dbo].[t_fin_document_item].[ACCOUNTID] = " + vm.AssetAccountID.ToString();
                        cmdAddCheck = new SqlCommand(strsqls, conn);
                        readerAddCheck = await cmdAddCheck.ExecuteReaderAsync();

                        if (readerAddCheck.HasRows)
                        {
                            while (readerAddCheck.Read())
                            {
                                var latestdate = readerAddCheck.GetDateTime(0);
                                if (vm.TranDate.Date < latestdate.Date)
                                {
                                    throw new Exception("Invalid date");
                                }

                                break;
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid account - no doc items");
                        }

                        readerAddCheck.Close();
                        readerAddCheck = null;
                        cmdAddCheck.Dispose();
                        cmdAddCheck = null;

                        // Check 3. Fetch current balance
                        strsqls = @"SELECT [balance]
                            FROM [dbo].[v_fin_report_bs] WHERE [accountid] = " + vm.AssetAccountID.ToString();
                        cmdAddCheck = new SqlCommand(strsqls, conn);
                        readerAddCheck = await cmdAddCheck.ExecuteReaderAsync();

                        if (readerAddCheck.HasRows)
                        {
                            while (readerAddCheck.Read())
                            {
                                dCurrBalance = readerAddCheck.GetDecimal(0);
                                if (dCurrBalance <= 0)
                                {
                                    throw new Exception("Balance is zero");
                                }

                                break;
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid account - no doc items");
                        }

                        readerAddCheck.Close();
                        readerAddCheck = null;
                        cmdAddCheck.Dispose();
                        cmdAddCheck = null;

                        // Now check the balance with the inputted value
                        var nCmpRst = Decimal.Compare(dCurrBalance, totalAmount);
                        if (nCmpRst > 0)
                        {
                            if (rlTranType.Value != FinanceTranTypeViewModel.TranType_AssetValueDecrease)
                            {
                                throw new Exception("Tran type is wrong");
                            }
                        }
                        else if (nCmpRst < 0)
                        {
                            if (rlTranType.Value != FinanceTranTypeViewModel.TranType_AssetValueIncrease)
                            {
                                throw new Exception("Tran type is wrong");
                            }
                        }
                        else if (nCmpRst == 0)
                        {
                            throw new Exception("Current balance equals to new value");
                        }
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }
                    finally
                    {
                        if (readerAddCheck != null)
                        {
                            readerAddCheck.Close();
                            readerAddCheck = null;
                        }
                        if (cmdAddCheck != null)
                        {
                            cmdAddCheck.Dispose();
                            cmdAddCheck = null;
                        }
                    }

                    // Begin the modification
                    tran = conn.BeginTransaction();

                    // First, craete the doc header => nNewDocID
                    queryString = HIHDBUtility.GetFinDocHeaderInsertString();
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, vmFIDoc, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    cmd.Dispose();
                    cmd = null;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vmFIDoc.Items)
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

                    // Do the commit
                    tran.Commit();

                    // Update the buffer
                    // Account List
                    try
                    {
                        var cacheKey = String.Format(CacheKeys.FinAccountList, vm.HID, null);
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

                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;

                if (tran != null)
                    tran.Rollback();
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

            // Return nothing
            return Ok(nNewDocID);
        }

        // PUT: api/FinanceAssetValueChange/5
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(int id, [FromBody] string value)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            return BadRequest();
        }
    }
}
