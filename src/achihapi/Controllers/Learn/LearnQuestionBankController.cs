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
using achihapi.Utilities;
using System.Net;

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

            BaseListViewModel<LearnQuestionBankViewModel> listVm = new BaseListViewModel<LearnQuestionBankViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = this.getQueryString(true, top, skip, null, hid);

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LearnQuestionBankViewModel vm = new LearnQuestionBankViewModel();
                            OnHeader2VM(reader, vm);

                            // Check the ID exist in the list already or not.
                            Boolean bExist = false;
                            foreach (LearnQuestionBankViewModel vm2 in listVm)
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
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

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

            LearnQuestionBankViewModel vm = new LearnQuestionBankViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = this.getQueryString(false, null, null, id, hid);

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

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
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
                    }

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
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
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
            SqlConnection conn = null;
            SqlTransaction tran = null;
            SqlCommand cmd = null;
            String queryString = "";
            Int32 nNewID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
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
                         VALUES (@HID
                               ,@Type
                               ,@Question
                               ,@BriefAnswer
                               ,@CREATEDBY
                               ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
                    cmd = new SqlCommand(queryString, conn)
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
                    foreach (var tag in vm.TagTerms)
                    {
                        queryString = HIHDBUtility.GetTagInsertString();

                        cmd = new SqlCommand(queryString, conn, tran);
                        HIHDBUtility.BindTagInsertParameter(cmd, vm.HID, HIHTagTypeEnum.LearnQuestionBank, nNewID, tag);

                        await cmd.ExecuteNonQueryAsync();

                        cmd.Dispose();
                        cmd = null;
                    }

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

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
            SqlConnection conn = null;
            SqlTransaction tran = null;
            SqlCommand cmd = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, vm.HID, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
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
                    cmd = new SqlCommand(queryString, conn)
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
                    queryString = HIHDBUtility.GetTagDeleteString();
                    cmd = new SqlCommand(queryString, conn, tran);
                    HIHDBUtility.BindTagDeleteParameter(cmd, vm.HID, HIHTagTypeEnum.LearnQuestionBank, id);

                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    foreach (var tag in vm.TagTerms)
                    {
                        queryString = HIHDBUtility.GetTagInsertString();

                        cmd = new SqlCommand(queryString, conn, tran);
                        HIHDBUtility.BindTagInsertParameter(cmd, vm.HID, HIHTagTypeEnum.LearnQuestionBank, id, tag);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();

                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

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

            // Update the database
            SqlConnection conn = null;
            SqlTransaction tran = null;
            SqlCommand cmd = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check Home assignment with current user
                    try
                    {
                        HIHAPIUtility.CheckHIDAssignment(conn, hid, usrName);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    tran = conn.BeginTransaction();

                    // Question bank
                    queryString = @"DELETE FROM [dbo].[t_learn_qtn_bank] WHERE [ID] = @ID AND [HID] = @HID";
                    cmd = new SqlCommand(queryString, conn)
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
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();

                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

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
