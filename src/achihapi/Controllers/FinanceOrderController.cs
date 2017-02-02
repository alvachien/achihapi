using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceOrderController : Controller
    {
        // GET: api/financeorder
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceOrderViewModel> listVMs = new BaseListViewModel<FinanceOrderViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getQueryString(true, top, skip, null);

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
                            this.onDB2VM(reader, listVMs);
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

        // GET api/financeorder/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            FinanceOrderViewModel vm = new FinanceOrderViewModel();

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
                    while (reader.Read())
                    {
                        BaseListViewModel<FinanceOrderViewModel> listVM = new BaseListViewModel<FinanceOrderViewModel>();
                        onDB2VM(reader, listVM);
                        if (listVM.ContentList.Count == 1)
                        {
                            vm = listVM.ContentList[0];
                        }
                        else
                        {
                            throw new Exception("Failed to read db entry successfully!");
                        }
                        break; // Should only one result!!!
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

        // POST api/financeorder
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financeorder/5
        [HttpPut("{id}")]
        [Authorize]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financeorder/5
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
        }

        #region Implmented methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_order];";
            }

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WITH ZOrder_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_fin_order]  ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
                strSQL += @" SELECT [ZOrder_CTE].[ID] ";
            }
            else
            {
                strSQL += @" SELECT [ID] ";
            }

            strSQL += @" ,[NAME]
                      ,[VALID_FROM]
                      ,[VALID_TO]
                      ,[COMMENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                      ,[RULEID]
                      ,[CONTROLCENTERID]
                      ,[CONTROLCENTERNAME]
                      ,[PRECENT] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += " FROM [ZOrder_CTE] LEFT OUTER JOIN [v_fin_order_srule] ON [ZOrder_CTE].[ID] = [v_fin_order_srule].[ID] ORDER BY [ID] ";
            }                
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " FROM [v_fin_order_srule] WHERE [ID] = " + nSearchID.Value.ToString();
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, BaseListViewModel<FinanceOrderViewModel> listVMs)
        {
            Int32 nOrderID = -1;
            while (reader.Read())
            {
                Int32 idx = 0;
                Int32 nCurrentID = reader.GetInt32(idx++);
                FinanceOrderViewModel vm = null;
                if (nOrderID != nCurrentID)
                {
                    nOrderID = nCurrentID;
                    vm = new FinanceOrderViewModel();

                    vm.ID = nCurrentID;
                    vm.Name = reader.GetString(idx++);
                    if (!reader.IsDBNull(idx))
                        vm.Valid_From = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.Valid_To = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.Comment = reader.GetString(idx++);
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
                    foreach (FinanceOrderViewModel ovm in listVMs)
                    {
                        if (ovm.ID == nCurrentID)
                        {
                            vm = ovm;
                            break;
                        }
                    }
                }

                idx = 9;
                FinanceOrderSRuleUIViewModel srvm = new FinanceOrderSRuleUIViewModel();
                srvm.RuleID = reader.GetInt32(idx++);
                srvm.ControlCenterID = reader.GetInt32(idx++);
                if (!reader.IsDBNull(idx))
                    srvm.ControlCenterName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    srvm.Precent = reader.GetInt32(idx++);
                else
                    ++idx;
                vm.SRuleList.Add(srvm);

                if (nOrderID != nCurrentID)
                {
                    listVMs.Add(vm);
                }
            }
        }
        #endregion
    }
}
