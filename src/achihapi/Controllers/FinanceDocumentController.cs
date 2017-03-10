using System;
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
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceDocumentUIViewModel> listVMs = new BaseListViewModel<FinanceDocumentUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getListQueryString(top, skip);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        while (reader.Read())
                        {
                            listVMs.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceDocumentUIViewModel avm = new FinanceDocumentUIViewModel();
                                this.listDB2VM(reader, avm);

                                listVMs.Add(avm);
                            }
                        }
                    }
                    ++nRstBatch;

                    reader.NextResult();
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

            if (bError)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(listVMs);
        }

        // GET api/financedocument/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            FinanceDocumentUIViewModel vm = new FinanceDocumentUIViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = this.getQueryString(false, null, null, id);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                //if (reader.HasRows)
                //{
                //    BaseListViewModel<FinanceDocumentUIViewModel> listVM = new BaseListViewModel<FinanceDocumentUIViewModel>();
                //    onDB2VM(reader, listVM);

                //    if (listVM.ContentList.Count == 1)
                //    {
                //        vm = listVM.ContentList[0];
                //    }
                //    else
                //    {
                //        throw new Exception("Failed to read db entry successfully!");
                //    }
                //}
                //else
                //{
                //    bNotFound = true;
                //}

                Int32 nRstBatch = 0;
                Int32 idx = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        // Header
                        while (reader.Read())
                        {
                            idx = 0;
                            vm.ID = reader.GetInt32(idx++);
                            vm.DocType = reader.GetInt16(idx++);
                            vm.TranDate = reader.GetDateTime(idx++);
                            vm.TranCurr = reader.GetString(idx++);
                            if (!reader.IsDBNull(idx))
                                vm.Desp = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.ExgRate = reader.GetByte(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.ExgRate_Plan = reader.GetBoolean(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.TranCurr2 = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.ExgRate2 = reader.GetByte(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.ExgRate_Plan2 = reader.GetBoolean(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.CreatedBy = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.CreatedAt = reader.GetDateTime(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.UpdatedBy = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                vm.UpdatedAt = reader.GetDateTime(idx++);
                            else
                                ++idx;
                        }
                    }
                    else if (nRstBatch == 1)
                    {
                        // Items
                        while (reader.Read())
                        {
                            FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                            idx = 0;

                            itemvm.DocID = reader.GetInt32(idx++);
                            itemvm.ItemID = reader.GetInt32(idx++);
                            itemvm.AccountID = reader.GetInt32(idx++);
                            if (!reader.IsDBNull(idx))
                                itemvm.AccountName = reader.GetString(idx++);
                            else
                                ++idx;
                            itemvm.TranType = reader.GetInt32(idx++);
                            if (!reader.IsDBNull(idx))
                                itemvm.TranTypeName = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                itemvm.UseCurr2 = reader.GetBoolean(idx++);
                            else
                                ++idx;
                            itemvm.TranAmount = reader.GetDecimal(idx++);
                            if (!reader.IsDBNull(idx))
                                itemvm.ControlCenterID = reader.GetInt32(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                itemvm.ControlCenterName = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                itemvm.OrderID = reader.GetInt32(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                itemvm.OrderName = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                itemvm.Desp = reader.GetString(idx++);
                            else
                                ++idx;

                            vm.Items.Add(itemvm);
                        }
                    }
                    ++nRstBatch;

                    reader.NextResult();
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
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            // Todo: checks!
            // Header check!

            // Check the items
            if (vm.Items.Count <= 0)
            {
                return BadRequest("No item has been assigned yet");
            }

            // Item check!

            // Specific check: Advance payment 
            if (vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment)
            {
                //if (vm.AccountVM)
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nNewID = -1;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrName = User.FindFirst(c => c.Type == "sub").Value;

                await conn.OpenAsync();
                SqlTransaction tran = conn.BeginTransaction();

                SqlCommand cmd = null;

                // Now go ahead for the creating
                queryString = @"INSERT INTO [dbo].[t_fin_document]
                           ([DOCTYPE]
                           ,[TRANDATE]
                           ,[TRANCURR]
                           ,[DESP]
                           ,[EXGRATE]
                           ,[EXGRATE_PLAN]
                           ,[TRANCURR2]
                           ,[EXGRATE2]
                           ,[CREATEDBY]
                           ,[CREATEDAT]
                           ,[UPDATEDBY]
                           ,[UPDATEDAT])
                     VALUES
                           (@DOCTYPE
                           ,@TRANDATE
                           ,@TRANCURR
                           ,@DESP
                           ,@EXGRATE
                           ,@EXGRATE_PLAN
                           ,@TRANCURR2
                           ,@EXGRATE2
                           ,@CREATEDBY
                           ,@CREATEDAT
                           ,@UPDATEDBY
                           ,@UPDATEDAT); SELECT @Identity = SCOPE_IDENTITY();";

                try
                {
                    cmd = new SqlCommand(queryString, conn);
                    cmd.Transaction = tran;
                    cmd.Parameters.AddWithValue("@DOCTYPE", vm.DocType);
                    cmd.Parameters.AddWithValue("@TRANDATE", vm.TranDate);
                    cmd.Parameters.AddWithValue("@TRANCURR", vm.TranCurr);
                    cmd.Parameters.AddWithValue("@DESP", vm.Desp);
                    if (vm.ExgRate > 0)
                        cmd.Parameters.AddWithValue("@EXGRATE", vm.ExgRate);
                    else
                        cmd.Parameters.AddWithValue("@EXGRATE", DBNull.Value);
                    if (vm.ExgRate_Plan)
                        cmd.Parameters.AddWithValue("@EXGRATE_PLAN", vm.ExgRate_Plan);
                    else
                        cmd.Parameters.AddWithValue("@EXGRATE_PLAN", DBNull.Value);
                    if (String.IsNullOrEmpty(vm.TranCurr2))
                        cmd.Parameters.AddWithValue("@TRANCURR2", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@TRANCURR2", vm.TranCurr2);
                    if (vm.ExgRate2 > 0)
                        cmd.Parameters.AddWithValue("@EXGRATE2", vm.ExgRate2);
                    else
                        cmd.Parameters.AddWithValue("@EXGRATE2", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                    cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                    cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = @"INSERT INTO [dbo].[t_fin_document_item]
                                       ([DOCID]
                                       ,[ITEMID]
                                       ,[ACCOUNTID]
                                       ,[TRANTYPE]
                                       ,[TRANAMOUNT]
                                       ,[USECURR2]
                                       ,[CONTROLCENTERID]
                                       ,[ORDERID]
                                       ,[DESP])
                                 VALUES
                                       (@DOCID
                                       ,@ITEMID
                                       ,@ACCOUNTID
                                       ,@TRANTYPE
                                       ,@TRANAMOUNT
                                       ,@USECURR2
                                       ,@CONTROLCENTERID
                                       ,@ORDERID
                                       ,@DESP)";
                        SqlCommand cmd2 = new SqlCommand(queryString, conn);
                        cmd2.Transaction = tran;
                        cmd2.Parameters.AddWithValue("@DOCID", nNewID);
                        cmd2.Parameters.AddWithValue("@ITEMID", ivm.ItemID);
                        cmd2.Parameters.AddWithValue("@ACCOUNTID", ivm.AccountID);
                        cmd2.Parameters.AddWithValue("@TRANTYPE", ivm.TranType);
                        cmd2.Parameters.AddWithValue("@TRANAMOUNT", ivm.TranAmount);
                        if (ivm.UseCurr2)
                            cmd2.Parameters.AddWithValue("@USECURR2", ivm.UseCurr2);
                        else
                            cmd2.Parameters.AddWithValue("@USECURR2", DBNull.Value);
                        if (ivm.ControlCenterID > 0)
                            cmd2.Parameters.AddWithValue("@CONTROLCENTERID", ivm.ControlCenterID);
                        else
                            cmd2.Parameters.AddWithValue("@CONTROLCENTERID", DBNull.Value);
                        if (ivm.OrderID > 0)
                            cmd2.Parameters.AddWithValue("@ORDERID", ivm.OrderID);
                        else
                            cmd2.Parameters.AddWithValue("@ORDERID", DBNull.Value);
                        cmd2.Parameters.AddWithValue("@DESP", String.IsNullOrEmpty(ivm.Desp) ? String.Empty : ivm.Desp);
                        await cmd2.ExecuteNonQueryAsync();
                    }

                    if (vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment)
                    {
                        // Three parts:
                        //  0. Template docs
                        //queryString = @"INSERT INTO [dbo].[t_fin_tmpdoc_dp]
                        //               ([DOCID]
                        //               ,[REFDOCID]
                        //               ,[ACCOUNTID]
                        //               ,[TRANDATE]
                        //               ,[TRANTYPE]
                        //               ,[TRANAMOUNT]
                        //               ,[CONTROLCENTERID]
                        //               ,[ORDERID]
                        //               ,[DESP]
                        //               ,[CREATEDBY]
                        //               ,[CREATEDAT]
                        //               ,[UPDATEDBY]
                        //               ,[UPDATEDAT])
                        //         VALUES
                        //               (@DOCID
                        //               ,@REFDOCID
                        //               ,@ACCOUNTID
                        //               ,@TRANDATE
                        //               ,@TRANTYPE
                        //               ,@TRANAMOUNT
                        //               ,@CONTROLCENTERID
                        //               ,@ORDERID
                        //               ,@DESP
                        //               ,@CREATEDBY
                        //               ,@CREATEDAT
                        //               ,@UPDATEDBY
                        //               ,@UPDATEDAT)";

                        //  1. Account
                        queryString = @"INSERT INTO [dbo].[t_fin_account]
                                       ([CTGYID]
                                       ,[NAME]
                                       ,[COMMENT]
                                       ,[OWNER]
                                       ,[CREATEDBY]
                                       ,[CREATEDAT]
                                       ,[UPDATEDBY]
                                       ,[UPDATEDAT])
                                 VALUES
                                       (@CTGYID
                                       ,@NAME
                                       ,@COMMENT
                                       ,@OWNER
                                       ,@CREATEDBY
                                       ,@CREATEDAT
                                       ,@UPDATEDBY
                                       ,@UPDATEDAT
                                    ); SELECT @Identity = SCOPE_IDENTITY();";

                        cmd = new SqlCommand(queryString, conn);
                        cmd.Transaction = tran;
                        cmd.Parameters.AddWithValue("@CTGYID", vm.AccountVM.CtgyID);
                        cmd.Parameters.AddWithValue("@NAME", vm.AccountVM.Name);
                        cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.AccountVM.Comment) ? String.Empty : vm.AccountVM.Comment);
                        if (String.IsNullOrEmpty(vm.AccountVM.Owner))
                        {
                            cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@OWNER", vm.AccountVM.Owner);
                        }
                        cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@CREATEDAT", vm.AccountVM.CreatedAt);
                        cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                        cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);

                        SqlParameter idparam2 = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam2.Direction = ParameterDirection.Output;

                        nRst = await cmd.ExecuteNonQueryAsync();
                        Int32 nNewAccountID = (Int32)idparam2.Value;

                        //  2. Advance payment account
                        if (vm.AccountVM.CtgyID == FinanceAccountViewModel.AccountCategory_AdvancePayment)
                        {
                            queryString = @"INSERT INTO [dbo].[t_fin_account_ext_dp]
                                        ([ACCOUNTID]
                                        ,[DIRECT]
                                        ,[STARTDATE]
                                        ,[ENDDATE]
                                        ,[RPTTYPE]
                                        ,[REFDOCID]
                                        ,[DEFRRDAYS]
                                        ,[COMMENT])
                                    VALUES
                                        (@ACCOUNTID
                                        ,@DIRECT
                                        ,@STARTDATE
                                        ,@ENDDATE
                                        ,@RPTTYPE
                                        ,@REFDOCID
                                        ,@DEFRRDAYS
                                        ,@COMMENT )";

                            cmd = new SqlCommand(queryString, conn);
                            cmd.Transaction = tran;
                            cmd.Parameters.AddWithValue("@ACCOUNTID", nNewAccountID);
                            cmd.Parameters.AddWithValue("@DIRECT", vm.AccountVM.AdvancePaymentInfo.Direct);
                            cmd.Parameters.AddWithValue("@STARTDATE", vm.AccountVM.AdvancePaymentInfo.StartDate);
                            cmd.Parameters.AddWithValue("@ENDDATE", vm.AccountVM.AdvancePaymentInfo.EndDate);
                            cmd.Parameters.AddWithValue("@RPTTYPE", vm.AccountVM.AdvancePaymentInfo.RptType);
                            cmd.Parameters.AddWithValue("@REFDOCID", vm.AccountVM.AdvancePaymentInfo.RefDocID);
                            cmd.Parameters.AddWithValue("@DEFRRDAYS", vm.AccountVM.AdvancePaymentInfo.DefrrDays);
                            cmd.Parameters.AddWithValue("@COMMENT", 
                                String.IsNullOrEmpty(vm.AccountVM.AdvancePaymentInfo.Comment) ? String.Empty : vm.AccountVM.AdvancePaymentInfo.Comment);

                            await cmd.ExecuteNonQueryAsync();
                        }




                    }

                    tran.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            return new ObjectResult(vm);
        }

        // PUT api/financedocument/5
        [HttpPut("{id}")]
        [Authorize]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financedocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
        }

        #region Implmented methods
        private string getListQueryString(Int32 top, Int32 skip)
        {
            String strSQL = @"SELECT count(*) FROM [dbo].[v_fin_document];";
            strSQL += @" SELECT [v_fin_document].[ID]
                      ,[v_fin_document].[DOCTYPE]
	                  ,[v_fin_document].[DOCTYPENAME]
                      ,[v_fin_document].[TRANDATE]
                      ,[v_fin_document].[TRANCURR]
                      ,[v_fin_document].[DESP]
                      ,[v_fin_document].[EXGRATE]
                      ,[v_fin_document].[EXGRATE_PLAN]
                      ,[v_fin_document].[TRANCURR2]
                      ,[v_fin_document].[EXGRATE2]
                      ,[v_fin_document].[EXGRATE_PLAN2]
                      ,[v_fin_document].[CREATEDBY]
                      ,[v_fin_document].[CREATEDAT]
                      ,[v_fin_document].[UPDATEDBY]
                      ,[v_fin_document].[UPDATEDAT]
                      ,[v_fin_document].[TRANAMOUNT]
                  FROM [dbo].[v_fin_document]";

            return strSQL;
        }
        private void listDB2VM(SqlDataReader reader, FinanceDocumentUIViewModel vm)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.DocType = reader.GetInt16(idx++);
            vm.DocTypeName = reader.GetString(idx++);
            vm.TranDate = reader.GetDateTime(idx++);
            vm.TranCurr = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.Desp = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ExgRate = reader.GetByte(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ExgRate_Plan = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.TranCurr2 = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ExgRate2 = reader.GetByte(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ExgRate_Plan2 = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CreatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedBy = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.UpdatedAt = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.TranAmount = reader.GetDecimal(idx++);
            else
                ++idx;
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID)
        {
            String strSQL = @"SELECT [ID]
                          ,[DOCTYPE]
                          ,[TRANDATE]
                          ,[TRANCURR]
                          ,[DESP]
                          ,[EXGRATE]
                          ,[EXGRATE_PLAN]
                          ,[TRANCURR2]
                          ,[EXGRATE2]
                          ,[EXGRATE_PLAN2]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [t_fin_document] WHERE [ID] = " + nSearchID.Value.ToString() + @"; 
                    SELECT [DOCID]
                            ,[ITEMID]
                            ,[ACCOUNTID]
                            ,[ACCOUNTNAME]
                            ,[TRANTYPE]
                            ,[TRANTYPENAME]
                            ,[USECURR2]
                            ,[TRANAMOUNT_ORG] AS [TRANAMOUNT]
                            ,[CONTROLCENTERID]
                            ,[CONTROLCENTERNAME]
                            ,[ORDERID]
                            ,[ORDERNAME]
                            ,[DESP]
                        FROM [v_fin_document_item1] WHERE [DOCID] = " + nSearchID.Value.ToString();

            //            if (bListMode)
            //            {
            //                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_document];";
            //            }

            //            if (bListMode && nTop.HasValue && nSkip.HasValue)
            //            {
            //                strSQL += @" WITH ZDoc_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_fin_document]  ORDER BY (SELECT NULL)
            //                        OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
            //                strSQL += @" SELECT [ZOrder_CTE].[ID] ";
            //            }
            //            else
            //            {
            //                strSQL += @" SELECT [ID] ";
            //            }

            //            strSQL += @" ,[DOCTYPE]
            //                      ,[DOCTYPENAME]
            //                      ,[TRANDATE]
            //                      ,[TRANCURR]
            //                      ,[DESP]
            //                      ,[EXGRATE]
            //                      ,[EXGRATE_PLAN]
            //                      ,[TRANCURR2]
            //                      ,[EXGRATE2]
            //                      ,[CREATEDBY]
            //                      ,[CREATEDAT]
            //                      ,[UPDATEDBY]
            //                      ,[UPDATEDAT]
            //                      ,[ITEMID]
            //                      ,[ACCOUNTID]
            //                      ,[ACCOUNTNAME]
            //                      ,[TRANTYPE]
            //                      ,[TRANAMOUNT]
            //                      ,[USECURR2]
            //                      ,[CONTROLCENTERID]
            //                      ,[CONTROLCENTERNAME]
            //                      ,[ORDERID]
            //                      ,[ORDERNAME]
            //                      ,[ITEMDESP] ";
            //            if (bListMode && nTop.HasValue && nSkip.HasValue)
            //            {
            //                strSQL += @" FROM [ZOrder_CTE] LEFT OUTER JOIN [v_fin_document_item] ON [ZOrder_CTE].[ID] = [v_fin_document_item].[ID] ORDER BY [ID] ";
            //            }
            //            else if (!bListMode && nSearchID.HasValue)
            //            {
            //                strSQL += @" FROM [v_fin_document_item1] WHERE [ID] = " + nSearchID.Value.ToString();
            //            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif


            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, BaseListViewModel<FinanceDocumentUIViewModel> listVMs)
        {
            Int32 nDocID = -1;
            while (reader.Read())
            {
                Int32 idx = 0;
                Int32 nCurrentID = reader.GetInt32(idx++);
                FinanceDocumentUIViewModel vm = null;
                if (nDocID != nCurrentID)
                {
                    nDocID = nCurrentID;
                    vm = new FinanceDocumentUIViewModel();

                    vm.ID = nCurrentID;
                    vm.DocType = reader.GetInt16(idx++);
                    vm.DocTypeName = reader.GetString(idx++);
                    vm.TranDate = reader.GetDateTime(idx++);
                    vm.TranCurr = reader.GetString(idx++);
                    if (!reader.IsDBNull(idx))
                        vm.Desp = reader.GetString(idx++);
                    else
                        ++idx;                    
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate = reader.GetByte(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate_Plan = reader.GetBoolean(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.TranCurr2 = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate2 = reader.GetByte(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.CreatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.CreatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.UpdatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.UpdatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                }
                else
                {
                    foreach (FinanceDocumentUIViewModel ovm in listVMs)
                    {
                        if (ovm.ID == nCurrentID)
                        {
                            vm = ovm;
                            break;
                        }
                    }
                }

                idx = 14; // Item part
                FinanceDocumentItemUIViewModel divm = new FinanceDocumentItemUIViewModel();
                if (!reader.IsDBNull(idx))
                    divm.ItemID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.AccountID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.AccountName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.TranType = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.TranAmount = reader.GetDecimal(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.UseCurr2 = reader.GetBoolean(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.ControlCenterID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.ControlCenterName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.OrderID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.OrderName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.Desp = reader.GetString(idx++);
                else
                    ++idx;
                vm.Items.Add(divm);

                if (nDocID != nCurrentID)
                {
                    listVMs.Add(vm);
                }
            }
        }
        #endregion
    }
}
