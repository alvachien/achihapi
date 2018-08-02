using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceLoanTmpDoc")]
    public class FinanceLoanTmpDocController : Controller
    {
        // GET: api/FinanceLoanTmpDoc
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean skipPosted = true, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            if (hid <= 0)
                return BadRequest("No HID inputted");

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

            List<FinanceTmpDocLoanViewModel> listVm = new List<FinanceTmpDocLoanViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = HIHDBUtility.GetFinanceDocLoanListQueryString() + " WHERE [HID] = @hid ";
                if (skipPosted)
                    queryString += " AND [REFDOCID] IS NULL ";
                if (dtbgn.HasValue)
                    queryString += " AND [TRANDATE] >= @dtbgn ";
                if (dtend.HasValue)
                    queryString += " AND [TRANDATE] <= @dtend ";
                queryString += " ORDER BY [TRANDATE] DESC";

                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@hid", hid);
                if (dtbgn.HasValue)
                    cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                if (dtbgn.HasValue)
                    cmd.Parameters.AddWithValue("@dtend", dtend.Value);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceTmpDocLoanViewModel dpvm = new FinanceTmpDocLoanViewModel();
                        HIHDBUtility.FinTmpDocLoan_DB2VM(reader, dpvm);
                        listVm.Add(dpvm);
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
            
            return new JsonResult(listVm, setting);
        }

        // GET: api/FinanceLoanTmpDoc/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/FinanceLoanTmpDoc
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromQuery]Int32 hid, Int32 tmpdocid, [FromBody]FinanceDocumentUIViewModel repaydoc)
        {
            // The post here is:
            // 1. Post a repayment document with the content from this template doc
            // 2. Update the template doc with REFDOCID

            // Basic check
            if (hid <= 0 || tmpdocid <= 0 
                || repaydoc == null || repaydoc.HID != hid 
                || repaydoc.DocType != FinanceDocTypeViewModel.DocType_Repay)
            {
                return BadRequest("No data inputted!");
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = String.Empty;
            Boolean bError = false;
            String strErrMsg = String.Empty;

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

            // Update the database
            FinanceTmpDocLoanViewModel vmTmpDoc = new FinanceTmpDocLoanViewModel();
            HomeDefViewModel vmHome = new HomeDefViewModel();
            FinanceAccountExtLoanViewModel vmAccount = new FinanceAccountExtLoanViewModel();

            try
            {
                await conn.OpenAsync();

                // Check: HID, it requires more info than just check, so it implemented it 
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                // Check: DocID
                String checkString = HIHDBUtility.GetFinanceDocLoanListQueryString() + " WHERE [DOCID] = " + tmpdocid.ToString() + " AND [HID] = " + hid.ToString();
                SqlCommand chkcmd = new SqlCommand(checkString, conn);
                SqlDataReader chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid Doc ID inputted: " + tmpdocid.ToString());
                }
                else
                {
                    while (chkreader.Read())
                    {
                        HIHDBUtility.FinTmpDocLoan_DB2VM(chkreader, vmTmpDoc);

                        // It shall be only one entry if found!
                        break;
                    }
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Check: Tmp doc has posted or not?
                if (vmTmpDoc == null || (vmTmpDoc.RefDocID.HasValue && vmTmpDoc.RefDocID.Value > 0))
                {
                    return BadRequest("Tmp Doc not existed yet or has been posted");
                }

                // Data validation - basic
                try
                {
                    await FinanceDocumentController.FinanceDocumentBasicValidationAsync(repaydoc, conn);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                // Data validation - loan specific
                try
                {
                    // Check the amount
                    decimal totalOut = repaydoc.Items.Where(item => item.TranType == FinanceTranTypeViewModel.TranType_RepaymentOut).Sum(item2 => item2.TranAmount);
                    decimal totalIn = repaydoc.Items.Where(item => item.TranType == FinanceTranTypeViewModel.TranType_RepaymentIn).Sum(item2 => item2.TranAmount);
                    decimal totalintOut = repaydoc.Items.Where(item => (item.TranType == FinanceTranTypeViewModel.TranType_InterestOut)).Sum(item2 => item2.TranAmount);
                    
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                // Now go ahead for the creating
                SqlTransaction tran = conn.BeginTransaction();

                SqlCommand cmd = null;
                Int32 nNewDocID = 0;

                // Now go ahead for creating
                queryString = HIHDBUtility.GetFinDocHeaderInsertString();

                try
                {
                    // Header
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, repaydoc, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    repaydoc.ID = nNewDocID;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in repaydoc.Items)
                    {
                        queryString = HIHDBUtility.GetFinDocItemInsertString();

                        SqlCommand cmd2 = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        HIHDBUtility.BindFinDocItemInsertParameter(cmd2, ivm, nNewDocID);

                        await cmd2.ExecuteNonQueryAsync();
                    }

                    // Then, update the template doc
                    queryString = @"UPDATE [dbo].[t_fin_tmpdoc_loan]
                                       SET [REFDOCID] = @REFDOCID
                                          ,[UPDATEDBY] = @UPDATEDBY
                                          ,[UPDATEDAT] = @UPDATEDAT
                                     WHERE [HID] = @HID AND [DOCID] = @DOCID";
                    SqlCommand cmdTmpDoc = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };
                    cmdTmpDoc.Parameters.AddWithValue("@REFDOCID", nNewDocID);
                    cmdTmpDoc.Parameters.AddWithValue("@UPDATEDBY", usrName);
                    cmdTmpDoc.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
                    cmdTmpDoc.Parameters.AddWithValue("@HID", hid);
                    cmdTmpDoc.Parameters.AddWithValue("@DOCID", tmpdocid);
                    await cmdTmpDoc.ExecuteNonQueryAsync();

                    tran.Commit();
                }
                catch (Exception exp)
                {
                    if (tran != null)
                        tran.Rollback();

                    throw exp; // Re-throw
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
            {
                return StatusCode(500, strErrMsg);
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(repaydoc, setting);
        }

        // PUT: api/FinanceLoanTmpDoc/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]FinanceTmpDocDPViewModel vm)
        {
            return BadRequest();
        }

        // DELETE: api/FinanceLoanTmpDoc/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }

        // Validation
        private async Task DocumentValidationAsync(FinanceDocumentUIViewModel vmDoc, FinanceTmpDocLoanViewModel vmLoan, SqlConnection conn)
        {
            String strCheckString = @"SELECT TOP (1) [BASECURR] FROM [dbo].[t_homedef] WHERE [ID] = @hid;";
            SqlCommand cmdCheck = new SqlCommand(strCheckString, conn);
            cmdCheck.Parameters.AddWithValue("@hid", vmDoc.HID);

            SqlDataReader reader = await cmdCheck.ExecuteReaderAsync();
            if (!reader.HasRows)
                throw new Exception("No home found");

            // Basic currency
            string basecurr = String.Empty;
            while (reader.Read())
            {
                basecurr = reader.GetString(0);
                break;
            }
            if (String.IsNullOrEmpty(basecurr))
                throw new Exception("No base currency defined!");

            reader.Dispose();
            reader = null;
            cmdCheck.Dispose();
            cmdCheck = null;

            // Currency
            strCheckString = @"SELECT TOP (1) CURR from t_fin_currency WHERE curr = @curr;";
            cmdCheck = new SqlCommand(strCheckString, conn);
            cmdCheck.Parameters.AddWithValue("@curr", vmDoc.TranCurr);
            reader = await cmdCheck.ExecuteReaderAsync();
            if (!reader.HasRows)
                throw new Exception("No currency found");
            reader.Dispose();
            reader = null;
            cmdCheck.Dispose();
            cmdCheck = null;

            if (String.CompareOrdinal(vmDoc.TranCurr, basecurr) != 0)
            {
                if (vmDoc.ExgRate == 0)
                {
                    throw new Exception("No exchange rate info provided!");
                }
            }

            // Second currency
            if (!String.IsNullOrEmpty(vmDoc.TranCurr2))
            {
                strCheckString = @"SELECT TOP (1) CURR from t_fin_currency WHERE curr = @curr;";
                cmdCheck = new SqlCommand(strCheckString, conn);
                cmdCheck.Parameters.AddWithValue("@curr", vmDoc.TranCurr2);
                reader = await cmdCheck.ExecuteReaderAsync();
                if (!reader.HasRows)
                    throw new Exception("No currency found");

                reader.Dispose();
                reader = null;
                cmdCheck.Dispose();
                cmdCheck = null;

                if (String.CompareOrdinal(vmDoc.TranCurr2, basecurr) != 0)
                {
                    if (vmDoc.ExgRate2 == 0)
                    {
                        throw new Exception("No exchange rate info provided!");
                    }
                }
            }

            // Doc type
            strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_doc_type] WHERE [ID] = @ID"; // @"SELECT TOP (1) [ID] FROM [t_fin_doc_type] WHERE [HID] = @HID AND [ID] = @ID";
            cmdCheck = new SqlCommand(strCheckString, conn);
            //cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
            cmdCheck.Parameters.AddWithValue("@ID", vmDoc.DocType);
            reader = await cmdCheck.ExecuteReaderAsync();
            if (!reader.HasRows)
                throw new Exception("Invalid document type");
            reader.Dispose();
            reader = null;
            cmdCheck.Dispose();
            cmdCheck = null;

            Decimal totalamount = 0;
            foreach (var item in vmDoc.Items)
            {
                // Account
                strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_account] WHERE [HID] = @HID AND [ID] = @ID";
                cmdCheck = new SqlCommand(strCheckString, conn);
                cmdCheck.Parameters.AddWithValue("@HID", vmDoc.HID);
                cmdCheck.Parameters.AddWithValue("@ID", item.AccountID);
                reader = await cmdCheck.ExecuteReaderAsync();
                if (!reader.HasRows)
                    throw new Exception("No account found");
                reader.Dispose();
                reader = null;
                cmdCheck.Dispose();
                cmdCheck = null;

                // Transaction type
                strCheckString = @"SELECT TOP (1) [ID], [EXPENSE] FROM [t_fin_tran_type] WHERE [ID] = @ID";//@"SELECT TOP (1) [ID], [EXPENSE] FROM [t_fin_tran_type] WHERE [HID] = @HID AND [ID] = @ID";
                cmdCheck = new SqlCommand(strCheckString, conn);
                cmdCheck.Parameters.AddWithValue("@ID", item.TranType);
                reader = await cmdCheck.ExecuteReaderAsync();
                if (!reader.HasRows)
                    throw new Exception("No tran. type found");

                Boolean isexp = false;
                while (reader.Read())
                {
                    isexp = reader.GetBoolean(1);
                    break;
                }
                reader.Dispose();
                reader = null;
                cmdCheck.Dispose();
                cmdCheck = null;

                // Control center
                if (item.ControlCenterID > 0)
                {
                    strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_controlcenter] WHERE [HID] = @HID AND [ID] = @ID";
                    cmdCheck = new SqlCommand(strCheckString, conn);
                    cmdCheck.Parameters.AddWithValue("@HID", vmDoc.HID);
                    cmdCheck.Parameters.AddWithValue("@ID", item.ControlCenterID);
                    reader = await cmdCheck.ExecuteReaderAsync();
                    if (!reader.HasRows)
                        throw new Exception("No control center found");
                    reader.Dispose();
                    reader = null;
                    cmdCheck.Dispose();
                    cmdCheck = null;
                }

                // Order
                if (item.OrderID > 0)
                {
                    strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_order] WHERE [HID] = @HID AND [ID] = @ID";
                    cmdCheck = new SqlCommand(strCheckString, conn);
                    cmdCheck.Parameters.AddWithValue("@HID", vmDoc.HID);
                    cmdCheck.Parameters.AddWithValue("@ID", item.OrderID);
                    reader = await cmdCheck.ExecuteReaderAsync();
                    if (!reader.HasRows)
                        throw new Exception("No order found");
                    reader.Dispose();
                    reader = null;
                    cmdCheck.Dispose();
                    cmdCheck = null;
                }

                // Item amount
                Decimal itemAmt = 0;
                if (item.UseCurr2)
                {
                    if (isexp)
                    {
                        if (vmDoc.ExgRate2 > 0)
                        {
                            itemAmt = -1 * item.TranAmount * vmDoc.ExgRate2 / 100;
                        }
                        else
                        {
                            itemAmt = -1 * item.TranAmount;
                        }
                    }
                    else
                    {
                        if (vmDoc.ExgRate2 > 0)
                        {
                            itemAmt = item.TranAmount * vmDoc.ExgRate2 / 100;
                        }
                        else
                        {
                            itemAmt = item.TranAmount;
                        }
                    }
                }
                else
                {
                    if (isexp)
                    {
                        if (vmDoc.ExgRate > 0)
                        {
                            itemAmt = -1 * item.TranAmount * vmDoc.ExgRate / 100;
                        }
                        else
                        {
                            itemAmt = -1 * item.TranAmount;
                        }
                    }
                    else
                    {
                        if (vmDoc.ExgRate > 0)
                        {
                            itemAmt = item.TranAmount * vmDoc.ExgRate / 100;
                        }
                        else
                        {
                            itemAmt = item.TranAmount;
                        }
                    }
                }

                if (itemAmt == 0)
                {
                    throw new Exception("Amount is not correct");
                }

                totalamount += itemAmt;
            }

            if (vmDoc.DocType == FinanceDocTypeViewModel.DocType_Transfer || vmDoc.DocType == FinanceDocTypeViewModel.DocType_CurrExchange)
            {
                if (totalamount != 0)
                {
                    throw new Exception("Amount must be zero");
                }
            }
        }
    }
}
