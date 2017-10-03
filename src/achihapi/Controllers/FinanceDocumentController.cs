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
        public async Task<IActionResult> Get([FromQuery]Int32 hid, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            BaseListViewModel<FinanceDocumentUIViewModel> listVMs = new BaseListViewModel<FinanceDocumentUIViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            if (hid <= 0)
                return BadRequest("No Home Inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

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

                queryString = @"SELECT count(*) FROM[dbo].[t_fin_document] WHERE[HID] = @hid ";
                if (dtbgn.HasValue)
                    queryString += " AND [TRANDATE] >= @dtbgn ";
                if (dtend.HasValue)
                    queryString += " AND [TRANDATE] <= @dtend ";
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@hid", hid);
                if (dtbgn.HasValue)
                    cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                if (dtend.HasValue)
                    cmd.Parameters.AddWithValue("@dtend", dtend.Value);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    listVMs.TotalCount = reader.GetInt32(0);
                    break;
                }
                reader.Dispose();
                reader = null;
                cmd.Dispose();
                cmd = null;

                if (listVMs.TotalCount > 0)
                {
                    queryString = SqlUtility.getFinanceDocListQueryString();
                    queryString += @" WHERE [HID] = @hid ";
                    if (dtbgn.HasValue)
                        queryString += " AND [TRANDATE] >= @dtbgn ";
                    if (dtend.HasValue)
                        queryString += " AND [TRANDATE] <= @dtend ";
                    queryString += " ORDER BY [TRANDATE] DESC";
                    cmd = new SqlCommand(queryString, conn);
                    cmd.Parameters.AddWithValue("@hid", hid);
                    if (dtbgn.HasValue)
                        cmd.Parameters.AddWithValue("@dtbgn", dtbgn.Value);
                    if (dtend.HasValue)
                        cmd.Parameters.AddWithValue("@dtend", dtend.Value);
                    reader = await cmd.ExecuteReaderAsync();
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
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("No Home Inputted");
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

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
            if (vm.HID <= 0)
            {
                return BadRequest("No Home ID inputted");
            }

            // Do the basic check!
            // Header check!
            if (String.IsNullOrEmpty(vm.Desp))
            {
                return BadRequest("No Desp in the header");
            }
            if (vm.DocType == 0)
            {
                return BadRequest("Doc type is must!");
            }

            // Check the items
            if (vm.Items.Count <= 0)
            {
                return BadRequest("No item has been assigned yet");
            }
            foreach (var item in vm.Items)
            {
                if (item.AccountID == 0 || item.TranAmount == 0 || item.TranType == 0)
                {
                    return BadRequest("Item must have account or tran. amount or tran. type"); ;
                }

                if (item.ControlCenterID == 0 && item.OrderID == 0)
                {
                    return BadRequest("Must input control object");
                }
                if (item.ControlCenterID > 0 && item.OrderID > 0)
                {
                    return BadRequest("Either control center or order shall be inputted, not both");
                }
            }

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Int32 nNewDocID = -1;
            Boolean bError = false;
            String strErrMsg = "";
            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User cannot recognize");

            try
            {
                await conn.OpenAsync();

                // Do the validation
                try
                {
                    String strCheckString = @"SELECT TOP (1) [BASECURR] FROM [dbo].[t_homedef] WHERE [ID] = @hid;";
                    SqlCommand cmdCheck = new SqlCommand(strCheckString, conn);
                    cmdCheck.Parameters.AddWithValue("@hid", vm.HID);
                    SqlDataReader reader = await cmdCheck.ExecuteReaderAsync();
                    if (!reader.HasRows)
                        return BadRequest("No home found");

                    // Basic currency
                    string basecurr = String.Empty;
                    while (reader.Read())
                    {
                        basecurr = reader.GetString(0);
                        break;
                    }
                    if (String.IsNullOrEmpty(basecurr))
                        return BadRequest("No base currency defined!");
                    reader.Dispose();
                    reader = null;
                    cmdCheck.Dispose();
                    cmdCheck = null;

                    // Currency
                    strCheckString = @"SELECT TOP (1) CURR from t_fin_currency WHERE curr = @curr;";
                    cmdCheck = new SqlCommand(strCheckString, conn);
                    cmdCheck.Parameters.AddWithValue("@curr", vm.TranCurr);
                    reader = await cmdCheck.ExecuteReaderAsync();
                    if (!reader.HasRows)
                        return BadRequest("No currency found");
                    reader.Dispose();
                    reader = null;
                    cmdCheck.Dispose();
                    cmdCheck = null;

                    if (String.CompareOrdinal(vm.TranCurr, basecurr) != 0)
                    {
                        if (vm.ExgRate == 0)
                        {
                            return BadRequest("No exchange rate info provided!");
                        }
                    }

                    // Second currency
                    if (!String.IsNullOrEmpty(vm.TranCurr2))
                    {
                        strCheckString = @"SELECT TOP (1) CURR from t_fin_currency WHERE curr = @curr;";
                        cmdCheck = new SqlCommand(strCheckString, conn);
                        cmdCheck.Parameters.AddWithValue("@curr", vm.TranCurr2);
                        reader = await cmdCheck.ExecuteReaderAsync();
                        if (!reader.HasRows)
                            return BadRequest("No currency found");
                        reader.Dispose();
                        reader = null;
                        cmdCheck.Dispose();
                        cmdCheck = null;

                        if (String.CompareOrdinal(vm.TranCurr2, basecurr) != 0)
                        {
                            if (vm.ExgRate2 == 0)
                            {
                                return BadRequest("No exchange rate info provided!");
                            }
                        }
                    }

                    // Doc type
                    strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_doc_type] WHERE [ID] = @ID"; // @"SELECT TOP (1) [ID] FROM [t_fin_doc_type] WHERE [HID] = @HID AND [ID] = @ID";
                    cmdCheck = new SqlCommand(strCheckString, conn);
                    //cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
                    cmdCheck.Parameters.AddWithValue("@ID", vm.DocType);
                    reader = await cmdCheck.ExecuteReaderAsync();
                    if (!reader.HasRows)
                        return BadRequest("No currency found");
                    reader.Dispose();
                    reader = null;
                    cmdCheck.Dispose();
                    cmdCheck = null;

                    Decimal totalamount = 0;
                    foreach(var item in vm.Items)
                    {
                        // Account
                        strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_account] WHERE [HID] = @HID AND [ID] = @ID";
                        cmdCheck = new SqlCommand(strCheckString, conn);
                        cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
                        cmdCheck.Parameters.AddWithValue("@ID", item.AccountID);
                        reader = await cmdCheck.ExecuteReaderAsync();
                        if (!reader.HasRows)
                            return BadRequest("No account found");
                        reader.Dispose();
                        reader = null;
                        cmdCheck.Dispose();
                        cmdCheck = null;

                        // Transaction type
                        strCheckString = @"SELECT TOP (1) [ID], [EXPENSE] FROM [t_fin_tran_type] WHERE [ID] = @ID";//@"SELECT TOP (1) [ID], [EXPENSE] FROM [t_fin_tran_type] WHERE [HID] = @HID AND [ID] = @ID";
                        cmdCheck = new SqlCommand(strCheckString, conn);
                        //cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
                        cmdCheck.Parameters.AddWithValue("@ID", item.TranType);
                        reader = await cmdCheck.ExecuteReaderAsync();
                        if (!reader.HasRows)
                            return BadRequest("No tran. type found");

                        Boolean isexp = false;
                        while (reader.Read())
                        {
                            isexp = reader.GetBoolean(1);
                            break;
                        }
                        reader.Dispose();
                        reader = null;
                        cmdCheck.Dispose();
                        cmdCheck = null;

                        // Control center
                        if (item.ControlCenterID > 0)
                        {
                            strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_controlcenter] WHERE [HID] = @HID AND [ID] = @ID";
                            cmdCheck = new SqlCommand(strCheckString, conn);
                            cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
                            cmdCheck.Parameters.AddWithValue("@ID", item.ControlCenterID);
                            reader = await cmdCheck.ExecuteReaderAsync();
                            if (!reader.HasRows)
                                return BadRequest("No control center found");
                            reader.Dispose();
                            reader = null;
                            cmdCheck.Dispose();
                            cmdCheck = null;
                        }

                        // Order
                        if (item.OrderID > 0)
                        {
                            strCheckString = @"SELECT TOP (1) [ID] FROM [t_fin_order] WHERE [HID] = @HID AND [ID] = @ID";
                            cmdCheck = new SqlCommand(strCheckString, conn);
                            cmdCheck.Parameters.AddWithValue("@HID", vm.HID);
                            cmdCheck.Parameters.AddWithValue("@ID", item.OrderID);
                            reader = await cmdCheck.ExecuteReaderAsync();
                            if (!reader.HasRows)
                                return BadRequest("No order found");
                            reader.Dispose();
                            reader = null;
                            cmdCheck.Dispose();
                            cmdCheck = null;
                        }

                        // Item amount
                        Decimal itemAmt = 0;
                        if (item.UseCurr2)
                        {
                            if (isexp)
                            {
                                if (vm.ExgRate2 > 0)
                                {
                                    itemAmt = -1 * item.TranAmount / vm.ExgRate2;
                                }
                                else
                                {
                                    itemAmt = -1 * item.TranAmount;
                                }
                            }
                            else
                            {
                                if (vm.ExgRate2 > 0)
                                {
                                    itemAmt = item.TranAmount / vm.ExgRate2;
                                }
                                else
                                {
                                    itemAmt = item.TranAmount;
                                }
                            }
                        }
                        else
                        {
                            if (isexp)
                            {
                                if (vm.ExgRate > 0)
                                {
                                    itemAmt = -1 * item.TranAmount / vm.ExgRate;
                                }
                                else
                                {
                                    itemAmt = -1 * item.TranAmount;
                                }
                            }
                            else
                            {
                                if (vm.ExgRate > 0)
                                {
                                    itemAmt = item.TranAmount / vm.ExgRate;
                                }
                                else
                                {
                                    itemAmt = item.TranAmount;
                                }
                            }
                        }

                        if (itemAmt == 0)
                        {
                            return BadRequest("Amount is not correct");
                        }

                        totalamount += itemAmt;
                    }

                    if (vm.DocType == HIHAPIConstants.FinDocType_Transfer || vm.DocType == HIHAPIConstants.FinDocType_CurrExchange)
                    {
                        if (totalamount != 0)
                        {
                            return BadRequest("Amount must be zero");
                        }
                    }
                }
                catch (Exception exp)
                {
                    return BadRequest(exp.Message);
                }


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
