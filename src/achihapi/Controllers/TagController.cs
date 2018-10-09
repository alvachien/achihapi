using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using achihapi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class TagController : Controller
    {
        // GET: api/tag
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Boolean reqamt = true, Byte? tagtype = null, string tagterm = null)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            List<TagCountViewModel> listTermCounts = new List<TagCountViewModel>();
            List<TagViewModel> listTerms = new List<TagViewModel>();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";

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

                    //queryString = @"SELECT DISTINCT [Term] FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString();
                    if (reqamt)
                    {
                        queryString = @"SELECT [Term], COUNT(*) AS TERMCOUNT FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString();
                        if (tagtype.HasValue)
                        {
                            queryString += " AND [TagType] = " + tagtype.Value.ToString();
                        }
                        if (!String.IsNullOrEmpty(tagterm))
                        {
                            queryString += " AND [Term] LIKE '%" + tagterm + "%'";
                        }
                        queryString += @" GROUP BY [Term]";
                    }
                    else
                    {
                        queryString = @"SELECT [Term], [TagType], [TagID], [TagSubID] FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString();
                        if (tagtype.HasValue)
                        {
                            queryString += " AND [TagType] = " + tagtype.Value.ToString();
                        }
                        if (!String.IsNullOrEmpty(tagterm))
                        {
                            queryString += " AND [Term] LIKE '%" + tagterm + "%'";
                        }
                    }

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reqamt)
                            {
                                listTermCounts.Add(new TagCountViewModel()
                                {
                                    Term = reader.GetString(0),
                                    TermCount = reader.GetInt32(1)
                                });
                            }
                            else
                            {
                                listTerms.Add(new TagViewModel()
                                {
                                    Term = reader.GetString(0),
                                    TagType = reader.GetInt16(1),
                                    TagID = reader.GetInt32(2),
                                    TagSubID = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
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

            return reqamt ? new JsonResult(listTermCounts) : new JsonResult(listTerms);
        }

        // POST api/tag
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TagViewModel vm)
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

            // Check on term
            if (vm.Term != null)
                vm.Term = vm.Term.Trim();
            if (String.IsNullOrEmpty(vm.Term))
            {
                return BadRequest("Term is a must!");
            }
            if (vm.TagID <= 0)
            {
                return BadRequest("Tag ID is invalid");
            }
            if (vm.TagType == (Byte)HIHTagTypeEnum.FinanceDocumentItem
                || vm.TagType == (Byte)HIHTagTypeEnum.LearnQuestionBank)
            {
            }
            else
            {
                return BadRequest("Non supported type");
            }

            // Update the database
            SqlConnection conn = null;
            String queryString = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = "";
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;

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

                    queryString = @"SELECT [HID],[TagType],[TagID],[Term] FROM [dbo].[t_tag] WHERE [HID] = "
                        + vm.HID.ToString() + " AND [TagType] = " + vm.TagType.ToString()
                        + " AND [TagID] = " + vm.TagID.ToString() + " AND [Term] = N'" + vm.Term + "'";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        strErrMsg = "Tag existed already!";
                        throw new Exception(strErrMsg);
                    }
                    else
                    {
                        reader.Dispose();
                        reader = null;

                        cmd.Dispose();
                        cmd = null;

                        // Now go ahead for the creating
                        tran = conn.BeginTransaction();

                        queryString = @"INSERT INTO [dbo].[t_tag]
                                       ([HID]
                                       ,[TagType]
                                       ,[TagID]
                                       ,[Term])
                                 VALUES (@HID
                                       ,@TagType
                                       ,@TagID
                                       ,@Term)";

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        cmd.Parameters.AddWithValue("@HID", vm.HID);
                        cmd.Parameters.AddWithValue("@TagType", vm.TagType);
                        cmd.Parameters.AddWithValue("@TagID", vm.TagID);
                        cmd.Parameters.AddWithValue("@Term", vm.Term);

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();

                        tran.Commit();
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;

                if (tran != null)
                    tran.Rollback();
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

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            
            return new JsonResult(vm, setting);
        }

        // PUT api/tag/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }

        // DELETE api/tag/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
