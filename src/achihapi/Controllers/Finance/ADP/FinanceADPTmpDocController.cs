using System;
using System.Collections.Generic;
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
    [Route("api/FinanceADPTmpDoc")]
    public class FinanceADPTmpDocController : Controller
    {
        // GET: api/FinanceADPTmpDoc
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

            List<FinanceTmpDocDPViewModel> listVm = new List<FinanceTmpDocDPViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = HIHDBUtility.getFinanceDocADPListQueryString() + " WHERE [HID] = @hid ";
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
                        FinanceTmpDocDPViewModel dpvm = new FinanceTmpDocDPViewModel();
                        HIHDBUtility.FinTmpDocADP_DB2VM(reader, dpvm);
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

        // GET: api/FinanceADPTmpDoc/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return BadRequest();
        }

        // POST: api/FinanceADPTmpDoc
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromQuery]Int32 hid, Int32 docid)
        {
            // The post here is:
            // 1. Post a normal document with the content from this template doc
            // 2. Update the template doc with REFDOCID

            // Basic check
            if (hid <= 0|| docid <= 0)
            {
                return BadRequest("No data inputted!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = String.Empty;
            Boolean bError = false;
            String strErrMsg = String.Empty;
            FinanceTmpDocDPViewModel vmTmpDoc = new FinanceTmpDocDPViewModel();
            HomeDefViewModel vmHome = new HomeDefViewModel();
            FinanceDocumentUIViewModel vmFIDOC = new FinanceDocumentUIViewModel();

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

            try
            {
                await conn.OpenAsync();

                // Check: HID, it requires more info than just check, so it implemented it 
                if (hid != 0)
                {
                    String strHIDCheck = HIHDBUtility.getHomeDefQueryString() + " WHERE [ID]= @hid AND [USER] = @user";
                    SqlCommand cmdHIDCheck = new SqlCommand(strHIDCheck, conn);
                    cmdHIDCheck.Parameters.AddWithValue("@hid", hid);
                    cmdHIDCheck.Parameters.AddWithValue("@user", usrName);
                    SqlDataReader readHIDCheck = await cmdHIDCheck.ExecuteReaderAsync();
                    if (!readHIDCheck.HasRows)
                        return BadRequest("No home found");
                    else
                    {
                        while (readHIDCheck.Read())
                        {
                            HIHDBUtility.HomeDef_DB2VM(readHIDCheck, vmHome);

                            // It shall be only one entry if found!
                            break;
                        }
                    }                        

                    readHIDCheck.Dispose();
                    readHIDCheck = null;
                    cmdHIDCheck.Dispose();
                    cmdHIDCheck = null;
                }

                if (vmHome == null || String.IsNullOrEmpty(vmHome.BaseCurrency) || vmHome.ID != hid)
                {
                    return BadRequest("Home Definition is invalid");
                }

                // Check: DocID
                String checkString = HIHDBUtility.getFinanceDocADPListQueryString() + " WHERE [DOCID] = " + docid.ToString() + " AND [HID] = " + hid.ToString();
                SqlCommand chkcmd = new SqlCommand(checkString, conn);
                SqlDataReader chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid Doc ID inputted: " + docid.ToString());
                } 
                else
                {
                    while(chkreader.Read())
                    {
                        HIHDBUtility.FinTmpDocADP_DB2VM(chkreader, vmTmpDoc);

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

                // Now go ahead for the creating
                SqlTransaction tran = conn.BeginTransaction();

                SqlCommand cmd = null;
                Int32 nNewDocID = 0;
                vmFIDOC.Desp = vmTmpDoc.Desp;
                vmFIDOC.DocType = FinanceDocTypeViewModel.DocType_Normal;
                vmFIDOC.HID = hid;
                //vmFIDOC.TranAmount = vmTmpDoc.TranAmount;
                vmFIDOC.TranCurr = vmHome.BaseCurrency;
                vmFIDOC.TranDate = vmTmpDoc.TranDate;
                vmFIDOC.CreatedAt = DateTime.Now;
                FinanceDocumentItemUIViewModel vmItem = new FinanceDocumentItemUIViewModel
                {
                    AccountID = vmTmpDoc.AccountID
                };
                if (vmTmpDoc.ControlCenterID.HasValue)
                    vmItem.ControlCenterID = vmTmpDoc.ControlCenterID.Value;
                if (vmTmpDoc.OrderID.HasValue)
                    vmItem.OrderID = vmTmpDoc.OrderID.Value;
                vmItem.Desp = vmTmpDoc.Desp;
                vmItem.ItemID = 1;
                vmItem.TranAmount = vmTmpDoc.TranAmount;
                vmItem.TranType = vmTmpDoc.TranType;
                vmFIDOC.Items.Add(vmItem);

                // Now go ahead for the creating
                queryString = HIHDBUtility.GetFinDocHeaderInsertString();

                try
                {
                    // Header
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, vmFIDOC, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;
                    vmFIDOC.ID = nNewDocID;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vmFIDOC.Items)
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
                    queryString = @"UPDATE [dbo].[t_fin_tmpdoc_dp]
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
                    cmdTmpDoc.Parameters.AddWithValue("@DOCID", docid);
                    await cmdTmpDoc.ExecuteNonQueryAsync();

                    tran.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                    bError = true;
                    strErrMsg = exp.Message;

                    if (tran != null)
                        tran.Rollback();
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

            return new JsonResult(vmFIDOC, setting);
        }

        // PUT: api/FinanceADPTmpDoc/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]FinanceTmpDocDPViewModel vm)
        {
            return BadRequest();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}