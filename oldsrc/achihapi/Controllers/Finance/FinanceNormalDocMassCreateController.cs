using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data.SqlClient;
using System.Net;
using achihapi.Utilities;
using System.Data;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceNormalDocMassCreateController : ControllerBase
    {
        // GET: api/FinanceNormalDocMassCreate
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Forbid();
        }

        // GET: api/FinanceNormalDocMassCreate/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(int id)
        {
            return Forbid();
        }

        // POST: api/FinanceNormalDocMassCreate
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(int hid, [FromBody]List<FinanceNormalDocMassCreateViewModel> items)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (hid <= 0)
            {
                return BadRequest("HID is invalid");
            }
            if (items.Count <= 0)
            {
                return BadRequest("Items must input");
            }
            foreach (var mi in items)
            {
                if (!mi.IsValid())
                    return BadRequest(mi.LastError);
            }

            List<FinanceDocumentUIViewModel> listDocs = new List<FinanceDocumentUIViewModel>();
            foreach (var docheader in items.GroupBy(info => info.TranDate.Date)
                        .Select(group => new {
                            TranDate = group.Key,
                            Count = group.Count()
                        })
                        .OrderBy(x => x.TranDate))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("{0} {1}", docheader.TranDate, docheader.Count);
#endif

                var docvm = new FinanceDocumentUIViewModel();
                if(String.IsNullOrEmpty(docvm.Desp))
                    docvm.Desp = docheader.TranDate.Date.ToShortDateString();
                docvm.DocType = FinanceDocTypeViewModel.DocType_Normal;
                docvm.HID = hid;
                // docvm.TranCurr = item.
                docvm.TranDate = docheader.TranDate.Date;
                var docitems = (from item in items
                               where item.TranDate.Date == docheader.TranDate.Date
                               select item);
                var itemid = 1;
                foreach(var docitem in docitems)
                {
                    var di = new FinanceDocumentItemUIViewModel();
                    di.ItemID = itemid++;
                    di.AccountID = docitem.AccountID;
                    if (docitem.ControlCenterID.HasValue)
                        di.ControlCenterID = docitem.ControlCenterID.Value;
                    if (docitem.OrderID.HasValue)
                        di.OrderID = docitem.OrderID.Value;
                    if (String.IsNullOrEmpty(docvm.TranCurr))
                        docvm.TranCurr = docitem.TranCurrency;
                    di.TranAmount = docitem.TranAmount;
                    di.TranType = docitem.TranType;
                    di.Desp = docitem.Desp;
                    docvm.Items.Add(di);
                }
                if (!docvm.IsValid())
                {
                    return BadRequest("Document is invalid: " + docvm.LastError);
                }

                listDocs.Add(docvm);
            }

            // Do the posting
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            String queryString = "";
            Int32 nNewDocID = -1;
            String strErrMsg = "";
            HttpStatusCode errorCode = HttpStatusCode.OK;

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

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Do the validation
                    try
                    {
                        foreach(var fidoc in listDocs)
                            await FinanceDocumentController.FinanceDocumentBasicValidationAsync(fidoc, conn);
                    }
                    catch (Exception)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw;
                    }

                    tran = conn.BeginTransaction();

                    foreach(var fidoc in listDocs)
                    {
                        // Now go ahead for the creating
                        queryString = HIHDBUtility.GetFinDocHeaderInsertString();

                        cmd = new SqlCommand(queryString, conn)
                        {
                            Transaction = tran
                        };

                        HIHDBUtility.BindFinDocHeaderInsertParameter(cmd, fidoc, usrName);
                        SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                        idparam.Direction = ParameterDirection.Output;

                        Int32 nRst = await cmd.ExecuteNonQueryAsync();
                        nNewDocID = (Int32)idparam.Value;
                        fidoc.ID = nNewDocID;
                        cmd.Dispose();
                        cmd = null;

                        // Then, creating the items
                        foreach (FinanceDocumentItemUIViewModel ivm in fidoc.Items)
                        {
                            queryString = HIHDBUtility.GetFinDocItemInsertString();
                            cmd = new SqlCommand(queryString, conn)
                            {
                                Transaction = tran
                            };
                            HIHDBUtility.BindFinDocItemInsertParameter(cmd, ivm, nNewDocID);

                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;

                            // Tags
                            if (ivm.TagTerms.Count > 0)
                            {
                                // Create tags
                                foreach (var term in ivm.TagTerms)
                                {
                                    queryString = HIHDBUtility.GetTagInsertString();

                                    cmd = new SqlCommand(queryString, conn, tran);

                                    HIHDBUtility.BindTagInsertParameter(cmd, fidoc.HID, HIHTagTypeEnum.FinanceDocumentItem, nNewDocID, term, ivm.ItemID);

                                    await cmd.ExecuteNonQueryAsync();

                                    cmd.Dispose();
                                    cmd = null;
                                }
                            }
                        }
                    }

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif

                if (tran != null)
                    tran.Rollback();

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
                        return BadRequest(strErrMsg);
                    default:
                        return StatusCode(500, strErrMsg);
                }
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = HIHAPIConstants.DateFormatPattern,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            return new JsonResult(listDocs, setting);
        }

        // PUT: api/FinanceNormalDocMassCreate/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] string value)
        {
            return Forbid();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Forbid();
        }
    }
}
