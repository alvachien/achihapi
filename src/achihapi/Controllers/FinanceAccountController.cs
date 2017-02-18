using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceAccountController : Controller
    {
        // GET: api/financeaccount
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceAccountUIViewModel> listVm = new BaseListViewModel<FinanceAccountUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
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
                var usrObj = User.FindFirst(c => c.Type == "sub");

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
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FinanceAccountUIViewModel vm = new FinanceAccountUIViewModel();
                                onDB2VM(reader, vm);
                                listVm.Add(vm);
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
            {
                return StatusCode(500, strErrMsg);
            }

            return new ObjectResult(listVm);
        }

        // GET api/financeaccount/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            FinanceAccountUIViewModel vmAccount = new FinanceAccountUIViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bExist = false;
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
                var usrObj = User.FindFirst(c => c.Type == "sub");

                queryString = this.getQueryString(false, null, null, id);

                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bExist = true;
                    while (reader.Read())
                    {
                        onDB2VM(reader, vmAccount);
                        // It should return one entry only!
                        // Nevertheless, ensure the code only execute once in API layer to keep toilence of dirty DB data;
                        break;
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

            // In case not found, return a 404
            if (!bExist)
                return NotFound();
            else if (bError)
                return StatusCode(500, strErrMsg);

            // Only return the meaningful object
            return new ObjectResult(vmAccount);
        }

        private void onDB2VM(SqlDataReader reader, FinanceAccountUIViewModel vm)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.CtgyID = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                vm.CtgyName = reader.GetString(idx++);
            else
                ++idx;
            vm.Name = reader.GetName(idx++);
            if (!reader.IsDBNull(idx))
                vm.Comment = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Owner = reader.GetString(idx++);
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

            if(vm.CtgyID == FinanceAccountViewModel.AccountCategory_AdvancePayment)
            {
                vm.AdvancePaymentInfo = new FinanceAccountExtDPViewModel();
                // Advance payment
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.Direct = reader.GetBoolean(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.StartDate = reader.GetDateTime(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.EndDate = reader.GetDateTime(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.RptType = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.RefDocID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.DefrrDays = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.AdvancePaymentInfo.Comment = reader.GetString(idx++);
                else
                    ++idx;
            }
        }

        // POST api/financeaccount
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAccountViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name))
            {
                return BadRequest("Name is a must!");
            }

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
                  FROM [dbo].[t_fin_account] WHERE [Name] = N'" + vm.Name + "'";

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

                    SqlTransaction tran = conn.BeginTransaction();

                    // Now go ahead for the creating
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

                    try
                    {
                        cmd = new SqlCommand(queryString, conn);
                        cmd.Transaction = tran;
                        cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
                        cmd.Parameters.AddWithValue("@NAME", vm.Name);
                        cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
                        if (String.IsNullOrEmpty(vm.Owner))
                        {
                            cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
                        }
                        cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                        cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                        cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewID = (Int32)idparam.Value;

                        if (vm.CtgyID == FinanceAccountViewModel.AccountCategory_AdvancePayment)
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
                            cmd.Parameters.AddWithValue("@ACCOUNTID", nNewID);
                            cmd.Parameters.AddWithValue("@DIRECT", vm.AdvancePaymentInfo.Direct);
                            cmd.Parameters.AddWithValue("@STARTDATE", vm.AdvancePaymentInfo.StartDate);
                            cmd.Parameters.AddWithValue("@ENDDATE", vm.AdvancePaymentInfo.EndDate);
                            cmd.Parameters.AddWithValue("@RPTTYPE", vm.AdvancePaymentInfo.RptType);
                            cmd.Parameters.AddWithValue("@REFDOCID", vm.AdvancePaymentInfo.RefDocID);
                            cmd.Parameters.AddWithValue("@DEFRRDAYS", vm.AdvancePaymentInfo.DefrrDays);
                            cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.AdvancePaymentInfo.Comment) ? String.Empty : vm.AdvancePaymentInfo.Comment);
                        }

                        // Now commit it!
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
                return BadRequest("Account already exists: " + nDuplicatedID.ToString());
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            return new ObjectResult(vm);
        }

        // PUT api/financeaccount/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]FinanceAccountViewModel vm)
        {            
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (vm.Name != null)
                vm.Name = vm.Name.Trim();
            if (String.IsNullOrEmpty(vm.Name) || vm.ID != id)
            {
                return BadRequest("Name is a must!");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                queryString = @"UPDATE [dbo].[t_fin_account]
                       SET [CTGYID] = @CTGYID
                          ,[NAME] = @NAME
                          ,[COMMENT] = @COMMENT
                          ,[OWNER] = @OWNER
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";

                await conn.OpenAsync();

                SqlTransaction tran = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
                cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
                cmd.Parameters.AddWithValue("@NAME", vm.Name);
                cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
                if (String.IsNullOrEmpty(vm.Owner))
                {
                    cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
                }
                cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                vm.UpdatedAt = DateTime.Now;
                cmd.Parameters.AddWithValue("@UPDATEDAT", vm.UpdatedAt);
                cmd.Parameters.AddWithValue("@ID", vm.ID);
                await cmd.ExecuteNonQueryAsync();

                // ToDo: Ext. info
                if (vm.CtgyID == FinanceAccountViewModel.AccountCategory_AdvancePayment)
                {
                    // UPDATE[dbo].[t_fin_account_ext_dp]
                    //  SET[ACCOUNTID] = < ACCOUNTID, int,>
                    //     ,[DIRECT] = <DIRECT, tinyint,>
                    //     ,[STARTDATE] = <STARTDATE, date,>
                    //     ,[ENDDATE] = <ENDDATE, date,>
                    //     ,[RPTTYPE] = <RPTTYPE, tinyint,>
                    //     ,[REFDOCID] = <REFDOCID, int,>
                    //     ,[DEFRRDAYS] = <DEFRRDAYS, nvarchar(100),>
                    //     ,[COMMENT] = <COMMENT, nvarchar(45),>
                    //WHERE<Search Conditions,,>
                    //if (vm.CtgyID == )
                    //{
                    //}
                }

                // Now commit it!
                tran.Commit();
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

            return new ObjectResult(vm);
        }

        // DELETE api/financeaccount/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // DELETE FROM [dbo].[t_fin_account]
            // WHERE [ID] = @ID

            // DELETE FROM [dbo].[t_fin_account_ext_dp]
            // WHERE [ACCOUNTID] = @ACCOUNTID

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                queryString = @"DELETE FROM [dbo].[t_fin_account] 
                     WHERE [ID] = @ID";

                await conn.OpenAsync();

                SqlTransaction tran = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
                cmd.Parameters.AddWithValue("@ID", id);
                await cmd.ExecuteNonQueryAsync();

                // Ext. info
                queryString = @"DELETE FROM [dbo].[t_fin_account_ext_dp] WHERE [ACCOUNTID] = @ACCOUNTID";
                cmd = new SqlCommand(queryString, conn);
                cmd.Transaction = tran;
                cmd.Parameters.AddWithValue("@ACCOUNTID", id);
                await cmd.ExecuteNonQueryAsync();

                // Now commit it!
                tran.Commit();
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

            return Ok();
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_fin_account];";
            }

            strSQL += @"SELECT [t_fin_account].[ID]
                      ,[t_fin_account].[CTGYID]
                      ,[t_fin_account_ctgy].[NAME] as [CTGYNAME]
                      ,[t_fin_account].[NAME]
                      ,[t_fin_account].[COMMENT]
                      ,[t_fin_account].[OWNER]
                      ,[t_fin_account].[CREATEDBY]
                      ,[t_fin_account].[CREATEDAT]
                      ,[t_fin_account].[UPDATEDBY]
                      ,[t_fin_account].[UPDATEDAT]
                      ,[t_fin_account_ext_dp].[DIRECT]
                      ,[t_fin_account_ext_dp].[STARTDATE]
                      ,[t_fin_account_ext_dp].[ENDDATE]
                      ,[t_fin_account_ext_dp].[RPTTYPE]
                      ,[t_fin_account_ext_dp].[REFDOCID]
                      ,[t_fin_account_ext_dp].[DEFRRDAYS]
                      ,[t_fin_account_ext_dp].[COMMENT]
                  FROM [dbo].[t_fin_account]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ctgy]
                       ON [t_fin_account].[CTGYID] = [t_fin_account_ctgy].[ID]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_dp]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_dp].[ACCOUNTID] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += @" WHERE [t_fin_account].[ID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }
    }
}
