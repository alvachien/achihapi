using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using achihapi.Utilities;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceLoanRepayDocument")]
    public class FinanceLoanRepayDocumentController : Controller
    {
        // GET: api/FinanceLoanRepayDocument
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/FinanceLoanRepayDocument/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/FinanceLoanRepayDocument
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
            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
            //FinanceAccountExtLoanViewModel vmAccount = new FinanceAccountExtLoanViewModel();

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

                // Check: Loan account
                checkString = HIHDBUtility.GetFinanceLoanAccountQueryString(hid, vmTmpDoc.AccountID);
                chkcmd = new SqlCommand(checkString, conn);
                chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid Doc ID inputted: " + tmpdocid.ToString());
                }
                else
                {
                    while (chkreader.Read())
                    {
                        Int32 aidx = 0;
                        aidx = HIHDBUtility.FinAccountHeader_DB2VM(chkreader, vmAccount, aidx);
                        vmAccount.ExtraInfo_Loan = new FinanceAccountExtLoanViewModel();
                        HIHDBUtility.FinAccountLoan_DB2VM(chkreader, vmAccount.ExtraInfo_Loan, aidx);
                    }
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

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
                    int ninvaliditems = 0;
                    // Only four tran. types are allowed
                    if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom)
                    {
                        ninvaliditems = repaydoc.Items.Where(item => item.TranType != FinanceTranTypeViewModel.TranType_InterestOut
                            && item.TranType != FinanceTranTypeViewModel.TranType_RepaymentOut
                            && item.TranType != FinanceTranTypeViewModel.TranType_RepaymentIn)
                            .Count();
                    }
                    else if (vmAccount.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
                    {
                        ninvaliditems = repaydoc.Items.Where(item => item.TranType != FinanceTranTypeViewModel.TranType_InterestIn
                            && item.TranType != FinanceTranTypeViewModel.TranType_RepaymentOut
                            && item.TranType != FinanceTranTypeViewModel.TranType_RepaymentIn)
                            .Count();
                    }
                    if (ninvaliditems > 0)
                    {
                        return BadRequest("Items with invalid tran type");
                    }

                    // Check the amount
                    decimal totalOut = repaydoc.Items.Where(item => item.TranType == FinanceTranTypeViewModel.TranType_RepaymentOut).Sum(item2 => item2.TranAmount);
                    decimal totalIn = repaydoc.Items.Where(item => item.TranType == FinanceTranTypeViewModel.TranType_RepaymentIn).Sum(item2 => item2.TranAmount);
                    decimal totalintOut = repaydoc.Items.Where(item => (item.TranType == FinanceTranTypeViewModel.TranType_InterestOut)).Sum(item2 => item2.TranAmount);

                    if (totalintOut != 0)
                    {
                        return BadRequest("Amount is not equal!");
                    }
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

        // PUT: api/FinanceLoanRepayDocument/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
