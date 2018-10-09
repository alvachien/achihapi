using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using achihapi.Utilities;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LearnEnSentence")]
    [Authorize]
    public class LearnEnSentenceController : Controller
    {
        // GET: api/LearnEnSentence
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

            BaseListViewModel<LearnEnSentenceViewModel> listVm = new BaseListViewModel<LearnEnSentenceViewModel>();
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
                            LearnEnSentenceViewModel vm = new LearnEnSentenceViewModel();
                            OnSentenceHeader2VM(reader, vm);
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

        // GET: api/LearnEnSentence/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            LearnEnSentenceViewModel vm = new LearnEnSentenceViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = this.getQueryString(false, null, null, id, hid);

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

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    // Header
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OnSentenceHeader2VM(reader, vm);
                            break; // Should only one result!!!
                        }
                    }
                    else
                    {
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
                    }
                    await reader.NextResultAsync();

                    // Explains
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // Explain
                            var vmExp = new LearnEnSentenceExpViewModel();
                            OnSentenceExplain2VM(reader, vmExp);
                            vm.Explains.Add(vmExp);
                        }
                    }
                    await reader.NextResultAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // Word ID
                            vm.RelatedWordIDs.Add(reader.GetInt32(0));
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

        // POST: api/LearnEnSentence
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LearnEnSentenceViewModel vm)
        {
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }
            if (vm.HID <= 0)
                return BadRequest("No Home Inputted");

            if (vm.Sentence != null)
                vm.Sentence = vm.Sentence.Trim();
            if (String.IsNullOrEmpty(vm.Sentence))
            {
                return BadRequest("Sentence is a must!");
            }
            if (vm.Explains == null || vm.Explains.Count <= 0)
                return BadRequest("Explain is a must");

            // Update the database
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                queryString = @"SELECT [ID] FROM [dbo].[t_learn_ensent] WHERE [Word] = N'" + vm.Sentence + "' AND [HID] = " + vm.HID.ToString();

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

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Int32 nDuplicatedID = -1;
                        while (reader.Read())
                        {
                            nDuplicatedID = reader.GetInt32(0);
                            break;
                        }

                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("Object with name already exists: " + nDuplicatedID.ToString());
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        // Now go ahead for the creating
                        queryString = @"INSERT INTO [dbo].[t_learn_ensent]
                                    ([HID]
                                    ,[Sentence]
                                    ,[CREATEDBY]
                                    ,[CREATEDAT])
                                VALUES (@HID
                                    ,@Sentence
                                    ,@CREATEDBY
                                    ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";

                        tran = conn.BeginTransaction();

                        // Header
                        cmd = new SqlCommand(queryString, conn, tran);

                        cmd.Parameters.AddWithValue("@HID", vm.HID);
                        cmd.Parameters.AddWithValue("@Sentence", vm.Sentence);
                        cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
                        cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewID = (Int32)idparam.Value;
                        vm.ID = nNewID;

                        cmd.Dispose();
                        cmd = null;

                        // Explains
                        foreach (var exp in vm.Explains)
                        {
                            queryString = @"INSERT INTO [dbo].[t_learn_ensentexp]
                                       ([SentID]
                                       ,[ExpID]
                                       ,[LangKey]
                                       ,[ExpDetail])
                                 VALUES (@SentID
                                       ,@ExpID
                                       ,@LangKey
                                       ,@ExpDetail)";
                            cmd = new SqlCommand(queryString, conn, tran);

                            cmd.Parameters.AddWithValue("@SentID", vm.ID);
                            cmd.Parameters.AddWithValue("@ExpID", exp.ExpID);
                            cmd.Parameters.AddWithValue("@LangKey", exp.LanguageKey);
                            cmd.Parameters.AddWithValue("@ExpDetail", exp.Detail);

                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }

                        // Related words
                        foreach (var rwid in vm.RelatedWordIDs)
                        {
                            queryString = @"INSERT INTO [dbo].[t_learn_ensent_word] ([SentID],[WordID]) VALUES (@SentID, @WordID)";
                            cmd = new SqlCommand(queryString, conn, tran);

                            cmd.Parameters.AddWithValue("@SentID", vm.ID);
                            cmd.Parameters.AddWithValue("@WordID", rwid);

                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }

                        tran.Commit();
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif

                if (tran != null)
                {
                    tran.Rollback();
                }

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

            vm.ID = nNewID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(vm, setting);
        }

        // PUT: api/LearnEnSentence/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
            //UPDATE[dbo].[t_learn_ensent]
            //   SET[HID] = < HID, int,>
            //      ,[Sentence] = <Sentence, nvarchar(100),>
            //      ,[CREATEDBY] = <CREATEDBY, nvarchar(50),>
            //      ,[CREATEDAT] = <CREATEDAT, date,>
            //      ,[UPDATEDBY] = <UPDATEDBY, nvarchar(50),>
            //      ,[UPDATEDAT] = <UPDATEDAT, date,>
            // WHERE<Search Conditions,,>

            //UPDATE[dbo].[t_learn_ensentexp]
            //   SET[SentID] = < SentID, int,>
            //      ,[ExpID] = <ExpID, smallint,>
            //      ,[LangKey] = <LangKey, nvarchar(10),>
            //      ,[ExpDetail] = <ExpDetail, nvarchar(100),>
            // WHERE<Search Conditions,,>

            //UPDATE[dbo].[t_learn_ensent_word]
            //   SET[SentID] = < SentID, int,>
            //      ,[WordID] = <WordID, int,>
            // WHERE<Search Conditions,,>
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
            // DELETE FROM[dbo].[t_learn_ensent] WHERE<Search Conditions,,>
            // DELETE FROM [dbo].[t_learn_ensentexp] WHERE <Search Conditions,,>
            // DELETE FROM [dbo].[t_learn_ensent_word] WHERE <Search Conditions,,>
        }

        private string getQueryString(Boolean bListMode, Int32? nTop, Int32? nSkip, Int32? nSearchID, Int32 hid)
        {
            String strSQL = "";
            if (bListMode)
            {
                strSQL += @"SELECT count(*) FROM [dbo].[t_learn_ensent] WHERE [HID] = " + hid.ToString() + "; ";
            }

            strSQL += @"SELECT [ID]
                          ,[HID]
                          ,[Sentence]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_learn_ensent] ";
            if (bListMode && nTop.HasValue && nSkip.HasValue)
            {
                strSQL += @" WHERE [HID] = " + hid.ToString()
                       + @" ORDER BY (SELECT NULL) OFFSET " + nSkip.Value.ToString() + " ROWS FETCH NEXT " + nTop.Value.ToString() + " ROWS ONLY;";
            }
            else if (!bListMode && nSearchID.HasValue)
            {
                strSQL += " WHERE [ID] = " + nSearchID.Value.ToString();
                strSQL += @"; SELECT [SentID],[ExpID],[LangKey],[ExpDetail] FROM [dbo].[t_learn_ensentexp] WHERE [SentID] = " + nSearchID.Value.ToString();
                strSQL += @"; SELECT [SentID],[WordID] FROM [dbo].[t_learn_ensent_word] WHERE [SentID] = " + nSearchID.Value.ToString();
            }

            return strSQL;
        }

        private void OnSentenceHeader2VM(SqlDataReader reader, LearnEnSentenceViewModel vm)
        {
            var idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.Sentence = reader.GetString(idx++);

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

        private void OnSentenceExplain2VM(SqlDataReader reader, LearnEnSentenceExpViewModel vmExp)
        {
            var idx = 0;
            vmExp.ExpID = reader.GetInt32(idx++);
            vmExp.LanguageKey = reader.GetString(idx++);
            vmExp.Detail = reader.GetString(idx++);
        }
    }
}
