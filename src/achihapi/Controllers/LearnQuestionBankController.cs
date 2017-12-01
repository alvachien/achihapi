using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LearnQuestionBank")]
    [Authorize]
    public class LearnQuestionBankController : Controller
    {
        // GET: api/LearnQuestionBank
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            BaseListViewModel<LearnQuestionBankViewModel> listVm = new BaseListViewModel<LearnQuestionBankViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = this.getQueryString(true, top, skip, null, hid);

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
                            LearnQuestionBankViewModel vm = new LearnQuestionBankViewModel();
                            OnHeader2VM(reader, vm);

                            // Check the ID exist in the list already or not.
                            Boolean bExist = false;
                            foreach(LearnQuestionBankViewModel vm2 in listVm)
                            {
                                if (vm2.ID == vm.ID)
                                {
                                    bExist = true;

                                    vm2.SubItemList.AddRange(vm.SubItemList);
                                }
                            }

                            if (!bExist)
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
                strErrMsg = exp.Message;
                bError = true;
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
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listVm, setting);
        }

        // GET: api/LearnQuestionBank/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            LearnQuestionBankViewModel vm = new LearnQuestionBankViewModel();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = this.getQueryString(false, null, null, id, hid);
            
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
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        OnHeader2VM(reader, vm);
                        break; // Shall only one item exist
                    }
                }
                else
                {
                    bNotFound = true;
                }

                if (!bNotFound)
                {
                    // Item
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LearnQuestionBankSubItemViewModel svm = new LearnQuestionBankSubItemViewModel
                            {
                                // QTNID
                                // [SUBITEM]
                                SubItem = reader.GetString(1),
                                // [DETAIL]
                                Detail = reader.GetString(2)
                            };
                            // [OTHERS]
                            if (!reader.IsDBNull(3))
                                svm.Others = reader.GetString(3);

                            vm.SubItemList.Add(svm);
                        }
                    }

                    // Tag
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            String strTag = reader.GetString(0);
                            vm.TagTerms.Add(strTag);
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

            return new JsonResult(vm, setting);
        }

        // POST: api/LearnQuestionBank
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LearnQuestionBankViewModel vm)
        {
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            if (vm == null)
                return BadRequest("No data is inputted");

            if (vm.HID <= 0)
                return BadRequest("No Home Inputted");

            // Check
            if (vm.Question != null)
                vm.Question = vm.Question.Trim();
            if (String.IsNullOrEmpty(vm.Question))
                return BadRequest("Question is a must!");

            if (vm.QuestionType == (Byte)HIHQuestionBankType.EssayQuestion
                || vm.QuestionType == (Byte)HIHQuestionBankType.MultipleChoice)
            {
            }
            else
            {
                // Non supported type
                return BadRequest("Non-supported type");
            }            

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewID = -1;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                tran = conn.BeginTransaction();

                // Question bank
                queryString = @"INSERT INTO [dbo].[t_learn_qtn_bank]
                               ([HID]
                               ,[Type]
                               ,[Question]
                               ,[BriefAnswer]
                               ,[CREATEDBY]
                               ,[CREATEDAT])
                         VALUES
                               (@HID
                               ,@Type
                               ,@Question
                               ,@BriefAnswer
                               ,@CREATEDBY
                               ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@HID", vm.HID);
                cmd.Parameters.AddWithValue("@Type", vm.QuestionType);
                cmd.Parameters.AddWithValue("@Question", vm.Question);
                if (!String.IsNullOrEmpty(vm.BriefAnswer))
                    cmd.Parameters.AddWithValue("@BriefAnswer", vm.BriefAnswer);
                else
                    cmd.Parameters.AddWithValue("@BriefAnswer", DBNull.Value);
                cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
                SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                idparam.Direction = ParameterDirection.Output;

                Int32 nRst = await cmd.ExecuteNonQueryAsync();
                nNewID = (Int32)idparam.Value;

                cmd.Dispose();
                cmd = null;

                // Question bank sub item
                foreach(var si in vm.SubItemList)
                {
                    queryString = @"INSERT INTO [dbo].[t_learn_qtn_bank_sub]
                                   ([QTNID]
                                   ,[SUBITEM]
                                   ,[DETAIL]
                                   ,[OTHERS])
                             VALUES (@QTNID
                                   ,@SUBITEM
                                   ,@DETAIL
                                   ,@OTHERS)";

                    cmd = new SqlCommand(queryString, conn, tran);
                    cmd.Parameters.AddWithValue("@QTNID", nNewID);
                    cmd.Parameters.AddWithValue("@SUBITEM", si.SubItem);
                    cmd.Parameters.AddWithValue("@DETAIL", si.Detail);
                    if (!String.IsNullOrEmpty(si.Others))
                        cmd.Parameters.AddWithValue("@OTHERS", si.Others);
                    else
                        cmd.Parameters.AddWithValue("@OTHERS", DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();

                    cmd.Dispose();
                    cmd = null;
                }

                // Tag
                foreach(var tag in vm.TagTerms)
                {
                    queryString = @"INSERT INTO [dbo].[t_tag]
                                           ([HID]
                                           ,[TagType]
                                           ,[TagID]
                                           ,[Term])
                                     VALUES (@HID
                                           ,@TagType
                                           ,@TagID
                                           ,@Term)";

                    cmd = new SqlCommand(queryString, conn, tran);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@TagType", HIHTagTypeEnum.LearnQuestionBank);
                    cmd.Parameters.AddWithValue("@TagID", nNewID);
                    cmd.Parameters.AddWithValue("@Term", tag);

                    await cmd.ExecuteNonQueryAsync();

                    cmd.Dispose();
                    cmd = null;
                }

                tran.Commit();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                tran.Rollback();
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
                return StatusCode(500, strErrMsg);

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }
        
        // PUT: api/LearnQuestionBank/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]LearnQuestionBankViewModel vm)
        {
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            if (vm == null)
                return BadRequest("No data is inputted");

            if (vm.HID <= 0)
                return BadRequest("No Home Inputted");

            // Check
            if (vm.ID != id)
                return BadRequest("Invalid data");
            if (vm.Question != null)
                vm.Question = vm.Question.Trim();
            if (String.IsNullOrEmpty(vm.Question))
                return BadRequest("Question is a must!");

            if (vm.QuestionType == (Byte)HIHQuestionBankType.EssayQuestion
                || vm.QuestionType == (Byte)HIHQuestionBankType.MultipleChoice)
            {
            }
            else
            {
                // Non supported type
                return BadRequest("Non-supported type");
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            SqlTransaction tran = null;
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                await conn.OpenAsync();

                // Check Home assignment with current user
                try
                {
                    HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }

                tran = conn.BeginTransaction();

                // Question bank
                queryString = @"UPDATE [dbo].[t_learn_qtn_bank]
                               SET [Type] = @Type
                                  ,[Question] = @Question
                                  ,[BriefAnswer] = @BriefAnswer
                                  ,[UPDATEDBY] = @UPDATEDBY
                                  ,[UPDATEDAT] = @UPDATEDAT
                             WHERE [HID] = @HID AND [ID] = @ID";
                SqlCommand cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@HID", vm.HID);
                cmd.Parameters.AddWithValue("@ID", vm.ID);
                cmd.Parameters.AddWithValue("@Type", vm.QuestionType);
                cmd.Parameters.AddWithValue("@Question", vm.Question);
                if (!String.IsNullOrEmpty(vm.BriefAnswer))
                    cmd.Parameters.AddWithValue("@BriefAnswer", vm.BriefAnswer);
                else
                    cmd.Parameters.AddWithValue("@BriefAnswer", DBNull.Value);
                cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
                cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);

                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                // Question bank sub item 
                queryString = @"DELETE FROM [dbo].[t_learn_qtn_bank_sub] WHERE [QTNID] = " + id.ToString();
                cmd = new SqlCommand(queryString, conn, tran);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                foreach (var si in vm.SubItemList)
                {
                    queryString = @"INSERT INTO [dbo].[t_learn_qtn_bank_sub]
                                   ([QTNID]
                                   ,[SUBITEM]
                                   ,[DETAIL]
                                   ,[OTHERS])
                             VALUES (@QTNID
                                   ,@SUBITEM
                                   ,@DETAIL
                                   ,@OTHERS)";

                    cmd = new SqlCommand(queryString, conn, tran);
                    cmd.Parameters.AddWithValue("@QTNID", id);
                    cmd.Parameters.AddWithValue("@SUBITEM", si.SubItem);
                    cmd.Parameters.AddWithValue("@DETAIL", si.Detail);
                    if (!String.IsNullOrEmpty(si.Others))
                        cmd.Parameters.AddWithValue("@OTHERS", si.Others);
                    else
                        cmd.Parameters.AddWithValue("@OTHERS", DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();

                    cmd.Dispose();
                    cmd = null;
                }

                // Tag
                queryString = @"DELETE FROM [dbo].[t_tag] WHERE [TagType] = @tagtype AND [TagID] = @tagid";
                cmd = new SqlCommand(queryString, conn, tran);
                cmd.Parameters.AddWithValue("@tagtype", HIHTagTypeEnum.LearnQuestionBank);
                cmd.Parameters.AddWithValue("@tagid", id);

                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                foreach (var tag in vm.TagTerms)
                {
                    queryString = @"INSERT INTO [dbo].[t_tag]
                                           ([HID]
                                           ,[TagType]
                                           ,[TagID]
                                           ,[Term])
                                     VALUES (@HID
                                           ,@TagType
                                           ,@TagID
                                           ,@Term)";

                    cmd = new SqlCommand(queryString, conn, tran);
                    cmd.Parameters.AddWithValue("@HID", vm.HID);
                    cmd.Parameters.AddWithValue("@TagType", HIHTagTypeEnum.LearnQuestionBank);
                    cmd.Parameters.AddWithValue("@TagID", id);
                    cmd.Parameters.AddWithValue("@Term", tag);

                    await cmd.ExecuteNonQueryAsync();
                }

                tran.Commit();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                tran.Rollback();
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
                return StatusCode(500, strErrMsg);

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery]Int32 hid = 0)
        {
            if (id <= 0)
                return BadRequest("Invalid ID");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            SqlTransaction tran = null;
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

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

                tran = conn.BeginTransaction();

                // Question bank
                queryString = @"DELETE FROM [dbo].[t_learn_qtn_bank] WHERE [ID] = @ID AND [HID] = @HID";
                SqlCommand cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@HID", hid);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                // Question bank sub
                queryString = @"DELETE FROM [dbo].[t_learn_qtn_bank_sub] WHERE [QTNID] = @QTNID";
                cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@QTNID", id);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                // Tags
                queryString = @"DELETE FROM [dbo].[t_tag] WHERE [TagType] = @tagtype AND [TagID] = @tagid AND [HID] = @HID";
                cmd = new SqlCommand(queryString, conn)
                {
                    Transaction = tran
                };
                cmd.Parameters.AddWithValue("@tagtype", HIHTagTypeEnum.LearnQuestionBank);
                cmd.Parameters.AddWithValue("@tagid", id);
                cmd.Parameters.AddWithValue("@HID", hid);
                await cmd.ExecuteNonQueryAsync();
                cmd.Dispose();
                cmd = null;

                tran.Commit();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                bError = true;
                strErrMsg = exp.Message;

                tran.Rollback();
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
                return StatusCode(500, strErrMsg);

            return Ok();
        }

         #region Implemented methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32 hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_qtn_bank] WHERE [HID] = " + hid.ToString() + "; ";
            }

            //if (bListMode && nTop.HasValue && nSkip.HasValue)
            //{
            //    strSQL += @" WITH ZQuestionBank_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_learn_qtn_bank] WHERE [HID] = " + hid.ToString()
            //            + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
            //    strSQL += @" SELECT [ZQuestionBank_CTE].[ID] ";
            //}
            //else
            //{
            //    strSQL += @" SELECT [ID] ";
            //}

            strSQL += @"SELECT [ID] 
                        ,[HID]
                        ,[Type]
                        ,[Question]
                        ,[BriefAnswer]
                        ,[CREATEDBY]
                        ,[CREATEDAT]
                        ,[UPDATEDBY]
                        ,[UPDATEDAT]";

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                //strSQL += " FROM [ZQuestionBank_CTE] LEFT OUTER JOIN [v_lrn_qtnbank] ON [ZQuestionBank_CTE].[ID] = [v_lrn_qtnbank].[ID] ORDER BY [ID] ";
                strSQL += " FROM [t_learn_qtn_bank] WHERE [HID] = " + hid.ToString() + " ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " FROM [t_learn_qtn_bank] WHERE [ID] = " + nSearchID.Value.ToString();
                strSQL += @"; SELECT [QTNID],[SUBITEM],[DETAIL],[OTHERS] FROM [dbo].[t_learn_qtn_bank_sub] WHERE [QTNID] = " + nSearchID.Value.ToString();
                strSQL += @"; SELECT [Term] FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString() + " AND [TagType] = 1 AND [TagID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void OnHeader2VM(SqlDataReader reader, LearnQuestionBankViewModel vm)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.QuestionType = reader.GetByte(idx++);
            vm.Question = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.BriefAnswer = reader.GetString(idx++);
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

            //if (!reader.IsDBNull(idx))
            //{
            //    LearnQuestionBankSubItemViewModel si = new LearnQuestionBankSubItemViewModel();
            //    si.SubItem = reader.GetString(idx++);
            //    si.Detail = reader.GetString(idx++);
            //    if (!reader.IsDBNull(idx))
            //        si.Others = reader.GetString(idx++);
            //    else
            //        ++idx;

            //    vm.SubItemList.Add(si);
            //}
        }
        #endregion
    }
}
