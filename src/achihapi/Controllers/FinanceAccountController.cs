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
            List<FinanceAccountViewModel> listVm = new List<FinanceAccountViewModel>();
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

                queryString = @"SELECT TOP (100) [ID]
                      ,[CTGYID]
                      ,[NAME]
                      ,[COMMENT]
                      ,[OWNER]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [dbo].[t_fin_account]";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceAccountViewModel vm = new FinanceAccountViewModel();
                        onDB2VM(reader, vm);
                        listVm.Add(vm);
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
            FinanceAccountViewModel vmAccount = new FinanceAccountViewModel();
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

                queryString = @"SELECT [ID]
                      ,[CTGYID]
                      ,[NAME]
                      ,[COMMENT]
                      ,[OWNER]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [dbo].[t_fin_account]
                  WHERE [ID] = " + id.ToString();

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

        private void onDB2VM(SqlDataReader reader, FinanceAccountViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            vm.CtgyID = reader.GetInt32(1);
            vm.Name = reader.GetName(2);
            if (!reader.IsDBNull(3))
                vm.Comment = reader.GetString(3);
            if (!reader.IsDBNull(4))
                vm.Owner = reader.GetString(4);
            if (!reader.IsDBNull(5))
                vm.CreatedBy = reader.GetString(5);
            if (!reader.IsDBNull(6))
                vm.CreatedAt = reader.GetDateTime(6);
            if (!reader.IsDBNull(7))
                vm.UpdatedBy = reader.GetString(7);
            if (!reader.IsDBNull(8))
                vm.UpdatedAt = reader.GetDateTime(8);
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

                    cmd = new SqlCommand(queryString, conn);
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
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financeaccount/5
        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
        }

        private string getQueryString(Boolean bIncContent, Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_obj];";
            }

            strSQL += @"SELECT [t_learn_obj].[ID]
                      ,[t_learn_obj].[CATEGORY]
                      ,[t_learn_ctgy].[NAME] as [CATEGORYNAME]
                      ,[t_learn_obj].[NAME]";
            if (bIncContent)
                strSQL += ",[t_learn_obj].[CONTENT] ";

            strSQL += @",[t_learn_obj].[CREATEDBY]
                      ,[t_learn_obj].[CREATEDAT]
                      ,[t_learn_obj].[UPDATEDBY]
                      ,[t_learn_obj].[UPDATEDAT] 
                        FROM [dbo].[t_learn_obj]
                            INNER JOIN [dbo].[t_learn_ctgy]
                        ON [t_learn_obj].[CATEGORY] = [t_learn_ctgy].[ID]";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " WHERE [t_learn_obj].[ID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, Boolean bIncContent, LearnObjectUIViewModel vm)
        {
            vm.ID = reader.GetInt32(0);
            vm.CategoryID = reader.GetInt32(1);
            vm.Name = reader.GetString(2);
            if (!reader.IsDBNull(3))
                vm.CategoryName = reader.GetString(3);
            else
                vm.CategoryName = String.Empty;

            Int32 idx = 4;
            if (bIncContent)
            {
                if (!reader.IsDBNull(idx))
                    vm.Content = reader.GetString(idx++);
            }
            if (!reader.IsDBNull(idx))
                vm.CreatedBy = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.CreatedAt = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                vm.UpdatedBy = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.UpdatedAt = reader.GetDateTime(idx++);
        }
    }
}
