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
                if (reader.HasRows)
                {
                    BaseListViewModel<FinanceDocumentUIViewModel> listVM = new BaseListViewModel<FinanceDocumentUIViewModel>();
                    onDB2VM(reader, listVM);

                    if (listVM.ContentList.Count == 1)
                    {
                        vm = listVM.ContentList[0];
                    }
                    else
                    {
                        throw new Exception("Failed to read db entry successfully!");
                    }
                }
                else
                {
                    bNotFound = true;
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
        public void Post([FromBody]string value)
        {
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
            String strSQL = @"SELECT count(*) FROM [dbo].[t_fin_document];";
            strSQL += @" SELECT [t_fin_document].[ID]
                      ,[t_fin_document].[DOCTYPE]
	                  ,[t_fin_doc_type].[NAME] AS [DOCTYPENAME]
                      ,[t_fin_document].[TRANDATE]
                      ,[t_fin_document].[TRANCURR]
                      ,[t_fin_document].[DESP]
                      ,[t_fin_document].[EXGRATE]
                      ,[t_fin_document].[EXGRATE_PLAN]
                      ,[t_fin_document].[TRANCURR2]
                      ,[t_fin_document].[EXGRATE2]
                      ,[t_fin_document].[CREATEDBY]
                      ,[t_fin_document].[CREATEDAT]
                      ,[t_fin_document].[UPDATEDBY]
                      ,[t_fin_document].[UPDATEDAT]
                  FROM [dbo].[t_fin_document]
	                LEFT OUTER JOIN [dbo].[t_fin_doc_type]
	                ON [dbo].[t_fin_document].[DOCTYPE] = [dbo].[t_fin_doc_type].[ID]";

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

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_document];";
            }

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WITH ZDoc_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_fin_document]  ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
                strSQL += @" SELECT [ZOrder_CTE].[ID] ";
            }
            else
            {
                strSQL += @" SELECT [ID] ";
            }

            strSQL += @" ,[DOCTYPE]
                      ,[DOCTYPENAME]
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
                      ,[UPDATEDAT]
                      ,[ITEMID]
                      ,[ACCOUNTID]
                      ,[ACCOUNTNAME]
                      ,[TRANTYPE]
                      ,[TRANAMOUNT]
                      ,[USECURR2]
                      ,[CONTROLCENTERID]
                      ,[CONTROLCENTERNAME]
                      ,[ORDERID]
                      ,[ORDERNAME]
                      ,[ITEMDESP] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" FROM [ZOrder_CTE] LEFT OUTER JOIN [v_fin_document_item] ON [ZOrder_CTE].[ID] = [v_fin_document_item].[ID] ORDER BY [ID] ";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += @" FROM [v_fin_document_item] WHERE [ID] = " + nSearchID.Value.ToString();
            }

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
