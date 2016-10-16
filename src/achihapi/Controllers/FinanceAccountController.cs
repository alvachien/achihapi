using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceAccountController : Controller
    {
        [HttpGet]
        public IEnumerable<FinanceAccountViewModel> Get()
        {
            List<FinanceAccountViewModel> listVm = new List<FinanceAccountViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrObj = User.FindFirst(c => c.Type == "sub");

                queryString = @"SELECT TOP (100) [ID]
                      ,[CTGYID]
                      ,[NAME]
                      ,[COMMENT]
                      ,[OWNER]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [achihdb].[dbo].[t_fin_account]";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        FinanceAccountViewModel avm = new FinanceAccountViewModel();
                        avm.ID = reader.GetInt32(0);
                        avm.CtgyID = reader.GetInt32(1);
                        avm.Name = reader.GetName(2);
                        if (!reader.IsDBNull(3))
                            avm.Comment = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            avm.Owner = reader.GetString(4);
                        if (!reader.IsDBNull(5))
                            avm.CreatedBy = reader.GetString(5);
                        if (!reader.IsDBNull(6))
                            avm.CreatedAt = reader.GetDateTime(6);
                        if (!reader.IsDBNull(7))
                            avm.UpdatedBy = reader.GetString(7);
                        if (!reader.IsDBNull(8))
                            avm.UpdatedAt = reader.GetDateTime(8);

                        listVm.Add(avm);
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return listVm;
        }
    }
}
