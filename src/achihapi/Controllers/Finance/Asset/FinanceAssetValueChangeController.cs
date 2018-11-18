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
            if (vm.TranAmount <= 0)
                return BadRequest("Amount is less than zero");
            if (vm.Items.Count <= 0)
                return BadRequest("No items inputted");

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

            foreach (var di in vm.Items)
            {
                if (di.ItemID <= 0 || di.TranAmount == 0 
                    || di.AccountID != vm.AssetAccountID
                    || (di.TranType != FinanceTranTypeViewModel.TranType_AssetValueIncrease
                        && di.TranType != FinanceTranTypeViewModel.TranType_AssetValueDecrease)
                    || (di.ControlCenterID <= 0 && di.OrderID <= 0))
                    return BadRequest("Invalid input data in items!");

                vmFIDoc.Items.Add(di);
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
                // Basic check again
                await FinanceDocumentController.FinanceDocumentBasicCheckAsync(vmFIDoc);

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
                        return BadRequest();
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
