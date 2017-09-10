using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceOrderController : Controller
    {
        // GET: api/financeorder
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceOrderViewModel> listVMs = new BaseListViewModel<FinanceOrderViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getListQueryString(hid, top, skip);

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
                                FinanceOrderViewModel vm = new FinanceOrderViewModel();
                                this.onListDB2VM(reader, vm);
                                listVMs.Add(vm);
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(listVMs, setting);
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
                //queryString = this.getQueryString(false, null, null, id);
                queryString = this.getItemQueryString(id);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    //BaseListViewModel<FinanceOrderViewModel> listVM = new BaseListViewModel<FinanceOrderViewModel>();
                    //onDB2VM(reader, listVM);
                    //if (listVM.ContentList.Count == 1)
                    //{
                    //    vm = listVM.ContentList[0];
                    //}
                    //else
                    //{
                    //    throw new Exception("Failed to read db entry successfully!");
                    //}

                    this.onItemDB2VM(reader, vm);
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
        }

        // POST api/financeorder
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceOrderViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            // Check on name
            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

            // Check the s.rule
            if (vm.SRuleList.Count <= 0)
            {
                return BadRequest("No rule has been assigned yet");
            }
            Int32 nPer = 0;
            foreach(FinanceOrderSRuleUIViewModel svm in vm.SRuleList)
            {
                if (svm.Precent <= 0)
                {
                    return BadRequest("Percentage less than zero!");
                }

                nPer += svm.Precent;
            }
            if (nPer != 100 )
            {
                return BadRequest("Total percentage shall sum up to 100");
            }

            // ToDo: Check existence of the control center!!

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
            Int32 nDuplicatedID = -1;
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

                queryString = @"SELECT [ID]
                  FROM [dbo].[t_fin_order] WHERE [Name] = N'" + vm.Name + "'";

                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bDuplicatedEntry = true;
                    while (reader.Read())
                    {
                        nDuplicatedID = reader.GetInt32(0);
                        break;
                    }
                }
                else
                {
                    reader.Dispose();
                    reader = null;

                    cmd.Dispose();
                    cmd = null;

                    // Now go ahead for the creating
                    SqlTransaction tran = conn.BeginTransaction();

                    queryString = @"INSERT INTO [dbo].[t_fin_order]
                               ([NAME]
                               ,[VALID_FROM]
                               ,[VALID_TO]
                               ,[COMMENT]
                               ,[CREATEDBY]
                               ,[CREATEDAT]
                               ,[UPDATEDBY]
                               ,[UPDATEDAT])
                         VALUES
                               (@NAME
                               ,@VALID_FROM
                               ,@VALID_TO
                               ,@COMMENT
                               ,@CREATEDBY
                               ,@CREATEDAT
                               ,@UPDATEDBY
                               ,@UPDATEDAT); SELECT @Identity = SCOPE_IDENTITY();";

                    try
                    {
                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        cmd.Parameters.AddWithValue("@NAME", vm.Name);
                        cmd.Parameters.AddWithValue("@VALID_FROM", vm.ValidFrom);
                        cmd.Parameters.AddWithValue("@VALID_TO", vm.ValidTo);
                        cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
                        cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                        cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                        cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewID = (Int32)idparam.Value;

                        // Then, creating the srules
                        foreach (FinanceOrderSRuleUIViewModel suivm in vm.SRuleList)
                        {
                            queryString = @"INSERT INTO [dbo].[t_fin_order_srule]
                                               ([ORDID]
                                               ,[RULEID]
                                               ,[CONTROLCENTERID]
                                               ,[PRECENT]
                                               ,[COMMENT])
                                         VALUES
                                               (@ORDID
                                               ,@RULEID
                                               ,@CONTROLCENTERID
                                               ,@PRECENT
                                               ,@COMMENT)";
                            SqlCommand cmd2 = new SqlCommand(queryString, conn)
                            {
                                Transaction = tran
                            };
                            cmd2.Parameters.AddWithValue("@ORDID", nNewID);
                            cmd2.Parameters.AddWithValue("@RULEID", suivm.RuleID);
                            cmd2.Parameters.AddWithValue("@CONTROLCENTERID", suivm.ControlCenterID);
                            cmd2.Parameters.AddWithValue("@PRECENT", suivm.Precent);
                            cmd2.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(suivm.Comment) ? String.Empty : suivm.Comment);
                            await cmd2.ExecuteNonQueryAsync();
                        }

                        tran.Commit();
                    }
                    catch(Exception exp)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                        tran.Rollback();
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

            if (bDuplicatedEntry)
            {
                return BadRequest("Order already exists: " + nDuplicatedID.ToString());
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
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
        private string getListQueryString(Int32 hid, Int32 top, Int32 skip)
        {
            String strSQL = @"SELECT count(*) FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.ToString() + "; ";
            strSQL += @"SELECT [ID]
                      ,[HID]
                      ,[NAME]
                      ,[VALID_FROM]
                      ,[VALID_TO]
                      ,[COMMENT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.ToString();

            return strSQL;
        }
        private void onListDB2VM(SqlDataReader reader, FinanceOrderViewModel vm)
        {
            Int32 idx = 0;

            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.Name = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.ValidFrom = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.ValidTo = reader.GetDateTime(idx++);
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

        private string getItemQueryString(Int32 nID)
        {
            String strSQL = @"SELECT [ID]
                              ,[HID]
                              ,[NAME]
                              ,[VALID_FROM]
                              ,[VALID_TO]
                              ,[COMMENT]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [t_fin_order] WHERE [ID] = " + nID.ToString() + "; ";
            strSQL += @"SELECT [ID] AS [ORDID]
                              ,[RULEID]
                              ,[CONTROLCENTERID]
                              ,[CONTROLCENTERNAME]
                              ,[PRECENT]
                              ,[COMMENT]
                          FROM [v_fin_order_srule] WHERE [ID] = " + nID.ToString();

            return strSQL;
        }
        private void onItemDB2VM(SqlDataReader reader, FinanceOrderViewModel vm)
        {
            Int32 nRstBatch = 0;
            Int32 idx = 0;
            while (reader.HasRows)
            {
                if (nRstBatch == 0)
                {
                    idx = 0;
                    while (reader.Read())
                    {
                        vm.ID = reader.GetInt32(idx++);
                        vm.HID = reader.GetInt32(idx++);
                        vm.Name = reader.GetString(idx++);
                        if (!reader.IsDBNull(idx))
                            vm.ValidFrom = reader.GetDateTime(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vm.ValidTo = reader.GetDateTime(idx++);
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
                }
                else if (nRstBatch == 1)
                {
                    FinanceOrderSRuleUIViewModel srvm = new FinanceOrderSRuleUIViewModel();

                    while (reader.Read())
                    {
                        idx = 0;
                        srvm.OrdID = reader.GetInt32(idx++);
                        srvm.RuleID = reader.GetInt32(idx++);
                        srvm.ControlCenterID = reader.GetInt32(idx++);
                        if (!reader.IsDBNull(idx))
                            srvm.ControlCenterName = reader.GetString(idx++);
                        else
                            ++idx;
                        srvm.Precent = reader.GetInt32(idx++);
                        if (!reader.IsDBNull(idx))
                        {
                            srvm.Comment = reader.GetString(idx++);
                        }
                        else
                            ++idx;

                        vm.SRuleList.Add(srvm);
                    }
                }
                ++nRstBatch;

                reader.NextResult();
            }
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32? hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.Value.ToString() + "; ";
            }

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WITH ZOrder_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_fin_order] WHERE [HID] = " + hid.Value.ToString() 
                        + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
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
                    vm = new FinanceOrderViewModel
                    {
                        ID = nCurrentID,
                        Name = reader.GetString(idx++)
                    };
                    if (!reader.IsDBNull(idx))
                        vm.ValidFrom = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ValidTo = reader.GetDateTime(idx++);
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
                FinanceOrderSRuleUIViewModel srvm = new FinanceOrderSRuleUIViewModel
                {
                    RuleID = reader.GetInt32(idx++),
                    ControlCenterID = reader.GetInt32(idx++)
                };
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
