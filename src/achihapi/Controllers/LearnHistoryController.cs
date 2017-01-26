using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data.SqlClient;

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
            if (!reader.IsDBNull(idx))
                vm.CreatedBy = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.CreatedAt = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                vm.UpdatedBy = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.UpdatedAt = reader.GetDateTime(idx++);
        }

        // GET api/learnhistory/5
        [HttpGet("{id}")]
        public string Get(String strid)
        {
            return "value";
        }

        // POST api/learnhistory
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/learnhistory/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/learnhistory/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
