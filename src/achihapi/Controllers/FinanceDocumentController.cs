using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceDocumentController : Controller
    {
        // GET: api/financedocument
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery]Int32 hid, Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<FinanceDocumentUIViewModel> listVMs = new BaseListViewModel<FinanceDocumentUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                queryString = SqlUtility.getFinanceDocListQueryString(hid, top, skip);

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
                                FinanceDocumentUIViewModel avm = new FinanceDocumentUIViewModel();
                                SqlUtility.FinDocList_DB2VM(reader, avm);

                                listVMs.Add(avm);
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

        // GET api/financedocument/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            FinanceDocumentUIViewModel vm = new FinanceDocumentUIViewModel();

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotFound = false;

            try
            {
                queryString = SqlUtility.getFinanceDocQueryString(id);

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        // Header
                        while (reader.Read())
                        {
                            SqlUtility.FinDocHeader_DB2VM(reader, vm);
                        }
                    }
                    else if (nRstBatch == 1)
                    {
                        // Items
                        while (reader.Read())
                        {
                            FinanceDocumentItemUIViewModel itemvm = new FinanceDocumentItemUIViewModel();
                            SqlUtility.FinDocItem_DB2VM(reader, itemvm);

                            vm.Items.Add(itemvm);
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

            if (bNotFound)
            {
                return NotFound();
            }
            else if (bError)
            {
                return StatusCode(500, strErrMsg);
            }

            return new ObjectResult(vm);
        }

        // POST api/financedocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceDocumentUIViewModel vm)
        {
            if (vm == null || vm.DocType == FinanceDocTypeViewModel.DocType_AdvancePayment)
            {
                return BadRequest("No data is inputted");
            }

            // Todo: checks!
            // Header check!

            // Check the items
            if (vm.Items.Count <= 0)
            {
                return BadRequest("No item has been assigned yet");
            }

            // Item check!

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nNewDocID = -1;
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;

                await conn.OpenAsync();
                SqlTransaction tran = conn.BeginTransaction();

                SqlCommand cmd = null;

                // Now go ahead for the creating
                queryString = SqlUtility.getFinDocHeaderInsertString();

                try
                {
                    cmd = new SqlCommand(queryString, conn)
                    {
                        Transaction = tran
                    };

                    SqlUtility.bindFinDocHeaderParameter(cmd, vm, usrName);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewDocID = (Int32)idparam.Value;

                    // Then, creating the items
                    foreach (FinanceDocumentItemUIViewModel ivm in vm.Items)
                    {
                        queryString = SqlUtility.getFinDocItemInsertString();

                        SqlCommand cmd2 = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };
                        SqlUtility.bindFinDocItemParameter(cmd2, ivm, nNewDocID);

                        await cmd2.ExecuteNonQueryAsync();
                    }

                    tran.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                    bError = true;
                    strErrMsg = exp.Message;
                    tran.Rollback();
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

            vm.ID = nNewDocID;
            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            ;
            return new JsonResult(vm, setting);
        }

        // PUT api/financedocument/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/financedocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}
