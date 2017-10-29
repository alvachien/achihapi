using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    internal enum AccountLoadMode
    {
        All = 0,
        AccountPlusADP = 1,
        AccountPlusAsset = 2
    }

    internal class SqlUtility
    {
        #region Home define
        internal static string getHomeDefQueryString(String strUserID = null)
        {
            String strSQL = @"SELECT [ID]
                          ,[NAME]
                          ,[HOST]
                          ,[BASECURR]
                          ,[DETAILS]
                          ,[USER]
                          ,[DISPLAYAS]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[v_homemember] ";
            if (!String.IsNullOrEmpty(strUserID))
            {
                strSQL += " WHERE [v_homemember].[USER] = N'" + strUserID + "'";
            }

            return strSQL;
        }

        internal static void HomeDef_DB2VM(SqlDataReader reader, HomeDefViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.Name = reader.GetString(idx++);
                vm.Host = reader.GetString(idx++);
                vm.BaseCurrency = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Details = reader.GetString(idx++);
                else
                    ++idx;
                // User - skipping
                idx++;
                // Display as
                if (!reader.IsDBNull(idx))
                    vm.CreatorDisplayAs = reader.GetString(idx++);
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
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: ID {0}, index {1}, {2}", vm.ID, idx, exp.Message));
                throw exp;
            }
        }
        
        internal static String getHomeDefInsertString()
        {
            return @"INSERT INTO [dbo].[t_homedef]
                   ([NAME]
                   ,[DETAILS]
                   ,[HOST]
                   ,[BASECURR]
                   ,[CREATEDBY]
                   ,[CREATEDAT])
                   VALUES
                   (@NAME
                   ,@DETAILS
                   ,@HOST
                   ,@BASECURR
                   ,@CREATEDBY
                   ,@CREATEDAT
                    ); SELECT @Identity = SCOPE_IDENTITY();";
        }

        internal static String getHomeDefUpdateString()
        {
            return @"UPDATE [dbo].[t_homedef]
                   SET [NAME] = @NAME
                      ,[DETAILS] = @DETAILS
                      ,[HOST] = @HOST
                      ,[BASECURR] = @BASECURR
                      ,[UPDATEDBY] = @UPDATEDBY
                      ,[UPDATEDAT] = @UPDATEDAT
                 WHERE [ID] = @ID";
        }

        internal static String getHomeDefDeleteString()
        {
            return @"DELETE FROM [dbo].[t_homedef]
                    WHERE [ID] = @ID";
        }

        internal static void bindHomeDefInsertParameter(SqlCommand cmd, HomeDefViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            if (String.IsNullOrEmpty(vm.Details))
                cmd.Parameters.AddWithValue("@DETAILS", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@DETAILS", vm.Details);
            cmd.Parameters.AddWithValue("@HOST", vm.Host);
            cmd.Parameters.AddWithValue("@BASECURR", vm.BaseCurrency);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
        }

        internal static void bindHomeDefUpdateParameter(SqlCommand cmd, HomeDefViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            if (String.IsNullOrEmpty(vm.Details))
                cmd.Parameters.AddWithValue("@DETAILS", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@DETAILS", vm.Details);
            cmd.Parameters.AddWithValue("@HOST", vm.Host);
            cmd.Parameters.AddWithValue("@BASECURR", vm.BaseCurrency);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
        }
        #endregion

        #region Home member
        internal static string getHomeMemQueryString(Int32? hid)
        {
            return @"SELECT [HID]
                      ,[USER]
                      ,[DISPLAYAS]
                      ,[RELT]
                      ,[CREATEDBY]
                      ,[CREATEDAT]
                      ,[UPDATEDBY]
                      ,[UPDATEDAT]
                  FROM [dbo].[t_homemem] " + (hid.HasValue? " WHERE [HID] = " + hid.Value.ToString() : "");
        }

        internal static String getHomeMemInsertString()
        {
            return @"INSERT INTO [dbo].[t_homemem]
                       ([HID]
                       ,[USER]
                       ,[DISPLAYAS]
                       ,[RELT]
                       ,[CREATEDBY]
                       ,[CREATEDAT])
                 VALUES
                       (@HID
                       ,@USER
                       ,@DISPLAYAS
                       ,@RELT
                       ,@CREATEDBY
                       ,@CREATEDAT)";
        }

        internal static void bindHomeMemInsertParameter(SqlCommand cmd, HomeMemViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HomeID);
            cmd.Parameters.AddWithValue("@USER", vm.User);
            cmd.Parameters.AddWithValue("@DISPLAYAS", vm.DisplayAs);
            cmd.Parameters.AddWithValue("@RELT", vm.Relation);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
            //cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
            //cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
        }

        internal static void HomeMem_DB2VM(SqlDataReader reader, HomeMemViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.HomeID = reader.GetInt32(idx++);
                vm.User = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.DisplayAs = reader.GetString(idx++);
                else
                    ++idx;
                vm.Relation = reader.GetInt16(idx++);
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
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: HID {0}, index {1}, {2}", vm.HomeID, idx, exp.Message));
                throw exp;
            }
        }

        internal static String getHomeMemUpdateString()
        {
            return @"UPDATE [dbo].[t_homemem]
                       SET [HID] = @HID
                          ,[USER] = @USER
                          ,[DISPLAYAS] = @DISPLAYAS
                          ,[RELT] = @RELT
                          ,[CREATEDBY] = @CREATEDBY
                          ,[CREATEDAT] = @CREATEDAT
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [HID] = @HID
                       AND [USER] = @USER";
        }

        internal static String getHomeMemDeleteString()
        {
            return @"DELETE FROM [dbo].[t_homemem]
                WHERE [HID] = @HID AND [USER] = @USER";
        }
        #endregion

        #region Home message
        internal static string getHomeMsgQueryString(String urName, Int32 hid)
        {
            return @"SELECT [ID]
                      ,[HID]
                      ,[USERTO]
                      ,[SENDDATE]
                      ,[USERFROM]
                      ,[TITLE]
                      ,[CONTENT]
                      ,[READFLAG]
                  FROM [dbo].[t_homemsg] WHERE [HID] = " + hid.ToString()  + " AND [USERTO] = N'" + urName + "'";
        }

        internal static void HomeMsg_DB2VM(SqlDataReader reader, HomeMsgViewModel vm)
        {
            Int32 idx = 0;
            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.HID = reader.GetInt32(idx++);
                vm.UserTo = reader.GetString(idx++);
                vm.SendDate = reader.GetDateTime(idx++);
                vm.UserFrom = reader.GetString(idx++);
                vm.Title = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Content = reader.GetString(idx++);
                else
                    ++idx;
                vm.ReadFlag = reader.GetBoolean(idx);
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: HID {0}, index {1}, {2}", vm.HID, idx, exp.Message));
                throw exp;
            }
        }
        #endregion

        #region Learn Category
        internal static string getLearnCategoryQueryString()
        {
            string strSQL = @"SELECT [ID]
                              ,[HID]
                              ,[PARID]
                              ,[NAME]
                              ,[COMMENT]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_learn_ctgy]";
            return strSQL;
        }

        internal static string getLearnCategoryInsertString()
        {
            string strSQL = @"INSERT INTO [dbo].[t_learn_ctgy]
                               ([HID]
                               ,[PARID]
                               ,[NAME]
                               ,[COMMENT]
                               ,[CREATEDBY]
                               ,[CREATEDAT])
                         VALUES (@HID
                               ,@PARID
                               ,@NAME
                               ,@COMMENT
                               ,@CREATEDBY
                               ,@CREATEDAT);
                        SELECT @Identity = SCOPE_IDENTITY();";
            return strSQL;
        }

        internal static void LearnCategory_DB2VM(SqlDataReader reader, LearnCategoryViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.ID = reader.GetInt32(idx++);
                if (!reader.IsDBNull(idx))
                    vm.HID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ParID = reader.GetInt32(idx++);
                else
                    ++idx;
                vm.Name = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Comment = reader.GetString(idx++);
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
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: ID {0}, index {1}, {2}", vm.ID, idx, exp.Message));
                throw exp;
            }
        }

        internal static void bindLearnCategoryInsertParameter(SqlCommand cmd, LearnCategoryViewModel vm, String usrName)
        {
            if (vm.HID != null)
                cmd.Parameters.AddWithValue("@HID", vm.HID.Value);
            else
                cmd.Parameters.AddWithValue("@HID", DBNull.Value);

            if (vm.ParID != null)
                cmd.Parameters.AddWithValue("@PARID", vm.ParID.Value);
            else
                cmd.Parameters.AddWithValue("@PARID", DBNull.Value);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            if (String.IsNullOrEmpty(vm.Comment))
                cmd.Parameters.AddWithValue("@COMMENT", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@COMMENT", vm.Comment);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
        }
        #endregion

        #region Learn History
        internal static string getLearnHistoryQueryString(Int32? hid, String strUser = "")
        {
            String strSQL = @"SELECT [t_learn_hist].[HID]
                      ,[t_learn_hist].[USERID]
                      ,[t_homemem].[DISPLAYAS] as [USERDISPLAYAS]
                      ,[t_learn_hist].[OBJECTID]
                      ,[t_learn_obj].[NAME] as [OBJECTNAME]
                      ,[t_learn_hist].[LEARNDATE]
                      ,[t_learn_hist].[COMMENT]
                      ,[t_learn_hist].[CREATEDBY]
                      ,[t_learn_hist].[CREATEDAT]
                      ,[t_learn_hist].[UPDATEDBY]
                      ,[t_learn_hist].[UPDATEDAT] 
                        FROM [dbo].[t_learn_hist]
                            INNER JOIN [dbo].[t_homemem] ON [t_learn_hist].[HID] = [t_homemem].[HID]
                                                        AND [t_learn_hist].[USERID] = [t_homemem].[USER]
                            INNER JOIN [dbo].[t_learn_obj] ON [t_learn_hist].[OBJECTID] = [t_learn_obj].[ID] ";
            if (hid.HasValue)
            {
                strSQL += " WHERE [t_learn_hist].[HID] = " + hid.Value.ToString() + " ";
                if (!String.IsNullOrEmpty(strUser))
                {
                    strSQL += " AND [t_learn_hist].[USERID] = N'" + strUser + "'";
                }
            } 
            else
            {
                if (!String.IsNullOrEmpty(strUser))
                {
                    strSQL += " WHERE [t_learn_hist].[USERID] = N'" + strUser + "'";
                }
            }

            return strSQL;
        }
        #endregion

        #region Finance Account
        internal static string getFinanceAccountQueryString(Int32? hid, Byte? status, String strOwner = "")
        {
            String strSQL = @"SELECT [t_fin_account].[ID]
                      ,[t_fin_account].[HID]  
                      ,[t_fin_account].[CTGYID]
                      ,[t_fin_account].[NAME]
                      ,[t_fin_account].[COMMENT]
                      ,[t_fin_account].[OWNER]
                      ,[t_fin_account].[STATUS]
                      ,[t_fin_account].[CREATEDBY]
                      ,[t_fin_account].[CREATEDAT]
                      ,[t_fin_account].[UPDATEDBY]
                      ,[t_fin_account].[UPDATEDAT]
                      ,[t_fin_account_ext_dp].[DIRECT]
                      ,[t_fin_account_ext_dp].[STARTDATE]
                      ,[t_fin_account_ext_dp].[ENDDATE]
                      ,[t_fin_account_ext_dp].[RPTTYPE]
                      ,[t_fin_account_ext_dp].[REFDOCID]
                      ,[t_fin_account_ext_dp].[DEFRRDAYS]
                      ,[t_fin_account_ext_dp].[COMMENT]
                      ,[t_fin_account_ext_as].[CTGYID] AS [ASCTGYID]
                      ,[t_fin_account_ext_as].[NAME] AS [ASNAME]
                      ,[t_fin_account_ext_as].[REFDOC_BUY] AS [ASREFDOC_BUY]
                      ,[t_fin_account_ext_as].[REFDOC_SOLD] AS [ASREFDOC_SOLD]
                      ,[t_fin_account_ext_as].[COMMENT] AS [ASCOMMENT]
                  FROM [dbo].[t_fin_account]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_dp]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_dp].[ACCOUNTID]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_as]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_as].[ACCOUNTID]";

            Boolean bwhere = false;
            if (hid.HasValue)
            {
                bwhere = true;
                strSQL += " WHERE [t_fin_account].[HID] = " + hid.Value.ToString();
                if (!String.IsNullOrEmpty(strOwner))
                {
                    strSQL += " AND [t_fin_account].[OWNER] = N'" + strOwner + "'";
                }
            }

            if (status.HasValue)
            {
                if (!bwhere)
                {
                    bwhere = true;
                    strSQL += " WHERE ";
                }
                else
                    strSQL += " AND ";
                
                if (status.Value == 0)
                    strSQL += " ( [t_fin_account].[STATUS] = 0 OR [t_fin_account].[STATUS] IS NULL ) ";
                else
                    strSQL += " [t_fin_account].[STATUS] = " + status.Value.ToString();
            }

            if (!String.IsNullOrEmpty(strOwner))
            {
                if (bwhere)
                    strSQL += " WHERE ";
                else
                    strSQL += " AND ";

                strSQL += " [t_fin_account].[OWNER] = N'" + strOwner + "'";
            }

            return strSQL;
        }

        internal static string GetFinanceAccountHeaderInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_account]
                                ([HID]    
                                ,[CTGYID]
                                ,[NAME]
                                ,[COMMENT]
                                ,[OWNER]
                                ,[CREATEDBY]
                                ,[CREATEDAT])
                            VALUES (@HID
                                ,@CTGYID
                                ,@NAME
                                ,@COMMENT
                                ,@OWNER
                                ,@CREATEDBY
                                ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }

        internal static string GetFinanceAccountHeaderUpdateString()
        {
            return @"UPDATE [dbo].[t_fin_account]
                           SET [CTGYID] = @CTGYID
                              ,[NAME] = @NAME
                              ,[COMMENT] = @COMMENT
                              ,[OWNER] = @OWNER
                              ,[STATUS] = @STATUS
                              ,[UPDATEDBY] = @UPDATEDBY
                              ,[UPDATEDAT] = @UPDATEDAT
                         WHERE [ID] = @ID AND [HID] = @HID";
        }

        internal static void BindFinAccountInsertParameter(SqlCommand cmd, FinanceAccountViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
            if (String.IsNullOrEmpty(vm.Owner))
                cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
            cmd.Parameters.AddWithValue("@STATUS", vm.Status);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
        }

        internal static void BindFinAccountUpdateParameter(SqlCommand cmd, FinanceAccountViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
            if (String.IsNullOrEmpty(vm.Owner))
                cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
            cmd.Parameters.AddWithValue("@STATUS", vm.Status);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@ID", vm.ID);
            cmd.Parameters.AddWithValue("@HID", vm.HID);
        }

        internal static Int32 FinAccountHeader_DB2VM(SqlDataReader reader, FinanceAccountUIViewModel vm, Int32 idx)
        {
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.CtgyID = reader.GetInt32(idx++);
            //if (!reader.IsDBNull(idx))
            //    vm.CtgyName = reader.GetString(idx++);
            //else
            //    ++idx;
            vm.Name = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.Comment = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Owner = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Status = reader.GetByte(idx++);
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

            return idx;
        }

        internal static void FinAccountADP_DB2VM(SqlDataReader reader, FinanceAccountExtDPViewModel vmdp, Int32 idx)
        {
            // Advance payment
            if (!reader.IsDBNull(idx))
                vmdp.Direct = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.StartDate = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.EndDate = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.RptType = reader.GetByte(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.RefDocID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.DefrrDays = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.Comment = reader.GetString(idx++);
            else
                ++idx;
        }

        internal static void FinAccountAsset_DB2VM(SqlDataReader reader, FinanceAccountExtASViewModel vmas, Int32 idx)
        {
            if (!reader.IsDBNull(idx))
                vmas.CategoryID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmas.Name = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmas.RefDocForBuy = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmas.RefDocForSold = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmas.Comment = reader.GetString(idx++);
            else
                ++idx;
        }

        internal static void FinAccount_DB2VM(SqlDataReader reader, FinanceAccountUIViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                idx = FinAccountHeader_DB2VM(reader, vm, idx);

                if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment)
                {
                    vm.ExtraInfo_ADP = new FinanceAccountExtDPViewModel();

                    FinAccountADP_DB2VM(reader, vm.ExtraInfo_ADP, idx);
                }
                else
                {
                    idx += 7;
                }

                if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_Asset)
                {
                    vm.ExtraInfo_AS = new FinanceAccountExtASViewModel();
                    FinAccountAsset_DB2VM(reader, vm.ExtraInfo_AS, idx);
                }
                else
                {
                    idx += 4; // ?!
                }
            }
            catch(Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: ID {0}, index {1}, {2}", vm.ID, idx, exp.Message));
                throw exp;
            }
        }
        #endregion

        #region Finance Account Extra: ADP
        internal static string GetFinanceAccountADPInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_account_ext_dp]
                                ([ACCOUNTID]
                                ,[DIRECT]
                                ,[STARTDATE]
                                ,[ENDDATE]
                                ,[RPTTYPE]
                                ,[REFDOCID]
                                ,[COMMENT])
                            VALUES
                                (@ACCOUNTID
                                ,@DIRECT
                                ,@STARTDATE
                                ,@ENDDATE
                                ,@RPTTYPE
                                ,@REFDOCID
                                ,@COMMENT )";
        }

        internal static void BindFinAccountADPInsertParameter(SqlCommand cmd, FinanceAccountExtDPViewModel vm, Int32 nNewDocID, Int32 nNewAccountID, String usrName)
        {
            cmd.Parameters.AddWithValue("@ACCOUNTID", nNewAccountID);
            cmd.Parameters.AddWithValue("@DIRECT", vm.Direct);
            cmd.Parameters.AddWithValue("@STARTDATE", vm.StartDate);
            cmd.Parameters.AddWithValue("@ENDDATE", vm.EndDate);
            cmd.Parameters.AddWithValue("@RPTTYPE", vm.RptType);
            cmd.Parameters.AddWithValue("@REFDOCID", nNewDocID);
            //cmd.Parameters.AddWithValue("@DEFRRDAYS", vm.AccountVM.AdvancePaymentInfo.DefrrDays);
            cmd.Parameters.AddWithValue("@COMMENT",
                String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
        }
        #endregion

        #region Finance Account Extra: Asset
        internal static string GetFinanceAccountAssetInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_account_ext_as]
                   ([ACCOUNTID]
                   ,[CTGYID]
                   ,[NAME]
                   ,[REFDOC_BUY]
                   ,[COMMENT])
             VALUES( @ACCOUNTID
                   ,@CTGYID
                   ,@NAME
                   ,@REFDOC_BUY
                   ,@COMMENT)";
        }

        internal static string GetFinanceAccountAssetUpdateString()
        {
            return @"UPDATE [dbo].[t_fin_account_ext_as]
                       SET [CTGYID] = @CTGYID
                          ,[NAME] = @NAME
                          ,[REFDOC_BUY] = @REFDOC_BUY
                          ,[COMMENT] = @COMMENT
                          ,[REFDOC_SOLD] = @REFDOC_SOLD
                     WHERE [ACCOUNTID] = @ACCOUNTID";
        }

        internal static void BindFinAccountAssetInsertParameter(SqlCommand cmd, FinanceAccountExtASViewModel vm)
        {
            cmd.Parameters.AddWithValue("@ACCOUNTID", vm.AccountID);
            cmd.Parameters.AddWithValue("@CTGYID", vm.CategoryID);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@REFDOC_BUY", vm.RefDocForBuy);
            cmd.Parameters.AddWithValue("@COMMENT",
                String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
        }

        internal static void BindFinAccountAssetUpdateParameter(SqlCommand cmd, FinanceAccountExtASViewModel vm)
        {
            cmd.Parameters.AddWithValue("@CTGYID", vm.CategoryID);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@REFDOC_BUY", vm.RefDocForBuy);
            cmd.Parameters.AddWithValue("@COMMENT",
                String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
            if (vm.RefDocForSold.HasValue)
                cmd.Parameters.AddWithValue("@REFDOC_SOLD", vm.RefDocForSold.Value.ToString());
            else
                cmd.Parameters.AddWithValue("@REFDOC_SOLD", DBNull.Value);
            cmd.Parameters.AddWithValue("@ACCOUNTID", vm.AccountID);
        }
        #endregion

        #region Finance document List
        internal static string getFinanceDocListQueryString()
        {
            return @" SELECT [ID]
                                ,[HID]
                                ,[DOCTYPE]
	                            ,[DOCTYPENAME]
                                ,[TRANDATE]
                                ,[TRANCURR]
                                ,[DESP]
                                ,[EXGRATE]
                                ,[EXGRATE_PLAN]
                                ,[TRANCURR2]
                                ,[EXGRATE2]
                                ,[EXGRATE_PLAN2]
                                ,[CREATEDBY]
                                ,[CREATEDAT]
                                ,[UPDATEDBY]
                                ,[UPDATEDAT]
                                ,[TRANAMOUNT]
                            FROM [dbo].[v_fin_document]";                            
        }

        internal static void FinDocList_DB2VM(SqlDataReader reader, FinanceDocumentUIViewModel vm)
        {
            Int32 idx = 0;
            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.HID = reader.GetInt32(idx++);
                vm.DocType = reader.GetInt16(idx++);
                vm.DocTypeName = reader.GetString(idx++);
                vm.TranDate = reader.GetDateTime(idx++);
                vm.TranCurr = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Desp = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate = reader.GetDecimal(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan = reader.GetBoolean(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.TranCurr2 = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate2 = reader.GetDecimal(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan2 = reader.GetBoolean(idx++);
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
                    vm.TranAmount = reader.GetDecimal(idx++);
                else
                    ++idx;
            }
            catch(Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: ID {0}, index {1}, {2}", vm.ID, idx, exp.Message));
                throw exp;
            }
        }
        #endregion

        #region Finance document READ
        internal static string getFinanceDocQueryString(Int32 nid)
        {
            String strSQL = @"SELECT [ID]
                          ,[HID]
                          ,[DOCTYPE]
                          ,[TRANDATE]
                          ,[TRANCURR]
                          ,[DESP]
                          ,[EXGRATE]
                          ,[EXGRATE_PLAN]
                          ,[TRANCURR2]
                          ,[EXGRATE2]
                          ,[EXGRATE_PLAN2]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [t_fin_document] WHERE [ID] = " + nid.ToString() + @"; 
                      SELECT [DOCID]
                            ,[ITEMID]
                            ,[ACCOUNTID]
                            ,[ACCOUNTNAME]
                            ,[TRANTYPE]
                            ,[TRANTYPENAME]
                            ,[USECURR2]
                            ,[TRANAMOUNT_ORG] AS [TRANAMOUNT]
                            ,[CONTROLCENTERID]
                            ,[CONTROLCENTERNAME]
                            ,[ORDERID]
                            ,[ORDERNAME]
                            ,[DESP]
                        FROM [v_fin_document_item1] WHERE [DOCID] = " + nid.ToString();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        internal static string getFinanceDocADPListQueryString()
        {
            return @"SELECT [t_fin_tmpdoc_dp].[DOCID]
                          ,[t_fin_tmpdoc_dp].[HID]
                          ,[t_fin_tmpdoc_dp].[REFDOCID]
                          ,[t_fin_tmpdoc_dp].[ACCOUNTID]
                          ,[t_fin_tmpdoc_dp].[TRANDATE]
                          ,[t_fin_tmpdoc_dp].[TRANTYPE]
                          ,[t_fin_tmpdoc_dp].[TRANAMOUNT]
                          ,[t_fin_tmpdoc_dp].[CONTROLCENTERID]
                          ,[t_fin_tmpdoc_dp].[ORDERID]
                          ,[t_fin_tmpdoc_dp].[DESP]
                          ,[t_fin_tmpdoc_dp].[CREATEDBY]
                          ,[t_fin_tmpdoc_dp].[CREATEDAT]
                          ,[t_fin_tmpdoc_dp].[UPDATEDBY]
                          ,[t_fin_tmpdoc_dp].[UPDATEDAT]
                        FROM [dbo].[t_fin_tmpdoc_dp] ";
        }

        internal static string getFinanceDocADPQueryString(Int32 nid)
        {
            String strSQL = @"SELECT [ID]
                          ,[HID]
                          ,[DOCTYPE]
                          ,[TRANDATE]
                          ,[TRANCURR]
                          ,[DESP]
                          ,[EXGRATE]
                          ,[EXGRATE_PLAN]
                          ,[TRANCURR2]
                          ,[EXGRATE2]
                          ,[EXGRATE_PLAN2]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [t_fin_document] WHERE [DOCTYPE] = " + FinanceDocTypeViewModel.DocType_AdvancePayment.ToString() + " AND [ID] = " + nid.ToString() + @"; 
                      SELECT [DOCID]
                            ,[ITEMID]
                            ,[ACCOUNTID]
                            ,[ACCOUNTNAME]
                            ,[TRANTYPE]
                            ,[TRANTYPENAME]
                            ,[USECURR2]
                            ,[TRANAMOUNT_ORG] AS [TRANAMOUNT]
                            ,[CONTROLCENTERID]
                            ,[CONTROLCENTERNAME]
                            ,[ORDERID]
                            ,[ORDERNAME]
                            ,[DESP]
                        FROM [v_fin_document_item1] WHERE [DOCID] = " + nid.ToString() + @"; 
                      SELECT [t_fin_account].[ID]
                            ,[t_fin_account].[HID]
                            ,[t_fin_account].[CTGYID]
                            ,[t_fin_account].[NAME]
                            ,[t_fin_account].[COMMENT]
                            ,[t_fin_account].[OWNER]
                            ,[t_fin_account].[STATUS]
                            ,[t_fin_account].[CREATEDBY]
                            ,[t_fin_account].[CREATEDAT]
                            ,[t_fin_account].[UPDATEDBY]
                            ,[t_fin_account].[UPDATEDAT]
                            ,[t_fin_account_ext_dp].[DIRECT]
                            ,[t_fin_account_ext_dp].[STARTDATE]
                            ,[t_fin_account_ext_dp].[ENDDATE]
                            ,[t_fin_account_ext_dp].[RPTTYPE]
                            ,[t_fin_account_ext_dp].[REFDOCID]
                            ,[t_fin_account_ext_dp].[DEFRRDAYS]
                            ,[t_fin_account_ext_dp].[COMMENT]
                        FROM [dbo].[t_fin_account]
                        LEFT OUTER JOIN [dbo].[t_fin_account_ext_dp]
                            ON [t_fin_account].[ID] = [t_fin_account_ext_dp].[ACCOUNTID]
                        WHERE [t_fin_account].[CTGYID] = "
                        + FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment.ToString()
                        + " AND [t_fin_account_ext_dp].[REFDOCID] = " + nid.ToString() + @"; 
                      SELECT [dbo].[t_fin_tmpdoc_dp].[DOCID]
                          ,[dbo].[t_fin_tmpdoc_dp].[HID]
                          ,[dbo].[t_fin_tmpdoc_dp].[REFDOCID]
                          ,[dbo].[t_fin_tmpdoc_dp].[ACCOUNTID]
                          ,[dbo].[t_fin_tmpdoc_dp].[TRANDATE]
                          ,[dbo].[t_fin_tmpdoc_dp].[TRANTYPE]
                          ,[dbo].[t_fin_tmpdoc_dp].[TRANAMOUNT]
                          ,[dbo].[t_fin_tmpdoc_dp].[CONTROLCENTERID]
                          ,[dbo].[t_fin_tmpdoc_dp].[ORDERID]
                          ,[dbo].[t_fin_tmpdoc_dp].[DESP]
                          ,[dbo].[t_fin_tmpdoc_dp].[CREATEDBY]
                          ,[dbo].[t_fin_tmpdoc_dp].[CREATEDAT]
                          ,[dbo].[t_fin_tmpdoc_dp].[UPDATEDBY]
                          ,[dbo].[t_fin_tmpdoc_dp].[UPDATEDAT]
                      FROM [dbo].[t_fin_tmpdoc_dp]
	                    INNER JOIN [dbo].[t_fin_account_ext_dp]
	                    ON [dbo].[t_fin_tmpdoc_dp].[ACCOUNTID] = [dbo].[t_fin_account_ext_dp].[ACCOUNTID]
	                    AND [dbo].[t_fin_account_ext_dp].[REFDOCID] = " + nid.ToString() + ";";

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }
        internal static string getFinanceDocAssetQueryString(Int32 nid, Boolean isBuyIn)
        {
            String strSQL = @"SELECT [ID]
                          ,[HID]
                          ,[DOCTYPE]
                          ,[TRANDATE]
                          ,[TRANCURR]
                          ,[DESP]
                          ,[EXGRATE]
                          ,[EXGRATE_PLAN]
                          ,[TRANCURR2]
                          ,[EXGRATE2]
                          ,[EXGRATE_PLAN2]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [t_fin_document] WHERE [DOCTYPE] = " 
                        + (isBuyIn? FinanceDocTypeViewModel.DocType_AssetBuyIn.ToString() : FinanceDocTypeViewModel.DocType_AssetSoldOut.ToString()) 
                        + " AND [ID] = " + nid.ToString() + @"; 
                      SELECT [DOCID]
                            ,[ITEMID]
                            ,[ACCOUNTID]
                            ,[ACCOUNTNAME]
                            ,[TRANTYPE]
                            ,[TRANTYPENAME]
                            ,[USECURR2]
                            ,[TRANAMOUNT_ORG] AS [TRANAMOUNT]
                            ,[CONTROLCENTERID]
                            ,[CONTROLCENTERNAME]
                            ,[ORDERID]
                            ,[ORDERNAME]
                            ,[DESP]
                        FROM [v_fin_document_item1] WHERE [DOCID] = " + nid.ToString() + @"; 
                      SELECT [t_fin_account].[ID]
                            ,[t_fin_account].[HID]
                            ,[t_fin_account].[CTGYID]
                            ,[t_fin_account].[NAME]
                            ,[t_fin_account].[COMMENT]
                            ,[t_fin_account].[OWNER]
                            ,[t_fin_account].[STATUS]
                            ,[t_fin_account].[CREATEDBY]
                            ,[t_fin_account].[CREATEDAT]
                            ,[t_fin_account].[UPDATEDBY]
                            ,[t_fin_account].[UPDATEDAT]
                            ,[t_fin_account_ext_as].[CTGYID] AS [ASCTGYID]
                            ,[t_fin_account_ext_as].[NAME] AS [ASNAME]
                            ,[t_fin_account_ext_as].[REFDOC_BUY] AS [ASREFDOC_BUY]
                            ,[t_fin_account_ext_as].[REFDOC_SOLD] AS [ASREFDOC_SOLD]
                            ,[t_fin_account_ext_as].[COMMENT] AS [ASCOMMENT]
                        FROM [dbo].[t_fin_account]
                        LEFT OUTER JOIN [dbo].[t_fin_account_ext_as]
                            ON [t_fin_account].[ID] = [t_fin_account_ext_as].[ACCOUNTID]
                        WHERE [t_fin_account].[CTGYID] = "
                        + FinanceAccountCtgyViewModel.AccountCategory_Asset.ToString()
                        + ( isBuyIn? " AND [t_fin_account_ext_as].[REFDOC_BUY] = " : " AND [t_fin_account_ext_as].[REFDOC_SOLD] = ") + nid.ToString();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }
        #endregion

        #region Finance document Header
        internal static string getFinDocHeaderInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_document]
                                           ([HID]
                                           ,[DOCTYPE]
                                           ,[TRANDATE]
                                           ,[TRANCURR]
                                           ,[DESP]
                                           ,[EXGRATE]
                                           ,[EXGRATE_PLAN]
                                           ,[EXGRATE_PLAN2]
                                           ,[TRANCURR2]
                                           ,[EXGRATE2]
                                           ,[CREATEDBY]
                                           ,[CREATEDAT]
                                           ,[UPDATEDBY]
                                           ,[UPDATEDAT])
                                     VALUES
                                           (@HID
                                           ,@DOCTYPE
                                           ,@TRANDATE
                                           ,@TRANCURR
                                           ,@DESP
                                           ,@EXGRATE
                                           ,@EXGRATE_PLAN
                                           ,@EXGRATE_PLAN2
                                           ,@TRANCURR2
                                           ,@EXGRATE2
                                           ,@CREATEDBY
                                           ,@CREATEDAT
                                           ,@UPDATEDBY
                                           ,@UPDATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }

        internal static void bindFinDocHeaderParameter(SqlCommand cmd, FinanceDocumentViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@DOCTYPE", vm.DocType);
            cmd.Parameters.AddWithValue("@TRANDATE", vm.TranDate);
            cmd.Parameters.AddWithValue("@TRANCURR", vm.TranCurr);
            cmd.Parameters.AddWithValue("@DESP", vm.Desp);
            if (vm.ExgRate > 0)
                cmd.Parameters.AddWithValue("@EXGRATE", vm.ExgRate);
            else
                cmd.Parameters.AddWithValue("@EXGRATE", DBNull.Value);
            if (vm.ExgRate_Plan)
                cmd.Parameters.AddWithValue("@EXGRATE_PLAN", vm.ExgRate_Plan);
            else
                cmd.Parameters.AddWithValue("@EXGRATE_PLAN", DBNull.Value);
            if (vm.ExgRate_Plan2)
                cmd.Parameters.AddWithValue("@EXGRATE_PLAN2", vm.ExgRate_Plan2);
            else
                cmd.Parameters.AddWithValue("@EXGRATE_PLAN2", DBNull.Value);
            if (String.IsNullOrEmpty(vm.TranCurr2))
                cmd.Parameters.AddWithValue("@TRANCURR2", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@TRANCURR2", vm.TranCurr2);
            if (vm.ExgRate2 > 0)
                cmd.Parameters.AddWithValue("@EXGRATE2", vm.ExgRate2);
            else
                cmd.Parameters.AddWithValue("@EXGRATE2", DBNull.Value);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", vm.CreatedAt);
            cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
        }

        internal static void FinDocHeader_DB2VM(SqlDataReader reader, FinanceDocumentUIViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.HID = reader.GetInt32(idx++);
                vm.DocType = reader.GetInt16(idx++);
                vm.TranDate = reader.GetDateTime(idx++);
                vm.TranCurr = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Desp = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate = reader.GetDecimal(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan = reader.GetBoolean(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.TranCurr2 = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate2 = reader.GetDecimal(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan2 = reader.GetBoolean(idx++);
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
            }
            catch(Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: ID {0}, index {1}, {2}", vm.ID, idx, exp.Message));
                throw exp;
            }
        }
        #endregion

        #region Finance document Item
        internal static string getFinDocItemInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_document_item]
                            ([DOCID]
                            ,[ITEMID]
                            ,[ACCOUNTID]
                            ,[TRANTYPE]
                            ,[TRANAMOUNT]
                            ,[USECURR2]
                            ,[CONTROLCENTERID]
                            ,[ORDERID]
                            ,[DESP])
                        VALUES
                            (@DOCID
                            ,@ITEMID
                            ,@ACCOUNTID
                            ,@TRANTYPE
                            ,@TRANAMOUNT
                            ,@USECURR2
                            ,@CONTROLCENTERID
                            ,@ORDERID
                            ,@DESP)";
        }

        internal static void bindFinDocItemParameter(SqlCommand cmd2, FinanceDocumentItemViewModel ivm, Int32 nNewDocID)
        {
            cmd2.Parameters.AddWithValue("@DOCID", nNewDocID);
            cmd2.Parameters.AddWithValue("@ITEMID", ivm.ItemID);
            cmd2.Parameters.AddWithValue("@ACCOUNTID", ivm.AccountID);
            cmd2.Parameters.AddWithValue("@TRANTYPE", ivm.TranType);
            cmd2.Parameters.AddWithValue("@TRANAMOUNT", ivm.TranAmount);
            if (ivm.UseCurr2)
                cmd2.Parameters.AddWithValue("@USECURR2", ivm.UseCurr2);
            else
                cmd2.Parameters.AddWithValue("@USECURR2", DBNull.Value);
            if (ivm.ControlCenterID > 0)
                cmd2.Parameters.AddWithValue("@CONTROLCENTERID", ivm.ControlCenterID);
            else
                cmd2.Parameters.AddWithValue("@CONTROLCENTERID", DBNull.Value);
            if (ivm.OrderID > 0)
                cmd2.Parameters.AddWithValue("@ORDERID", ivm.OrderID);
            else
                cmd2.Parameters.AddWithValue("@ORDERID", DBNull.Value);
            cmd2.Parameters.AddWithValue("@DESP", String.IsNullOrEmpty(ivm.Desp) ? String.Empty : ivm.Desp);
        }

        internal static void FinDocItem_DB2VM(SqlDataReader reader, FinanceDocumentItemUIViewModel itemvm)
        {
            Int32 idx = 0;

            itemvm.DocID = reader.GetInt32(idx++);
            itemvm.ItemID = reader.GetInt32(idx++);
            itemvm.AccountID = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.AccountName = reader.GetString(idx++);
            else
                ++idx;
            itemvm.TranType = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.TranTypeName = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.UseCurr2 = reader.GetBoolean(idx++);
            else
                ++idx;
            itemvm.TranAmount = reader.GetDecimal(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.ControlCenterID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.ControlCenterName = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.OrderID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.OrderName = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.Desp = reader.GetString(idx++);
            else
                ++idx;
        }
        #endregion

        #region Finance Template Doc
        internal static String getFinanceTmpDocADPInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_tmpdoc_dp]
                                ([HID]
                                ,[REFDOCID]
                                ,[ACCOUNTID]
                                ,[TRANDATE]
                                ,[TRANTYPE]
                                ,[TRANAMOUNT]
                                ,[CONTROLCENTERID]
                                ,[ORDERID]
                                ,[DESP]
                                ,[CREATEDBY]
                                ,[CREATEDAT]
                                ,[UPDATEDBY]
                                ,[UPDATEDAT])
                            VALUES
                                (@HID
                                ,@REFDOCID
                                ,@ACCOUNTID
                                ,@TRANDATE
                                ,@TRANTYPE
                                ,@TRANAMOUNT
                                ,@CONTROLCENTERID
                                ,@ORDERID
                                ,@DESP
                                ,@CREATEDBY
                                ,@CREATEDAT
                                ,@UPDATEDBY
                                ,@UPDATEDAT)";
        }

        internal static void bindFinTmpDocADPParameter(SqlCommand cmd, FinanceTmpDocDPViewModel avm, Int32 nNewAccountID, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", avm.HID);
            if (avm.RefDocID.HasValue)
                cmd.Parameters.AddWithValue("@REFDOCID", avm.RefDocID.Value);
            else
                cmd.Parameters.AddWithValue("@REFDOCID", DBNull.Value);
            cmd.Parameters.AddWithValue("@ACCOUNTID", nNewAccountID);
            cmd.Parameters.AddWithValue("@TRANDATE", avm.TranDate);
            cmd.Parameters.AddWithValue("@TRANTYPE", avm.TranType);
            cmd.Parameters.AddWithValue("@TRANAMOUNT", avm.TranAmount);
            if (avm.ControlCenterID.HasValue)
                cmd.Parameters.AddWithValue("@CONTROLCENTERID", avm.ControlCenterID.Value);
            else
                cmd.Parameters.AddWithValue("@CONTROLCENTERID", DBNull.Value);
            if (avm.OrderID.HasValue)
                cmd.Parameters.AddWithValue("@ORDERID", avm.OrderID.Value);
            else
                cmd.Parameters.AddWithValue("@ORDERID", DBNull.Value);
            cmd.Parameters.AddWithValue("@DESP",
                String.IsNullOrEmpty(avm.Desp) ? String.Empty : avm.Desp);

            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
        }
        
        internal static void FinTmpDoc_DB2VM(SqlDataReader reader, FinanceTmpDocDPViewModel vm)
        {
            Int32 idx = 0;
            vm.DocID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                vm.RefDocID = reader.GetInt32(idx++);
            else
                ++idx;
            vm.AccountID = reader.GetInt32(idx++);
            vm.TranDate = reader.GetDateTime(idx++);
            vm.TranType = reader.GetInt32(idx++);
            vm.TranAmount = reader.GetDecimal(idx++);
            if (!reader.IsDBNull(idx))
                vm.ControlCenterID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.OrderID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Desp = reader.GetString(idx++);
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
        }
        #endregion

        #region Finance Account Category
        internal static string getFinanceAccountCategoryQueryString()
        {
            return String.Empty;
        }
        #endregion

        #region Finance Document item - Account View
        internal static String getFinDocItemAccountView(Int32 nAcntID)
        {
            return @"WITH A2 AS (
                    SELECT
	                      ROW_NUMBER() OVER (ORDER BY [TRANDATE] ASC) AS [ROWID]
	                      ,[DOCID]
                          ,[ITEMID]
                          ,[TRANDATE]
                          ,[DOCDESP]
                          ,[ACCOUNTID]
                          ,[ACCOUNTNAME]
                          ,[TRANTYPE]
                          ,[TRANTYPENAME]
                          ,[TRANTYPE_EXP]
                          ,[USECURR2]
                          ,[TRANCURR]
                          ,[TRANAMOUNT_ORG]
                          ,[TRANAMOUNT]
                          ,[TRANAMOUNT_LC]
                          ,[CONTROLCENTERID]
                          ,[CONTROLCENTERNAME]
                          ,[ORDERID]
                          ,[ORDERNAME]
                          ,[DESP]
                      FROM [dbo].[v_fin_document_item1]
                      WHERE [ACCOUNTID] = " + nAcntID.ToString() +
                    @")
                    SELECT [DOCID]
                          ,[ITEMID]
                          ,[TRANDATE]
                          ,[DOCDESP]
                          ,[ACCOUNTID]
                          ,[ACCOUNTNAME]
                          ,[TRANTYPE]
                          ,[TRANTYPENAME]
                          ,[TRANTYPE_EXP]
                          ,[USECURR2]
                          ,[TRANCURR]
                          ,[TRANAMOUNT_ORG]
                          ,[TRANAMOUNT]
                          ,[TRANAMOUNT_LC]
                          ,[CONTROLCENTERID]
                          ,[CONTROLCENTERNAME]
                          ,[ORDERID]
                          ,[ORDERNAME]
                          ,[DESP]
                          ,(SELECT SUM(T2.TRANAMOUNT_LC) FROM A2 AS T2 WHERE T2.ROWID <= T1.ROWID) AS BALANCE_LC FROM A2 AS T1";
        }

        internal static void FinDocItemWithBalanceList_DB2VM(SqlDataReader reader, FinanceDocumentItemWithBalanceUIViewModel avm)
        {
            avm.DocID = reader.GetInt32(0);
            avm.ItemID = reader.GetInt32(1);
            avm.TranDate = reader.GetDateTime(2);
            if (!reader.IsDBNull(3))
                avm.DocDesp = reader.GetString(3);
            avm.AccountID = reader.GetInt32(4);
            if (!reader.IsDBNull(5))
                avm.AccountName = reader.GetString(5);
            if (!reader.IsDBNull(6))
                avm.TranType = reader.GetInt32(6);
            if (!reader.IsDBNull(7))
                avm.TranTypeName = reader.GetString(7);
            if (!reader.IsDBNull(8))
                avm.TranType_Exp = reader.GetBoolean(8);
            if (!reader.IsDBNull(9))
                avm.UseCurr2 = reader.GetBoolean(9);
            if (!reader.IsDBNull(10))
                avm.TranCurr = reader.GetString(10);
            if (!reader.IsDBNull(11))
                avm.TranAmount_Org = reader.GetDecimal(11);
            if (!reader.IsDBNull(12))
                avm.TranAmount = reader.GetDecimal(12);
            if (!reader.IsDBNull(13))
                avm.TranAmount_LC = reader.GetDecimal(13);
            if (!reader.IsDBNull(14))
                avm.ControlCenterID = reader.GetInt32(14);
            if (!reader.IsDBNull(15))
                avm.ControlCenterName = reader.GetString(15);
            if (!reader.IsDBNull(16))
                avm.OrderID = reader.GetInt32(16);
            if (!reader.IsDBNull(17))
                avm.OrderName = reader.GetString(17);
            if (!reader.IsDBNull(18))
                avm.Desp = reader.GetString(18);
            avm.Balance = reader.GetDecimal(19);
        }
        #endregion

        #region Finance Document item - Control center View
        internal static String getFinDocItemControlCenterView(Int32 nCCID)
        {
            return @"WITH A2 AS (
                    SELECT
	                      ROW_NUMBER() OVER (ORDER BY [TRANDATE] ASC) AS [ROWID]
	                      ,[DOCID]
                          ,[ITEMID]
                          ,[TRANDATE]
                          ,[DOCDESP]
                          ,[ACCOUNTID]
                          ,[ACCOUNTNAME]
                          ,[TRANTYPE]
                          ,[TRANTYPENAME]
                          ,[TRANTYPE_EXP]
                          ,[USECURR2]
                          ,[TRANCURR]
                          ,[TRANAMOUNT_ORG]
                          ,[TRANAMOUNT]
                          ,[TRANAMOUNT_LC]
                          ,[CONTROLCENTERID]
                          ,[CONTROLCENTERNAME]
                          ,[ORDERID]
                          ,[ORDERNAME]
                          ,[DESP]
                      FROM [dbo].[v_fin_document_item1]
                      WHERE [CONTROLCENTERID] = " + nCCID.ToString() +
                    @")
                    SELECT [DOCID]
                          ,[ITEMID]
                          ,[TRANDATE]
                          ,[DOCDESP]
                          ,[ACCOUNTID]
                          ,[ACCOUNTNAME]
                          ,[TRANTYPE]
                          ,[TRANTYPENAME]
                          ,[TRANTYPE_EXP]
                          ,[USECURR2]
                          ,[TRANCURR]
                          ,[TRANAMOUNT_ORG]
                          ,[TRANAMOUNT]
                          ,[TRANAMOUNT_LC]
                          ,[CONTROLCENTERID]
                          ,[CONTROLCENTERNAME]
                          ,[ORDERID]
                          ,[ORDERNAME]
                          ,[DESP]
                          ,(SELECT SUM(T2.TRANAMOUNT_LC) FROM A2 AS T2 WHERE T2.ROWID <= T1.ROWID) AS BALANCE_LC FROM A2 AS T1";
        }
        #endregion

        #region Finance Document item - Control center View
        internal static String getFinDocItemOrderView(Int32 nOrderID)
        {
            return @"WITH A2 AS (
                    SELECT
	                      ROW_NUMBER() OVER (ORDER BY [TRANDATE] ASC) AS [ROWID]
	                      ,[DOCID]
                          ,[ITEMID]
                          ,[TRANDATE]
                          ,[DOCDESP]
                          ,[ACCOUNTID]
                          ,[ACCOUNTNAME]
                          ,[TRANTYPE]
                          ,[TRANTYPENAME]
                          ,[TRANTYPE_EXP]
                          ,[USECURR2]
                          ,[TRANCURR]
                          ,[TRANAMOUNT_ORG]
                          ,[TRANAMOUNT]
                          ,[TRANAMOUNT_LC]
                          ,[CONTROLCENTERID]
                          ,[CONTROLCENTERNAME]
                          ,[ORDERID]
                          ,[ORDERNAME]
                          ,[DESP]
                      FROM [dbo].[v_fin_document_item1]
                      WHERE [ORDERID] = " + nOrderID.ToString() +
                    @")
                    SELECT [DOCID]
                          ,[ITEMID]
                          ,[TRANDATE]
                          ,[DOCDESP]
                          ,[ACCOUNTID]
                          ,[ACCOUNTNAME]
                          ,[TRANTYPE]
                          ,[TRANTYPENAME]
                          ,[TRANTYPE_EXP]
                          ,[USECURR2]
                          ,[TRANCURR]
                          ,[TRANAMOUNT_ORG]
                          ,[TRANAMOUNT]
                          ,[TRANAMOUNT_LC]
                          ,[CONTROLCENTERID]
                          ,[CONTROLCENTERNAME]
                          ,[ORDERID]
                          ,[ORDERNAME]
                          ,[DESP]
                          ,(SELECT SUM(T2.TRANAMOUNT_LC) FROM A2 AS T2 WHERE T2.ROWID <= T1.ROWID) AS BALANCE_LC FROM A2 AS T1";
        }
        #endregion

        #region Lib book category
        internal static string getLibBookCategoryQueryString()
        {
            string strSQL = @"SELECT [ID]
                              ,[HID]
                              ,[Name]
                              ,[ParID]
                              ,[Others]
                              ,[CREATEDBY]
                              ,[CREATEDAT]
                              ,[UPDATEDBY]
                              ,[UPDATEDAT]
                          FROM [dbo].[t_lib_book_ctgy]";
            return strSQL;
        }

        internal static void LibBookCategory_DB2VM(SqlDataReader reader, LibBookCategoryViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.ID = reader.GetInt32(idx++);
                if (!reader.IsDBNull(idx))
                    vm.HID = reader.GetInt32(idx++);
                else
                    ++idx;
                vm.Name = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.ParentID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Others = reader.GetString(idx++);
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
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: ID {0}, index {1}, {2}", vm.ID, idx, exp.Message));
                throw exp;
            }
        }
        #endregion

        #region Tag
        internal static void GetTagTerms()
        {

        }
        internal static void RemoveTag()
        {

        }

        internal static void AddTag()
        {

        }
        #endregion

        #region Obsoleted methods
        internal static void FinDocDB2VM(SqlDataReader reader, BaseListViewModel<FinanceDocumentUIViewModel> listVMs)
        {
            Int32 nDocID = -1;
            while (reader.Read())
            {
                Int32 idx = 0;
                Int32 nCurrentID = reader.GetInt32(idx++);
                FinanceDocumentUIViewModel vm = null;
                if (nDocID != nCurrentID)
                {
                    nDocID = nCurrentID;
                    vm = new FinanceDocumentUIViewModel
                    {
                        ID = nCurrentID,
                        DocType = reader.GetInt16(idx++),
                        DocTypeName = reader.GetString(idx++),
                        TranDate = reader.GetDateTime(idx++),
                        TranCurr = reader.GetString(idx++)
                    };
                    if (!reader.IsDBNull(idx))
                        vm.Desp = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate = reader.GetDecimal(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate_Plan = reader.GetBoolean(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.TranCurr2 = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate2 = reader.GetDecimal(idx++);
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
                }
                else
                {
                    foreach (FinanceDocumentUIViewModel ovm in listVMs)
                    {
                        if (ovm.ID == nCurrentID)
                        {
                            vm = ovm;
                            break;
                        }
                    }
                }

                idx = 14; // Item part
                FinanceDocumentItemUIViewModel divm = new FinanceDocumentItemUIViewModel();
                if (!reader.IsDBNull(idx))
                    divm.ItemID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.AccountID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.AccountName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.TranType = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.TranAmount = reader.GetDecimal(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.UseCurr2 = reader.GetBoolean(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.ControlCenterID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.ControlCenterName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.OrderID = reader.GetInt32(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.OrderName = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    divm.Desp = reader.GetString(idx++);
                else
                    ++idx;
                vm.Items.Add(divm);

                if (nDocID != nCurrentID)
                {
                    listVMs.Add(vm);
                }
            }
        }
        #endregion
    }
}
