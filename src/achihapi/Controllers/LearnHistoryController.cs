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
    public class LearnHistoryController : Controller
    {
        // GET: api/learnhistory
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<LearnHistoryUIViewModel> listVm = new BaseListViewModel<LearnHistoryUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getSQLString(true, top, skip, null, null);

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
                        while (reader.Read())
                        {
                            LearnHistoryUIViewModel vm = new LearnHistoryUIViewModel();
                            onDB2VM(reader, vm);
                            listVm.Add(vm);
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

            return new ObjectResult(listVm);
        }

        private string getSQLString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nUserID, Int32? nObjID)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_hist];";
            }

            strSQL += @"SELECT [t_learn_hist].[USERID]
                      ,[t_userdetail].[DISPLAYAS] as [USERDISPLAYAS]
                      ,[t_learn_hist].[OBJECTID]
                      ,[t_learn_obj].[NAME] as [OBJECTNAME]
                      ,[t_learn_hist].[LEARNDATE]
                      ,[t_learn_hist].[COMMENT]
                      ,[t_learn_hist].[CREATEDBY]
                      ,[t_learn_hist].[CREATEDAT]
                      ,[t_learn_hist].[UPDATEDBY]
                      ,[t_learn_hist].[UPDATEDAT] 
                        FROM [dbo].[t_learn_hist]
                            INNER JOIN [dbo].[t_userdetail] ON [t_learn_hist].[USERID] = [t_userdetail].[USERID]
                            INNER JOIN [dbo].[t_learn_obj] ON [t_learn_hist].[OBJECTID] = [t_learn_obj].[ID]
                        ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" ORDER BY (SELECT NULL)
                        OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nUserID.HasValue && nObjID.HasValue)
            {
                strSQL += " WHERE [t_learn_hist].[USERID] = " + nUserID.Value.ToString() + " AND [t_learn_hist].[OBJECTID] = " + nObjID.Value.ToString();
            }

            return strSQL;
        }

        private void onDB2VM(SqlDataReader reader, LearnHistoryUIViewModel vm)
        {
            Int32 idx = 0;
            vm.UserID = reader.GetString(idx++);
            vm.UserDisplayAs = reader.GetString(idx++);
            vm.ObjectID = reader.GetInt32(idx++);
            vm.ObjectName = reader.GetString(idx++);
            vm.LearnDate = reader.GetDateTime(idx++);
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

        // GET api/learnhistory/5
        [HttpGet("{id}")]
        public string Get(String sid)
        {
            return "value";
        }

        // POST api/learnhistory, create a learn history
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]LearnHistoryViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            var usr = User.FindFirst(c => c.Type == "sub");
            String usrName = String.Empty;
            if (usr != null)
                usrName = usr.Value;

            try
            {
                await conn.OpenAsync();

                // Do the check first: object id
                String checkString = @"SELECT [ID]
                            FROM [dbo].[t_learn_obj] WHERE [ID] = " + vm.ObjectID.ToString();
                SqlCommand chkcmd = new SqlCommand(checkString, conn);
                SqlDataReader chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid Object ID : " + vm.ObjectID.ToString());
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Do the check: name
                checkString = @"SELECT [USERID] FROM [dbo].[t_userdetail] WHERE [USERID] = N'" + vm.UserID + "'";
                chkcmd = new SqlCommand(checkString, conn);
                chkreader = chkcmd.ExecuteReader();
                if (!chkreader.HasRows)
                {
                    return BadRequest("Invalid user ID : " + vm.UserID);
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Do the check: primary key check
                checkString = @"SELECT [USERID], [OBJECTID], [LEARNDATE] FROM [dbo].[t_learn_hist] WHERE [USERID] = @USERID AND [OBJECTID] = @OBJECTID AND [LEARNDATE] = @LEARNDATE";
                chkcmd = new SqlCommand(checkString, conn);
                chkcmd.Parameters.AddWithValue("@USERID", vm.UserID);
                chkcmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                chkcmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                chkreader = chkcmd.ExecuteReader();
                if (chkreader.HasRows)
                {
                    return BadRequest("Duplicated entry: user (" + vm.UserID + "), object (" + vm.ObjectID.ToString() + ") at date (" + vm.LearnDate.ToString() + ")");
                }
                chkreader.Dispose();
                chkreader = null;
                chkcmd.Dispose();
                chkcmd = null;

                // Now go ahead for the creating
                queryString = @"INSERT INTO [dbo].[t_learn_hist]
                           ([USERID]
                           ,[OBJECTID]
                           ,[LEARNDATE]
                           ,[COMMENT]
                           ,[CREATEDBY]
                           ,[CREATEDAT]
                           ,[UPDATEDBY]
                           ,[UPDATEDAT])
                     VALUES
                           (@USERID
                           ,@OBJECTID
                           ,@LEARNDATE
                           ,@COMMENT
                           ,@CREATEDBY
                           ,@CREATEDAT
                           ,@UPDATEDBY
                           ,@UPDATEDAT);";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@USERID", vm.UserID);
                cmd.Parameters.AddWithValue("@OBJECTID", vm.ObjectID);
                cmd.Parameters.AddWithValue("@LEARNDATE", vm.LearnDate);
                cmd.Parameters.AddWithValue("@COMMENT", vm.Comment);
                cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);

                Int32 nRst = await cmd.ExecuteNonQueryAsync();
                //if (nRst )
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

            return new ObjectResult(vm);
        }

        // PUT api/learnhistory/5_a, change
        [HttpPut("{id}")]
        public void Put(string sid, [FromBody]string value)
        {
        }

        // DELETE api/learnhistory/5
        [HttpDelete("{id}")]
        public void Delete(string sid)
        {
        }
    }
}
