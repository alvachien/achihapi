﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceDocumentController : Controller
    {
        // GET: api/financedocument
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            BaseListViewModel<FinanceDocumentUIViewModel> listVMs = new BaseListViewModel<FinanceDocumentUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            if (hid <= 0)
                return BadRequest("No Home Inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            try
            {
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

                queryString = @"SELECT count(*) FROM[dbo].[t_fin_document] WHERE[HID] = @hid ";
                if (dtbgn.HasValue)
                    queryString += " AND [TRANDATE] >= @dtbgn ";
                if (dtend.HasValue)
                    queryString += " AND [TRANDATE] <= @dtend ";
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@hid", hid);
                if (dtbgn.HasValue)
                    cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                if (dtend.HasValue)
                    cmd.Parameters.AddWithValue("@dtend", dtend.Value);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    listVMs.TotalCount = reader.GetInt32(0);
                    break;
                }
                reader.Dispose();
                reader = null;
                cmd.Dispose();
                cmd = null;

                if (listVMs.TotalCount > 0)
                {
                    queryString = SqlUtility.getFinanceDocListQueryString();
                    queryString += @" WHERE [HID] = @hid ";
                    if (dtbgn.HasValue)
                        queryString += " AND [TRANDATE] >= @dtbgn ";
                    if (dtend.HasValue)
                        queryString += " AND [TRANDATE] <= @dtend ";
                    queryString += " ORDER BY [TRANDATE] DESC";
                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@hid", hid);
                    if (dtbgn.HasValue)
                        cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                    if (dtend.HasValue)
                        cmd.Parameters.AddWithValue("@dtend", dtend.Value);
                    reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            FinanceDocumentUIViewModel avm = new FinanceDocumentUIViewModel();
                            SqlUtility.FinDocList_DB2VM(reader, avm);

                            listVMs.Add(avm);
                        }
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
            ;
            return new JsonResult(listVMs, setting);
        }

        // GET api/financedocument/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            FinanceDocumentUIViewModel vm = new FinanceDocumentUIViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = SqlUtility.getFinanceDocQueryString(id, hid);

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
                SqlDataReader reader = cmd.ExecuteReader();

                // Header
                while (reader.Read())
                {
                    SqlUtility.FinDocHeader_DB2VM(reader, vm);
                }
                reader.NextResult();

                // Items
                while (reader.Read())
                {
                    FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                    SqlUtility.FinDocItem_DB2VM(reader, itemvm);

                    vm.Items.Add(itemvm);
                }
                reader.NextResult();

                // Tags
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

            if (bNotFound)
            {
                return NotFound();
            }
            else if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            return new ObjectResult(vm);
        }

        // POST api/financedocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceDocumentUIViewModel vm)
        {
            if (vm == null || vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment
                    || vm.DocType == FinanceDocTypeViewModel.DocType_AssetBuyIn
                    || vm.DocType == FinanceDocTypeViewModel.DocType_AssetSoldOut
                    || vm.DocType == FinanceDocTypeViewModel.DocType_Loan)
            {
                return BadRequest("No data is inputted or for Advancepay/Loan/Asset");
            }
            if (vm.HID <= 0)
                return BadRequest("No Home ID inputted");

            // Do the basic check!
            try
            {
                BasicChecks(vm);
            }
            catch (Exception exp)
            {
                return BadRequest(exp.Message);
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nNewDocID = -1;
            Boolean bError = false;
            String strErrMsg = "";
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            try
            {
                await conn.OpenAsync();

                // Do the validation
                try
                {
                    await BasicValidationAsync(vm, conn);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlTransaction tran = conn.BeginTransaction();

                SqlCommand cmd = null;

                // Now go ahead for the creating
                queryString = SqlUtility.GetFinDocHeaderInsertString();

                try
                {
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    SqlUtility.BindFinDocHeaderInsertParameter(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = SqlUtility.GetFinDocItemInsertString();

                        SqlCommand cmd2 = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        SqlUtility.BindFinDocItemInsertParameter(cmd2, ivm, nNewDocID);

                        await cmd2.ExecuteNonQueryAsync();

                        cmd2.Dispose();
                        cmd2 = null;

                        // Tags
                        if (ivm.TagTerms.Count > 0)
                        {
                            // Create tags
                            foreach(var term in ivm.TagTerms)
                            {
                                queryString = SqlUtility.GetTagInsertString();

                                cmd2 = new SqlCommand(queryString, conn, tran);

                                SqlUtility.BindTagInsertParameter(cmd2, vm.HID, HIHTagTypeEnum.FinanceDocumentItem, nNewDocID, term, ivm.ItemID);

                                await cmd2.ExecuteNonQueryAsync();

                                cmd2.Dispose();
                                cmd2 = null;
                            }
                        }
                    }

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
                return StatusCode(500, strErrMsg);

            vm.ID = nNewDocID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // PUT api/financedocument/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]FinanceDocumentUIViewModel vm)
        {
            if (vm == null || vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment
                    || vm.DocType == FinanceDocTypeViewModel.DocType_AssetBuyIn
                    || vm.DocType == FinanceDocTypeViewModel.DocType_AssetSoldOut
                    || vm.DocType == FinanceDocTypeViewModel.DocType_Loan)
            {
                return BadRequest("No data is inputted or for Advancepay/Loan/Asset");
            }
            if (vm.HID <= 0)
                return BadRequest("No Home ID inputted");
            if (vm.ID <= 0)
                return BadRequest("No Doc ID inputted");

            // Do the basic check!
            try
            {
                BasicChecks(vm);
            }
            catch(Exception exp)
            {
                return BadRequest(exp.Message);
            }

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            try
            {
                await conn.OpenAsync();

                // Do the validation
                try
                {
                    await BasicValidationAsync(vm, conn);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                SqlTransaction tran = conn.BeginTransaction();

                SqlCommand cmd = null;

                // Now go ahead for the updating
                queryString = SqlUtility.GetFinDocHeaderUpdateString();

                try
                {
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    // Step 1, Update Header
                    SqlUtility.BindFinDocHeaderUpdateParameter(cmd, vm, usrName);
                    await cmd.ExecuteNonQueryAsync();

                    // Step 2, Delete all items
                    queryString = @"DELETE FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + id.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Step 3 , Re-create the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = SqlUtility.GetFinDocItemInsertString();

                        SqlCommand cmd2 = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        SqlUtility.BindFinDocItemInsertParameter(cmd2, ivm, vm.ID);

                        await cmd2.ExecuteNonQueryAsync();

                        cmd2.Dispose();
                        cmd2 = null;

                        // Delete tag if exist
                        queryString = SqlUtility.GetTagDeleteString(true);
                        cmd2 = new SqlCommand(queryString, conn, tran);
                        SqlUtility.BindTagDeleteParameter(cmd2, vm.HID, HIHTagTypeEnum.FinanceDocumentItem, vm.ID, ivm.ItemID);
                        await cmd2.ExecuteNonQueryAsync();
                        cmd2.Dispose();
                        cmd2 = null;

                        // Tags
                        if (ivm.TagTerms.Count > 0)
                        {
                            // Create tags
                            foreach (var term in ivm.TagTerms)
                            {
                                queryString = SqlUtility.GetTagInsertString();

                                cmd2 = new SqlCommand(queryString, conn, tran);
                                SqlUtility.BindTagInsertParameter(cmd2, vm.HID, HIHTagTypeEnum.FinanceDocumentItem, vm.ID, term, ivm.ItemID);

                                await cmd2.ExecuteNonQueryAsync();

                                cmd2.Dispose();
                                cmd2 = null;
                            }
                        }
                    }

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
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // DELETE api/financedocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            Int32 accID = -1, tmpdocID = -1;
            List<Int32> listPostedID = new List<Int32>();

            String strErrMsg = "";
            SqlTransaction tran = null;

            try
            {
                queryString = @"SELECT [ACCOUNTID],[REFDOCID] FROM [dbo].[t_fin_account_ext_dp] WHERE [REFDOCID] = " + id.ToString() 
                        + " UNION ALL SELECT [ACCOUNTID],[REFDOCID] FROM [dbo].[t_fin_account_ext_loan] WHERE [REFDOCID] = " + id.ToString()
                        + " UNION ALL SELECT [ACCOUNTID],[REFDOC_BUY] AS [REFDOCID] FROM [dbo].[t_fin_account_ext_as] WHERE [REFDOC_BUY] = " + id.ToString()
                        + " UNION ALL SELECT [ACCOUNTID],[REFDOC_SOLD] AS [REFDOCID] FROM [dbo].[t_fin_account_ext_as] WHERE [REFDOC_SOLD] = " + id.ToString()
                        ;

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

                // Check current document is used for creating account for adp
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accID = reader.GetInt32(0);
                        break; // One doc shall be reference to one Account at maximum
                    }
                }
                reader.Dispose();
                reader = null;
                cmd.Dispose();
                cmd = null;

                if (accID == -1)
                {
                    // Check current document is referenced in template doc
                    queryString = @"SELECT [DOCID],[REFDOCID] FROM [dbo].[t_fin_tmpdoc_dp] WHERE [REFDOCID] = " + id.ToString()
                            + @" UNION ALL SELECT [DOCID],[REFDOCID] FROM [dbo].[t_fin_tmpdoc_loan] WHERE [REFDOCID] = " + id.ToString();

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tmpdocID = reader.GetInt32(0);
                            break; // One doc shall be reference to one tmp. doc at maximum
                        }
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;
                }
                else
                {
                    // Need fetch out the document posted already
                    queryString = @"SELECT [REFDOCID] FROM [dbo].[t_fin_tmpdoc_dp] WHERE [ACCOUNTID] = " + accID.ToString()
                            + @" AND [REFDOCID] IS NOT NULL UNION ALL SELECT [REFDOCID] FROM [dbo].[t_fin_tmpdoc_loan] WHERE [ACCOUNTID] = " + accID.ToString()
                            + " AND [REFDOCID] IS NOT NULL";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            listPostedID.Add(reader.GetInt32(0));
                        }
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;
                }

                // Checks have been applied, go ahead to deletion
                tran = conn.BeginTransaction();

                // Document header
                queryString = @"DELETE FROM [dbo].[t_fin_document] WHERE [ID] = " + id.ToString();
                cmd = new SqlCommand(queryString, conn, tran);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                // Document items
                queryString = @"DELETE FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + id.ToString();
                cmd = new SqlCommand(queryString, conn, tran);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                if (accID != -1)
                {
                    // Account deletion
                    queryString = @"DELETE FROM [t_fin_account] WHERE [ID] = " + accID.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // ADP account
                    queryString = @"DELETE FROM [dbo].[t_fin_account_ext_dp] WHERE [ACCOUNTID] = " + accID.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Loan account
                    queryString = @"DELETE FROM [dbo].[t_fin_account_ext_loan] WHERE [ACCOUNTID] = " + accID.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Asset account
                    queryString = @"DELETE FROM [dbo].[t_fin_account_ext_as] WHERE [ACCOUNTID] = " + accID.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Template doc - ADP
                    queryString = @"DELETE FROM [dbo].[t_fin_tmpdoc_dp] WHERE [ACCOUNTID] = " + accID.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Template doc - Loan
                    queryString = @"DELETE FROM [dbo].[t_fin_tmpdoc_loan] WHERE [ACCOUNTID] = " + accID.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Posted documents
                    foreach(Int32 npostid in listPostedID)
                    {
                        queryString = @"DELETE FROM [dbo].[t_fin_document] WHERE [ID] = " + npostid.ToString();
                        cmd = new SqlCommand(queryString, conn, tran);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        queryString = @"DELETE FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + npostid.ToString();
                        cmd = new SqlCommand(queryString, conn, tran);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;
                    }
                }
                else if (tmpdocID != -1)
                {
                    // Just clear the refernce id
                    queryString = @"UPDATE [dbo].[t_fin_tmpdoc_dp] SET [REFDOCID] = NULL WHERE [REFDOCID] = " + id.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Just clear the refernce id
                    queryString = @"UPDATE [dbo].[t_fin_tmpdoc_loan] SET [REFDOCID] = NULL WHERE [REFDOCID] = " + id.ToString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;
                }

                tran.Commit();
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();

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
                    conn = null;
                }
            }

            if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            return Ok();
        }

        // Checks
        private void BasicChecks(FinanceDocumentUIViewModel vm)
        {
            // Header check!
            if (String.IsNullOrEmpty(vm.Desp))
                throw new Exception("No Desp in the header");
            if (vm.DocType == 0)
                throw new Exception("Doc type is must!");

            // Check the items
            if (vm.Items.Count <= 0)
                throw new Exception("No item has been assigned yet");

            foreach (var item in vm.Items)
            {
                if (item.AccountID == 0 || item.TranAmount == 0 || item.TranType == 0)
                    throw new Exception("Item must have account or tran. amount or tran. type"); ;
                if (item.ControlCenterID == 0 && item.OrderID == 0)
                    throw new Exception("Must input control object");
                if (item.ControlCenterID > 0 && item.OrderID > 0)
                    throw new Exception("Either control center or order shall be inputted, not both");
                if (String.IsNullOrEmpty(item.Desp))
                    throw new Exception("Desp is a must for an item");
            }

        }

        private async Task BasicValidationAsync(FinanceDocumentUIViewModel vm, SqlConnection conn)
        {
            String strCheckString = @"SELECT TOP (1) [BASECURR] FROM [dbo].[t_homedef] WHERE [ID] = @hid;";
            SqlCommand cmdCheck = new SqlCommand(strCheckString, conn);
            cmdCheck.Parameters.AddWithValue("@hid", vm.HID);

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
            cmdCheck.Parameters.AddWithValue("@curr", vm.TranCurr);
            reader = await cmdCheck.ExecuteReaderAsync();
            if (!reader.HasRows)
                throw new Exception("No currency found");
            reader.Dispose();
            reader = null;
            cmdCheck.Dispose();
            cmdCheck = null;

            if (String.CompareOrdinal(vm.TranCurr, basecurr) != 0)
            {
                if (vm.ExgRate == 0)
                {
                    throw new Exception("No exchange rate info provided!");
                }
            }

            // Second currency
            if (!String.IsNullOrEmpty(vm.TranCurr2))
            {
                strCheckString = @"SELECT TOP (1) CURR from t_fin_currency WHERE curr = @curr;";
                cmdCheck = new SqlCommand(strCheckString, conn);
                cmdCheck.Parameters.AddWithValue("@curr", vm.TranCurr2);
                reader = await cmdCheck.ExecuteReaderAsync();
                if (!reader.HasRows)
                    throw new Exception("No currency found");

                reader.Dispose();
                reader = null;
                cmdCheck.Dispose();
                cmdCheck = null;

                if (String.CompareOrdinal(vm.TranCurr2, basecurr) != 0)
                {
                    if (vm.ExgRate2 == 0)
                    {
                        throw new Exception("No exchange rate info provided!");
                    }
                }
            }

            // Doc type
            strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_doc_type] WHERE [ID] = @ID"; // @"SELECT TOP (1) [ID] FROM [t_fin_doc_type] WHERE [HID] = @HID AND [ID] = @ID";
            cmdCheck = new SqlCommand(strCheckString, conn);
            //cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
            cmdCheck.Parameters.AddWithValue("@ID", vm.DocType);
            reader = await cmdCheck.ExecuteReaderAsync();
            if (!reader.HasRows)
                throw new Exception("No currency found");
            reader.Dispose();
            reader = null;
            cmdCheck.Dispose();
            cmdCheck = null;

            Decimal totalamount = 0;
            foreach (var item in vm.Items)
            {
                // Account
                strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_account] WHERE [HID] = @HID AND [ID] = @ID";
                cmdCheck = new SqlCommand(strCheckString, conn);
                cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
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
                //cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
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
                    cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
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
                    cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
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
                        if (vm.ExgRate2 > 0)
                        {
                            itemAmt = -1 * item.TranAmount * vm.ExgRate2 / 100;
                        }
                        else
                        {
                            itemAmt = -1 * item.TranAmount;
                        }
                    }
                    else
                    {
                        if (vm.ExgRate2 > 0)
                        {
                            itemAmt = item.TranAmount * vm.ExgRate2 / 100;
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
                        if (vm.ExgRate > 0)
                        {
                            itemAmt = -1 * item.TranAmount * vm.ExgRate / 100;
                        }
                        else
                        {
                            itemAmt = -1 * item.TranAmount;
                        }
                    }
                    else
                    {
                        if (vm.ExgRate > 0)
                        {
                            itemAmt = item.TranAmount * vm.ExgRate / 100;
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

            if (vm.DocType == FinanceDocTypeViewModel.DocType_Transfer || vm.DocType == FinanceDocTypeViewModel.DocType_CurrExchange)
            {
                if (totalamount != 0)
                {
                    throw new Exception("Amount must be zero");
                }
            }
        }
    }
}
