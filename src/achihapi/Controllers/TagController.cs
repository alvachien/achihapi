using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using achihapi.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class TagController : Controller
    {
        // GET: api/tag
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Byte? tagtype = null, Int32? tagid = null)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            List<String> listTerms = new List<String>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
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

                queryString = @"SELECT DISTINCT [Term] FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString();
                if (tagtype.HasValue)
                {
                    queryString += " AND [TagType] = " + tagtype.Value.ToString();
                }
                if (tagid.HasValue)
                {
                    queryString += " AND [TagID] = " + tagid.Value.ToString();
                }

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        listTerms.Add(reader.GetString(0));
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
                    conn = null;
                }
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new JsonResult(listTerms);
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
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bDuplicatedEntry = false;
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

                queryString = @"SELECT [HID],[TagType],[TagID],[Term] FROM [dbo].[t_tag] WHERE [HID] = " + vm.HID.ToString() + " AND [TagType] = " + vm.TagType.ToString() 
                    + " AND [TagID] = " + vm.TagID.ToString() + " AND [Term] = N'" + vm.Term + "'";

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    bDuplicatedEntry = true;
                }
                else
                {
                    reader.Dispose();
                    reader = null;

                    cmd.Dispose();
                    cmd = null;

                    // Now go ahead for the creating
                    SqlTransaction tran = conn.BeginTransaction();

                    queryString = @"INSERT INTO [dbo].[t_tag]
                                       ([HID]
                                       ,[TagType]
                                       ,[TagID]
                                       ,[Term])
                                 VALUES (@HID
                                       ,@TagType
                                       ,@TagID
                                       ,@Term)";

                    try
                    {
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
                    catch (Exception exp)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                        bError = true;
                        strErrMsg = exp.Message;

                        if (tran != null)
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
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (bDuplicatedEntry)
            {
                return BadRequest("Tag already existed!");
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

        // PUT api/tag/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/tag/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
