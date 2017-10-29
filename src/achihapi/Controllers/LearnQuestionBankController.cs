using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
                            OnDB2VM(reader, vm);

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

            LearnQuestionBankViewModel vm = null;
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = this.getQueryString(false, null, null, id, 0);

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
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (vm == null)
                        {
                            vm = new LearnQuestionBankViewModel();
                            OnDB2VM(reader, vm);
                        } 
                        else
                        {
                            LearnQuestionBankViewModel vm2 = new LearnQuestionBankViewModel();
                            OnDB2VM(reader, vm2);
                            vm.SubItemList.AddRange(vm2.SubItemList);
                        }
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

                queryString = @"";
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                // Now go ahead for the creating
                queryString = @"INSERT INTO [dbo].[t_learn_obj]
                                       ([HID]
                                       ,[CATEGORY]
                                       ,[NAME]
                                       ,[CONTENT]
                                       ,[CREATEDBY]
                                       ,[CREATEDAT]
                                       ,[UPDATEDBY]
                                       ,[UPDATEDAT])
                                 VALUES (@HID
                                       ,@CTGY
                                       ,@NAME
                                       ,@CONTENT
                                       ,@CREATEDBY
                                       ,@CREATEDAT
                                       ,@UPDATEDBY
                                       ,@UPDATEDAT
                                    ); SELECT @Identity = SCOPE_IDENTITY();";

                cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@HID", vm.HID);
                //cmd.Parameters.AddWithValue("@CTGY", vm.CategoryID);
                //cmd.Parameters.AddWithValue("@NAME", vm.Name);
                //cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
                cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
                cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
                SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                idparam.Direction = ParameterDirection.Output;

                Int32 nRst = await cmd.ExecuteNonQueryAsync();
                nNewID = (Int32)idparam.Value;
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
            {
                return StatusCode(500, strErrMsg);
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
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        #region Implemented methods
        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32 hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_qtn_bank] WHERE [HID] = " + hid.ToString() + "; ";
            }

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WITH ZQuestionBank_CTE (ID) AS ( SELECT [ID] FROM [dbo].[t_learn_qtn_bank] WHERE [HID] = " + hid.ToString()
                        + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + @" ROWS FETCH NEXT " + nTop.Value.ToString() + @" ROWS ONLY ) ";
                strSQL += @" SELECT [ZQuestionBank_CTE].[ID] ";
            }
            else
            {
                strSQL += @" SELECT [ID] ";
            }

            strSQL += @",[HID]
                        ,[Type]
                        ,[Question]
                        ,[BriefAnswer]
                        ,[CREATEDBY]
                        ,[CREATEDAT]
                        ,[UPDATEDBY]
                        ,[UPDATEDAT]
                        ,[SUBITEM]
                        ,[DETAIL]
                        ,[OTHERS]";

            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += " FROM [ZQuestionBank_CTE] LEFT OUTER JOIN [v_lrn_qtnbank] ON [ZQuestionBank_CTE].[ID] = [v_lrn_qtnbank].[ID] ORDER BY [ID] ";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " FROM [v_lrn_qtnbank] WHERE [ID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void OnDB2VM(SqlDataReader reader, LearnQuestionBankViewModel vm)
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

            if (!reader.IsDBNull(idx))
            {
                LearnQuestionBankSubItemViewModel si = new LearnQuestionBankSubItemViewModel();
                si.SubItem = reader.GetString(idx++);
                si.Detail = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    si.Others = reader.GetString(idx++);
                else
                    ++idx;

                vm.SubItemList.Add(si);
            }
        }
        #endregion
    }
}
