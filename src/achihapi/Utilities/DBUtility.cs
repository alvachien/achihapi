using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace achihapi.Utilities
{
    internal class HIHDBUtility
    {
        internal const String TSqlDateFormat = "yyyy-MM-dd";

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
        internal static string getHomeMsgQueryString(String urName, Int32 hid, Boolean sentbox)
        {
            return sentbox? @"SELECT [ID]
                      ,[t_homemsg].[HID]
                      ,[USERTO]
	                  ,mem2.[DISPLAYAS] AS [USERTO_DISPLAYAS]
                      ,[SENDDATE]
                      ,[USERFROM]
	                  ,mem1.[DISPLAYAS] AS [USERFROM_DISPLAYAS]
                      ,[TITLE]
                      ,[CONTENT]
                      ,[READFLAG]
                  FROM [dbo].[t_homemsg]
                  INNER JOIN [dbo].[t_homemem] as mem1
	                ON [t_homemsg].[HID] = mem1.[HID]
	                AND [t_homemsg].USERFROM = mem1.[USER]
                  INNER JOIN [dbo].[t_homemem] as mem2
	                ON [t_homemsg].[HID] = mem2.[HID]
	                AND [t_homemsg].USERTO = mem2.[USER] WHERE [t_homemsg].[HID] = "
                  + hid.ToString()  + (@" AND [t_homemsg].[SEND_DEL] IS NULL AND [t_homemsg].[USERFROM] = N'" + urName + "'" )
                  : @" SELECT[ID]
                      ,[t_homemsg].[HID]
                      ,[USERTO]
	                  ,mem2.[DISPLAYAS] AS[USERTO_DISPLAYAS]
                      ,[SENDDATE]
                      ,[USERFROM]
                      , mem1.[DISPLAYAS] AS[USERFROM_DISPLAYAS]
                      ,[TITLE]
                      ,[CONTENT]
                      ,[READFLAG]
                    FROM [dbo].[t_homemsg]
                       INNER JOIN [dbo].[t_homemem] as mem1
                    ON [t_homemsg].[HID] = mem1.[HID]
                    AND [t_homemsg].USERFROM = mem1.[USER]
                       INNER JOIN [dbo].[t_homemem] as mem2
                    ON [t_homemsg].[HID] = mem2.[HID]
                    AND [t_homemsg].USERTO = mem2.[USER] WHERE [t_homemsg].[HID] = "
                    + hid.ToString() + (@" AND [t_homemsg].[REV_DEL] IS NULL AND [t_homemsg].[USERTO] = N'" + urName + "'");
        }

        internal static void HomeMsg_DB2VM(SqlDataReader reader, HomeMsgViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.HID = reader.GetInt32(idx++);
                vm.UserTo = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.UserTo_DisplayAs = reader.GetString(idx++);
                else
                    ++idx;
                vm.SendDate = reader.GetDateTime(idx++);
                vm.UserFrom = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.UserFrom_DisplayAs = reader.GetString(idx++);
                else
                    ++idx;
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

        internal static string HomeMsg_GetMarkAsReadUpdateString()
        {
            return @"UPDATE [dbo].[t_homemsg]
                       SET [READFLAG] = @readflag
                     WHERE [ID] = @id AND [HID] = @hid";
        }
        internal static void HomeMsg_BindMarkAsReadUpdateParameters(SqlCommand cmd, Boolean readFlag, Int32 id, Int32 hid)
        {
            cmd.Parameters.AddWithValue("@readflag", readFlag);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@hid", hid);
        }
        internal static string HomeMsg_GetSenderDeletionUpdateString()
        {
            return @"UPDATE [dbo].[t_homemsg]
                       SET [SEND_DEL] = @senddel
                     WHERE [ID] = @id AND [HID] = @hid";
        }
        internal static void HomeMsg_BindSenderDeletioUpdateParameters(SqlCommand cmd, Boolean delflag, Int32 id, Int32 hid)
        {
            cmd.Parameters.AddWithValue("@senddel", delflag);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@hid", hid);
        }
        internal static string HomeMsg_GetReceiverDeletionUpdateString()
        {
            return @"UPDATE [dbo].[t_homemsg]
                       SET [REV_DEL] = @revdel
                     WHERE [ID] = @id AND [HID] = @hid";
        }
        internal static void HomeMsg_BindReceiverDeletionUpdateParameters(SqlCommand cmd, Boolean delflag, Int32 id, Int32 hid)
        {
            cmd.Parameters.AddWithValue("@revdel", delflag);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@hid", hid);
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
                      ,[t_fin_account_ext_loan].[STARTDATE] AS [LOAN_STARTDATE]
                      ,[t_fin_account_ext_loan].[ANNUALRATE] AS [LOAN_ANNUALRATE]
                      ,[t_fin_account_ext_loan].[INTERESTFREE] AS [LOAN_INTERESTFREE]
                      ,[t_fin_account_ext_loan].[REPAYMETHOD] AS [LOAN_REPAYMETHOD]
                      ,[t_fin_account_ext_loan].[TOTALMONTH] AS [LOAN_TOTALMONTH]
                      ,[t_fin_account_ext_loan].[REFDOCID] AS [LOAN_REFDOCID]
                      ,[t_fin_account_ext_loan].[OTHERS] AS [LOAN_OTHERS]
                      ,[t_fin_account_ext_loan].[EndDate] AS [LOAN_ENDDATE]
                      ,[t_fin_account_ext_loan].[PAYINGACCOUNT] AS [LOAN_PAYINGACCOUNT]
                      ,[t_fin_account_ext_loan].[PARTNER] AS [LOAN_PARTNER]
                  FROM [dbo].[t_fin_account]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_dp]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_dp].[ACCOUNTID]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_as]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_as].[ACCOUNTID]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_loan]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_loan].[ACCOUNTID]
                  ";

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

        internal static string GetFinanceAccountStatusUpdateString()
        {
            return @"UPDATE [dbo].[t_fin_account]
                           SET [STATUS] = @STATUS
                              ,[UPDATEDBY] = @UPDATEDBY
                              ,[UPDATEDAT] = @UPDATEDAT
                         WHERE [ID] = @ID AND [HID] = @HID";
        }

        internal static void BindFinAccountStatusUpdateParameter(SqlCommand cmd, FinanceAccountStatus nstatus, Int32 acntID, Int32 hid, String usrName)
        {
            cmd.Parameters.AddWithValue("@STATUS", nstatus);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@ID", acntID);
            cmd.Parameters.AddWithValue("@HID", hid);
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
                vm.Status = (FinanceAccountStatus)reader.GetByte(idx++);
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
                vmdp.RptType = (RepeatFrequency)reader.GetByte(idx++);
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

        internal static void FinAccountLoan_DB2VM(SqlDataReader reader, FinanceAccountExtLoanViewModel vmdp, Int32 idx)
        {
            if (!reader.IsDBNull(idx))
                vmdp.StartDate = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.AnnualRate = reader.GetDecimal(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.InterestFree = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.RepaymentMethod = (LoanRepaymentMethod)reader.GetByte(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.TotalMonths = reader.GetInt16(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.RefDocID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.Others = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.EndDate = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.PayingAccount = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vmdp.Partner = reader.GetString(idx++);
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
                    idx += 5;
                }

                if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom
                    || vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_LendTo)
                {
                    vm.ExtraInfo_Loan = new FinanceAccountExtLoanViewModel();
                    FinAccountLoan_DB2VM(reader, vm.ExtraInfo_Loan, idx);
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

        internal static string GetFinanceAccountAssetUpdateSoldDocString()
        {
            return @"UPDATE [dbo].[t_fin_account_ext_as]
                       SET [REFDOC_SOLD] = @REFDOC_SOLD
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
        internal static void BindFinAccountAssetUpdateSoldDocParameter(SqlCommand cmd, Int32 rdoc, Int32 nacntid)
        {
            cmd.Parameters.AddWithValue("@REFDOC_SOLD", rdoc);
            cmd.Parameters.AddWithValue("@ACCOUNTID", nacntid);
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

        #region Finance Account Extra: Loan
        internal static string GetFinanceAccountLoanInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_account_ext_loan]
                   ([ACCOUNTID]
                   ,[STARTDATE]
                   ,[ANNUALRATE]
                   ,[INTERESTFREE]
                   ,[REPAYMETHOD]
                   ,[TOTALMONTH]
                   ,[REFDOCID]
                   ,[OTHERS]
                   ,[ENDDATE]
                   ,[PAYINGACCOUNT]
                   ,[PARTNER])
             VALUES
                   (@ACCOUNTID
                   ,@STARTDATE
                   ,@ANNUALRATE
                   ,@INTERESTFREE
                   ,@REPAYMETHOD
                   ,@TOTALMONTH
                   ,@REFDOCID
                   ,@OTHERS
                   ,@ENDDATE
                   ,@PAYINGACCOUNT
                   ,@PARTNER)";
        }

        internal static void BindFinAccountLoanInsertParameter(SqlCommand cmd, FinanceAccountExtLoanViewModel vm, Int32 nNewDocID, Int32 nNewAccountID, String usrName)
        {
            cmd.Parameters.AddWithValue("@ACCOUNTID", nNewAccountID);
            cmd.Parameters.AddWithValue("@STARTDATE", vm.StartDate);
            if (vm.AnnualRate.HasValue)
                cmd.Parameters.AddWithValue("@ANNUALRATE", vm.AnnualRate.Value);
            else
                cmd.Parameters.AddWithValue("@ANNUALRATE", DBNull.Value);
            if (vm.InterestFree.HasValue)
                cmd.Parameters.AddWithValue("@INTERESTFREE", vm.InterestFree.Value);
            else
                cmd.Parameters.AddWithValue("@INTERESTFREE", DBNull.Value);
            if (vm.RepaymentMethod.HasValue)
                cmd.Parameters.AddWithValue("@REPAYMETHOD", vm.RepaymentMethod.Value);
            else
                cmd.Parameters.AddWithValue("@REPAYMETHOD", DBNull.Value);
            if (vm.TotalMonths.HasValue)
                cmd.Parameters.AddWithValue("@TOTALMONTH", vm.TotalMonths.Value);
            else
                cmd.Parameters.AddWithValue("@TOTALMONTH", DBNull.Value);
            cmd.Parameters.AddWithValue("@REFDOCID", nNewDocID);
            cmd.Parameters.AddWithValue("@OTHERS",
                String.IsNullOrEmpty(vm.Others) ? String.Empty : vm.Others);
            if (vm.EndDate.HasValue)
                cmd.Parameters.AddWithValue("@ENDDATE", vm.EndDate.Value);
            else
                cmd.Parameters.AddWithValue("@ENDDATE", DBNull.Value);
            if (vm.PayingAccount.HasValue)
                cmd.Parameters.AddWithValue("@PAYINGACCOUNT", vm.PayingAccount.Value);
            else
                cmd.Parameters.AddWithValue("@PAYINGACCOUNT", DBNull.Value);
            if (!String.IsNullOrEmpty(vm.Partner))
                cmd.Parameters.AddWithValue("@PARTNER", vm.Partner);
            else
                cmd.Parameters.AddWithValue("@PARTNER", DBNull.Value);
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
        internal static string getFinanceDocQueryString(Int32 nid, Int32 hid)
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
                          ,[TRANTYPE]
                          ,[TRANAMOUNT]
                          ,[USECURR2]
                          ,[CONTROLCENTERID]
                          ,[ORDERID]
                          ,[DESP]
                        FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + nid.ToString() + @";
                    SELECT [TagSubID], [Term]
                      FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString() + " AND [TagType] = 10 AND [TagID] = " + nid.ToString();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        internal static string getFinanceDocAssetQueryString(Int32 nid, Boolean isBuyIn, Int32 hid)
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
                          ,[TRANTYPE]
                          ,[TRANAMOUNT]
                          ,[USECURR2]
                          ,[CONTROLCENTERID]
                          ,[ORDERID]
                          ,[DESP]
                        FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + nid.ToString() + @";
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
                        + ( isBuyIn? " AND [t_fin_account_ext_as].[REFDOC_BUY] = " : " AND [t_fin_account_ext_as].[REFDOC_SOLD] = ") + nid.ToString() + @";
                    SELECT [TagSubID], [Term]
                      FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString() + " AND [TagType] = 10 AND [TagID] = " + nid.ToString();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }
        #endregion

        #region Finance document Header
        internal static string GetFinDocHeaderInsertString()
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
                                           ,[CREATEDAT])
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
                                           ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }

        internal static string GetFinDocHeaderUpdateString()
        {
            return @"UPDATE [dbo].[t_fin_document]
                       SET [HID] = @HID
                          ,[DOCTYPE] = @DOCTYPE
                          ,[TRANDATE] = @TRANDATE
                          ,[TRANCURR] = @TRANCURR
                          ,[DESP] = @DESP
                          ,[EXGRATE] = @EXGRATE
                          ,[EXGRATE_PLAN] = @EXGRATE_PLAN
                          ,[EXGRATE_PLAN2] = @EXGRATE_PLAN2
                          ,[TRANCURR2] = @TRANCURR2
                          ,[EXGRATE2] = @EXGRATE2
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";
        }

        internal static string GetFinDocHeaderExistCheckString(Int32 id)
        {
            return @"SELECT [ID] FROM [t_fin_document] WHERE [ID] = " + id.ToString();
        }

        internal static string GetFinDocHeader_PatchString(Boolean? tranDate, Boolean? desp)
        {
            if (!tranDate.HasValue && !desp.HasValue)
                throw new Exception("Invalid caller");

            List<String> listStr = new List<string>();
            if (tranDate.HasValue && tranDate.Value)
                listStr.Add("[TRANDATE] = @TRANDATE");
            if (desp.HasValue && desp.Value)
                listStr.Add("[DESP] = @DESP");
            var result = String.Join(", ", listStr.ToArray());
            return @"UPDATE [dbo].[t_fin_document] SET "
                    + result
                    + ", [UPDATEDAT] = @UPDATEDAT, [UPDATEDBY] = @UPDATEDBY WHERE [ID] = @ID";
        }

        internal static void BindFinDocHeaderInsertParameter(SqlCommand cmd, FinanceDocumentViewModel vm, String usrName)
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
        }

        internal static void BindFinDocHeaderUpdateParameter(SqlCommand cmd, FinanceDocumentViewModel vm, String usrName)
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
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@ID", vm.ID);
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
        internal static string GetFinDocItemInsertString()
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

        internal static void BindFinDocItemInsertParameter(SqlCommand cmd2, FinanceDocumentItemViewModel ivm, Int32 nNewDocID)
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
            //if (!reader.IsDBNull(idx))
            //    itemvm.AccountName = reader.GetString(idx++);
            //else
            //    ++idx;
            itemvm.TranType = reader.GetInt32(idx++);
            //if (!reader.IsDBNull(idx))
            //    itemvm.TranTypeName = reader.GetString(idx++);
            //else
            //    ++idx;
            itemvm.TranAmount = reader.GetDecimal(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.UseCurr2 = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.ControlCenterID = reader.GetInt32(idx++);
            else
                ++idx;
            //if (!reader.IsDBNull(idx))
            //    itemvm.ControlCenterName = reader.GetString(idx++);
            //else
            //    ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.OrderID = reader.GetInt32(idx++);
            else
                ++idx;
            //if (!reader.IsDBNull(idx))
            //    itemvm.OrderName = reader.GetString(idx++);
            //else
            //    ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.Desp = reader.GetString(idx++);
            else
                ++idx;
        }

        internal static String getFinDocItemSearchView(String strWhere, Int32? top = null, Int32? skip = null)
        {
            String strRst = @" SELECT COUNT(*) FROM [dbo].[V_FIN_DOCUMENT_ITEM] "
                    + (String.IsNullOrEmpty(strWhere) ? "" : (" WHERE " + strWhere))
                    + @"; SELECT [DOCID],
                    [ITEMID],
		            [TRANDATE],
		            [DOCDESP],
                    [ACCOUNTID],
                    [TRANTYPE],
		            [TRANTYPENAME],
		            [TRANTYPE_EXP],
		            [USECURR2],
                    [TRANCURR],
                    [TRANAMOUNT_ORG],
                    [TRANAMOUNT],
                    [TRANAMOUNT_LC],
                    [CONTROLCENTERID],
                    [ORDERID],
                    [DESP]
                    FROM [dbo].[V_FIN_DOCUMENT_ITEM] "
                    + (String.IsNullOrEmpty(strWhere) ? "" : (" WHERE " + strWhere));
            if (top.HasValue)
                strRst += " ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;";

            return strRst;
        }
        internal static void FinDocItem_SearchView2VM(SqlDataReader reader, FinanceDocItemSearchResultViewModel itemvm)
        {
            Int32 idx = 0;

            itemvm.DocID = reader.GetInt32(idx++);
            itemvm.ItemID = reader.GetInt32(idx++);
            itemvm.TranDate = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.DocDesp = reader.GetString(idx++);
            else
                ++idx;
            itemvm.AccountID = reader.GetInt32(idx++);
            itemvm.TranType = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.TranTypeName = reader.GetString(idx++);
            else
                ++idx;
            itemvm.TranType_Exp = reader.GetBoolean(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.UseCurr2 = reader.GetBoolean(idx++);
            else
                ++idx;
            itemvm.TranCurr = reader.GetString(idx++);
            itemvm.TranAmount_Org = reader.GetDecimal(idx++);
            itemvm.TranAmount = reader.GetDecimal(idx++);
            itemvm.TranAmount_LC = reader.GetDecimal(idx++);
            if (!reader.IsDBNull(idx))
                itemvm.ControlCenterID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.OrderID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                itemvm.Desp = reader.GetString(idx++);
            else
                ++idx;
        }
        #endregion

        #region Finance Template Doc - ADP
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

        internal static string getFinanceDocADPQueryString(Int32 nid, Int32 hid)
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
                          ,[TRANTYPE]
                          ,[TRANAMOUNT]
                          ,[USECURR2]
                          ,[CONTROLCENTERID]
                          ,[ORDERID]
                          ,[DESP]
                        FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + nid.ToString() + @";
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
	                    AND [dbo].[t_fin_account_ext_dp].[REFDOCID] = " + nid.ToString() + @";
                    SELECT [TagSubID], [Term]
                      FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString() + " AND [TagType] = 10 AND [TagID] = " + nid.ToString();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        internal static string GetFinanceDocADPDeleteString(Int32 hid, Int32 acntid, Boolean? delunpostdoco = null)
        {
            String strrst = @"DELETE FROM [t_fin_tmpdoc_dp] WHERE [HID] = " + hid.ToString()
                 + " AND [ACCOUNTID] = " + acntid.ToString();
            if (delunpostdoco.HasValue && delunpostdoco.Value)
                strrst += " AND [REFDOCID] IS NULL";
            return strrst;
        }
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
        
        internal static void FinTmpDocADP_DB2VM(SqlDataReader reader, FinanceTmpDocDPViewModel vm)
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

        #region Finance Template Doc - Loan
        internal static string GetFinanceDocLoanDeleteString(Int32 hid, Int32 acntid, Boolean? delunpostdoco = null)
        {
            String strrst = @"DELETE FROM [t_fin_tmpdoc_loan] WHERE [HID] = " + hid.ToString()
                 + " AND [ACCOUNTID] = " + acntid.ToString();
            if (delunpostdoco.HasValue && delunpostdoco.Value)
                strrst += " AND [REFDOCID] IS NULL";
            return strrst;
        }

        internal static string GetFinanceDocLoanListQueryString()
        {
            return @"SELECT [DOCID]
                          ,[HID]
                          ,[REFDOCID]
                          ,[ACCOUNTID]
                          ,[TRANDATE]
                          ,[TRANAMOUNT]
                          ,[INTERESTAMOUNT]
                          ,[CONTROLCENTERID]
                          ,[ORDERID]
                          ,[DESP]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                        FROM [dbo].[t_fin_tmpdoc_loan]";
        }
        internal static string GetFinanceLoanAccountQueryString(Int32 hid, Int32 acntid)
        {
            return @"SELECT [t_fin_account].[ID]
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
                            ,[t_fin_account_ext_loan].[STARTDATE]
                            ,[t_fin_account_ext_loan].[ANNUALRATE]
                            ,[t_fin_account_ext_loan].[INTERESTFREE]
                            ,[t_fin_account_ext_loan].[REPAYMETHOD]
                            ,[t_fin_account_ext_loan].[TOTALMONTH]
                            ,[t_fin_account_ext_loan].[REFDOCID]
                            ,[t_fin_account_ext_loan].[OTHERS]
                            ,[t_fin_account_ext_loan].[ENDDATE]
                            ,[t_fin_account_ext_loan].[PAYINGACCOUNT]
                            ,[t_fin_account_ext_loan].[PARTNER]
                        FROM [dbo].[t_fin_account]
                        LEFT OUTER JOIN [dbo].[t_fin_account_ext_loan]
                            ON [t_fin_account].[ID] = [t_fin_account_ext_loan].[ACCOUNTID]
                        WHERE [t_fin_account].[CTGYID] IN ( "
                        + FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom.ToString()
                        + ", " + FinanceAccountCtgyViewModel.AccountCategory_LendTo.ToString()
                        + " ) AND [t_fin_account].[ID] = " + acntid.ToString(); 
        }

        internal static string GetFinanceDocLoanQueryString(Int32 nid, Int32 hid)
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
                          ,[TRANTYPE]
                          ,[TRANAMOUNT]
                          ,[USECURR2]
                          ,[CONTROLCENTERID]
                          ,[ORDERID]
                          ,[DESP]
                        FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + nid.ToString() + @";
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
                            ,[t_fin_account_ext_loan].[STARTDATE]
                            ,[t_fin_account_ext_loan].[ANNUALRATE]
                            ,[t_fin_account_ext_loan].[INTERESTFREE]
                            ,[t_fin_account_ext_loan].[REPAYMETHOD]
                            ,[t_fin_account_ext_loan].[TOTALMONTH]
                            ,[t_fin_account_ext_loan].[REFDOCID]
                            ,[t_fin_account_ext_loan].[OTHERS]
                            ,[t_fin_account_ext_loan].[ENDDATE]
                            ,[t_fin_account_ext_loan].[PAYINGACCOUNT]
                            ,[t_fin_account_ext_loan].[PARTNER]
                        FROM [dbo].[t_fin_account]
                        LEFT OUTER JOIN [dbo].[t_fin_account_ext_loan]
                            ON [t_fin_account].[ID] = [t_fin_account_ext_loan].[ACCOUNTID]
                        WHERE [t_fin_account].[CTGYID] IN ( "
                        + FinanceAccountCtgyViewModel.AccountCategory_BorrowFrom.ToString()
                        + ", " + FinanceAccountCtgyViewModel.AccountCategory_LendTo.ToString()
                        + " ) AND [t_fin_account_ext_loan].[REFDOCID] = " + nid.ToString() + @"; 
                      SELECT [t_fin_tmpdoc_loan].[DOCID]
                          ,[t_fin_tmpdoc_loan].[HID]
                          ,[t_fin_tmpdoc_loan].[REFDOCID]
                          ,[t_fin_tmpdoc_loan].[ACCOUNTID]
                          ,[t_fin_tmpdoc_loan].[TRANDATE]
                          ,[t_fin_tmpdoc_loan].[TRANAMOUNT]
                          ,[t_fin_tmpdoc_loan].[INTERESTAMOUNT]
                          ,[t_fin_tmpdoc_loan].[CONTROLCENTERID]
                          ,[t_fin_tmpdoc_loan].[ORDERID]
                          ,[t_fin_tmpdoc_loan].[DESP]
                          ,[t_fin_tmpdoc_loan].[CREATEDBY]
                          ,[t_fin_tmpdoc_loan].[CREATEDAT]
                          ,[t_fin_tmpdoc_loan].[UPDATEDBY]
                          ,[t_fin_tmpdoc_loan].[UPDATEDAT]
                      FROM [dbo].[t_fin_tmpdoc_loan]
	                    INNER JOIN [dbo].[t_fin_account_ext_loan]
	                    ON [dbo].[t_fin_tmpdoc_loan].[ACCOUNTID] = [dbo].[t_fin_account_ext_loan].[ACCOUNTID]
	                    AND [dbo].[t_fin_account_ext_loan].[REFDOCID] = " + nid.ToString() + @";
                    SELECT [TagSubID], [Term]
                      FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString() + " AND [TagType] = 10 AND [TagID] = " + nid.ToString();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(strSQL);
#endif

            return strSQL;
        }

        internal static String GetFinanceTmpDocLoanInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_tmpdoc_loan]
                                ([HID]
                                ,[REFDOCID]
                                ,[ACCOUNTID]
                                ,[TRANDATE]
                                ,[TRANAMOUNT]
                                ,[INTERESTAMOUNT]
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
                                ,@TRANAMOUNT
                                ,@INTERESTAMOUNT
                                ,@CONTROLCENTERID
                                ,@ORDERID
                                ,@DESP
                                ,@CREATEDBY
                                ,@CREATEDAT
                                ,@UPDATEDBY
                                ,@UPDATEDAT)";
        }

        internal static void BindFinTmpDocLoanParameter(SqlCommand cmd, FinanceTmpDocLoanViewModel avm, Int32 nNewAccountID, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", avm.HID);
            if (avm.RefDocID.HasValue)
                cmd.Parameters.AddWithValue("@REFDOCID", avm.RefDocID.Value);
            else
                cmd.Parameters.AddWithValue("@REFDOCID", DBNull.Value);
            cmd.Parameters.AddWithValue("@ACCOUNTID", nNewAccountID);
            cmd.Parameters.AddWithValue("@TRANDATE", avm.TranDate);
            // cmd.Parameters.AddWithValue("@TRANTYPE", avm.TranType);
            cmd.Parameters.AddWithValue("@TRANAMOUNT", avm.TranAmount);
            if (avm.InterestAmount.HasValue)
                cmd.Parameters.AddWithValue("@INTERESTAMOUNT", avm.InterestAmount.Value);
            else
                cmd.Parameters.AddWithValue("@INTERESTAMOUNT", DBNull.Value);
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

        internal static void FinTmpDocLoan_DB2VM(SqlDataReader reader, FinanceTmpDocLoanViewModel vm)
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
            //vm.TranType = reader.GetInt32(idx++);
            vm.TranAmount = reader.GetDecimal(idx++);
            if (!reader.IsDBNull(idx))
                vm.InterestAmount = reader.GetDecimal(idx++);
            else
                ++idx;
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
        internal static String getFinDocItemAccountView(Int32 nAcntID, Int32? top = null, Int32? skip = null, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            String strAcnt = " [ACCOUNTID] = " + nAcntID.ToString();
            String strWhereClause = strAcnt;
            String strRange = "";
            if (dtbgn.HasValue || dtend.HasValue)
            {
                List<String> listStr = new List<string>();
                if (dtbgn.HasValue)
                {
                    String strval = " [TRANDATE] >= '" + dtbgn.Value.ToString(TSqlDateFormat) + "'";
                    strWhereClause += " AND " + strval;
                    listStr.Add(strval);
                }                    
                if (dtend.HasValue)
                {
                    String strval = " [TRANDATE] <= '" + dtend.Value.ToString(TSqlDateFormat) + "'";
                    strWhereClause += " AND " + strval;
                    listStr.Add(strval);
                }

                strRange = String.Join(" AND ", listStr);
            }
            String strRst = @"SELECT COUNT(*) FROM [dbo].[v_fin_document_item1] WHERE " + strWhereClause
                + @"; WITH A2 AS (SELECT
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
                      WHERE " + strAcnt 
                      + @") " + (String.IsNullOrEmpty(strRange) ? "" : " SELECT * FROM ( ")
                        + @" SELECT [DOCID]
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
                          ,(SELECT SUM(T2.TRANAMOUNT_LC) FROM A2 AS T2 WHERE T2.ROWID <= T1.ROWID) AS BALANCE_LC FROM A2 AS T1" 
                        + (String.IsNullOrEmpty(strRange) ? "" : ") AS T3 WHERE  " + strRange);
            if (top.HasValue)
                strRst += " ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue ? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;";

            return strRst;
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
        internal static String getFinDocItemControlCenterView(Int32 nCCID, Int32? top = null, Int32? skip = null, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            String strCC = " [CONTROLCENTERID] = " + nCCID.ToString();
            String strWhereClause = strCC;
            String strRange = "";
            if (dtbgn.HasValue || dtend.HasValue)
            {
                List<String> listStr = new List<string>();
                if (dtbgn.HasValue)
                {
                    String strval = " [TRANDATE] >= '" + dtbgn.Value.ToString(TSqlDateFormat) + "'";
                    strWhereClause += " AND " + strval;
                    listStr.Add(strval);
                }
                if (dtend.HasValue)
                {
                    String strval = " [TRANDATE] <= '" + dtend.Value.ToString(TSqlDateFormat) + "'";
                    strWhereClause += " AND " + strval;
                    listStr.Add(strval);
                }

                strRange = String.Join(" AND ", listStr);
            }
            String strRst = @"SELECT COUNT(*) FROM [dbo].[v_fin_document_item1] WHERE " + strWhereClause
                    + @"; WITH A2 AS ( SELECT
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
                      WHERE " + strCC
                    + @") " + (String.IsNullOrEmpty(strRange) ? "" : " SELECT * FROM ( ")
                    + @"SELECT [DOCID]
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
                          ,(SELECT SUM(T2.TRANAMOUNT_LC) FROM A2 AS T2 WHERE T2.ROWID <= T1.ROWID) AS BALANCE_LC FROM A2 AS T1 "
                    + (String.IsNullOrEmpty(strRange) ? "" : ") AS T3 WHERE  " + strRange);
            if (top.HasValue)
                strRst += " ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue ? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;";

            return strRst;
        }
        #endregion

        #region Finance Document item - Control center View
        internal static String getFinDocItemOrderView(Int32 nOrderID, Int32? top = null, Int32? skip = null, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            String strOrder = " [ORDERID] = " + nOrderID.ToString();
            String strWhereClause = strOrder;
            String strRange = "";
            if (dtbgn.HasValue || dtend.HasValue)
            {
                List<String> listStr = new List<string>();
                if (dtbgn.HasValue)
                {
                    String strval = " [TRANDATE] >= '" + dtbgn.Value.ToString(TSqlDateFormat) + "'";
                    strWhereClause += " AND " + strval;
                    listStr.Add(strval);
                }
                if (dtend.HasValue)
                {
                    String strval = " [TRANDATE] <= '" + dtend.Value.ToString(TSqlDateFormat) + "'";
                    strWhereClause += " AND " + strval;
                    listStr.Add(strval);
                }

                strRange = String.Join(" AND ", listStr);
            }
            String strRst = @"SELECT COUNT(*) FROM [dbo].[v_fin_document_item1] WHERE " + strWhereClause
                      + @"; WITH A2 AS ( SELECT
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
                      WHERE " + strWhereClause
                    + @") " + (String.IsNullOrEmpty(strRange) ? "" : " SELECT * FROM ( ")
                    + @"SELECT [DOCID]
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
                          ,(SELECT SUM(T2.TRANAMOUNT_LC) FROM A2 AS T2 WHERE T2.ROWID <= T1.ROWID) AS BALANCE_LC FROM A2 AS T1 "
                    + (String.IsNullOrEmpty(strRange) ? "" : ") AS T3 WHERE  " + strRange);
            if (top.HasValue)
                strRst += " ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue ? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;";

            return strRst;
        }
        #endregion

        #region Order header
        internal static string GetFinOrderInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_order]
                               ([HID]            
                               ,[NAME]
                               ,[VALID_FROM]
                               ,[VALID_TO]
                               ,[COMMENT]
                               ,[CREATEDBY]
                               ,[CREATEDAT] )
                         VALUES
                               (@HID
                               ,@NAME
                               ,@VALID_FROM
                               ,@VALID_TO
                               ,@COMMENT
                               ,@CREATEDBY
                               ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }
        internal static void BindFinOrderInsertParameter(SqlCommand cmd, FinanceOrderViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@VALID_FROM", vm.ValidFrom);
            cmd.Parameters.AddWithValue("@VALID_TO", vm.ValidTo);
            cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
        }
        internal static string GetFinOrderUpdateString()
        {
            return @"UPDATE[dbo].[t_fin_order]
                        SET[NAME] = @NAME
                            ,[VALID_FROM] = @VALID_FROM
                            ,[VALID_TO] = @VALID_TO
                            ,[COMMENT] = @COMMENT
                            ,[UPDATEDBY] = @UPDATEDBY
                            ,[UPDATEDAT] = @UPDATEDAT
                         WHERE [HID] = @HID AND [ID] = @ID";
        }
        internal static void BindFinOrderUpdateParameter(SqlCommand cmd, FinanceOrderViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@VALID_FROM", vm.ValidFrom);
            cmd.Parameters.AddWithValue("@VALID_TO", vm.ValidTo);
            cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", vm.CreatedAt);
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@ID", vm.ID);
        }
        #endregion

        #region Order - S. Rule
        internal static string GetFinOrderSRuleInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_order_srule]
                                    ([ORDID]
                                    ,[RULEID]
                                    ,[CONTROLCENTERID]
                                    ,[PRECENT]
                                    ,[COMMENT])
                                VALUES
                                    (@ORDID
                                    ,@RULEID
                                    ,@CONTROLCENTERID
                                    ,@PRECENT
                                    ,@COMMENT)";
        }
        internal static void BindFinOrderSRuleInsertParameter(SqlCommand cmd2, FinanceOrderSRuleUIViewModel suivm, Int32 ordid)
        {
            cmd2.Parameters.AddWithValue("@ORDID", ordid);
            cmd2.Parameters.AddWithValue("@RULEID", suivm.RuleID);
            cmd2.Parameters.AddWithValue("@CONTROLCENTERID", suivm.ControlCenterID);
            cmd2.Parameters.AddWithValue("@PRECENT", suivm.Precent);
            cmd2.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(suivm.Comment) ? String.Empty : suivm.Comment);
        }
        internal static string GetFinOrderSRuleDeleteString()
        {
            return @"DELETE FROM[dbo].[t_fin_order_srule] WHERE[ORDID] = @ID";
        }
        internal static void BindFinOrderSRuleDeleteParameter(SqlCommand cmd, Int32 ordid)
        {
            cmd.Parameters.AddWithValue("@ID", ordid);
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

        #region Lib book category
        internal static string getLibMovieGenreQueryString()
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
                          FROM [dbo].[t_lib_movie_genre]";
            return strSQL;
        }

        internal static void LibMovieGenre_DB2VM(SqlDataReader reader, LibMovieGenreViewModel vm)
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

        #region Normal Event
        internal static string Event_GetNormalEventQueryString(Boolean listmode, String usrid, Int32? hid = null, Int32? skip = null, Int32? top = null, Int32? id = null, 
            Boolean? skipfinish = null, DateTime? dtbgn = null, DateTime? dtend = null)
        {
            if (String.IsNullOrEmpty(usrid)
                || (listmode && !hid.HasValue)
                || (!listmode && !id.HasValue))
                throw new Exception("Invalid input!");

            StringBuilder sb = new StringBuilder();
            if (listmode)
            {
                var sql1 = @"SELECT count(*) FROM [dbo].[t_event] WHERE[HID] = " + hid.ToString() + " AND [Assignee] = N'" + usrid + "' ";
                if (dtbgn.HasValue)
                    sql1 += " AND [StartTime] >= '" + dtbgn.Value.ToString("yyyy-MM-dd") + "' ";
                if (dtend.HasValue)
                    sql1 += " AND [StartTime] <= '" + dtend.Value.ToString("yyyy-MM-dd") + "' "; 
                if (skipfinish.HasValue)
                    sql1 += ( skipfinish.Value ? " AND [CompleteTime] IS NULL " : " [CompleteTime] IS NOT NULL ");
                sb.AppendLine(sql1);
            }            

            sb.Append(@"; SELECT [ID]
                          ,[HID]
                          ,[Name]
                          ,[StartTime]
                          ,[EndTime]
                          ,[CompleteTime]");
            if (!listmode)
                sb.Append(@",[Content]");
            sb.Append(@",[IsPublic]
                        ,[Assignee]
                        ,[RefRecurID]
                        ,[CREATEDBY]
                        ,[CREATEDAT]
                        ,[UPDATEDBY]
                        ,[UPDATEDAT]
                      FROM [dbo].[t_event] ");

            if (listmode)
            {
                sb.Append(" WHERE [HID] = " + hid.ToString() + " AND [Assignee] = N'" + usrid + "' ");
                if (dtbgn.HasValue)
                    sb.Append(" AND [StartTime] >= '" + dtbgn.Value.ToString("yyyy-MM-dd") + "' ");
                if (dtend.HasValue)
                    sb.Append(" AND [StartTime] <= '" + dtend.Value.ToString("yyyy-MM-dd") + "' ");
                if (skipfinish.HasValue)
                    sb.Append(skipfinish.Value ? " AND [CompleteTime] IS NULL " : " [CompleteTime] IS NOT NULL ");
                if (top.HasValue)
                    sb.Append(" ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue ? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;");
            }
            else
                sb.Append(" WHERE [ID] = " + id.Value.ToString());

            return sb.ToString();
        }
        internal static string Event_GetNormalEventByRecurIDString()
        {
            return @"SELECT [ID]
                          ,[HID]
                          ,[Name]
                          ,[StartTime]
                          ,[EndTime]
                          ,[CompleteTime] 
                          ,[IsPublic]
                          ,[Assignee]
                          ,[RefRecurID]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                           FROM [dbo].[t_event] WHERE [HID] = @HID AND [RefRecurID] =@RECURID";
        }
        internal static void Event_BindNormalEventByRecurIDParameters(SqlCommand cmd, Int32 hid, Int32 recurid)
        {
            cmd.Parameters.AddWithValue("@HID", hid);
            cmd.Parameters.AddWithValue("@RECURID", recurid);
        }
        internal static void Event_DB2VM(SqlDataReader reader, EventViewModel vm, Boolean listmode)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.Name = reader.GetString(idx++);
            vm.StartTimePoint = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                vm.EndTimePoint = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.CompleteTimePoint = reader.GetDateTime(idx++);
            else
                ++idx;
            if (!listmode)
                vm.Content = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.IsPublic = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Assignee = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.RefRecurrID = reader.GetInt32(idx++);
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

        internal static string Event_GetNormalEventInsertString(Boolean bNeedID = true)
        {
            return @"INSERT INTO [dbo].[t_event]
                       ([HID]
                       ,[Name]
                       ,[StartTime]
                       ,[EndTime]
                       ,[CompleteTime]
                       ,[Content]
                       ,[IsPublic]
                       ,[Assignee]
                       ,[RefRecurID]
                       ,[CREATEDBY]
                       ,[CREATEDAT])
                 VALUES
                       (@HID
                       ,@Name
                       ,@StartTime
                       ,@EndTime
                       ,@CompleteTime
                       ,@Content
                       ,@IsPublic
                       ,@Assignee
                       ,@RefRecurID
                       ,@CREATEDBY
                       ,@CREATEDAT); " + ( bNeedID ? " SELECT @Identity = SCOPE_IDENTITY();" : "" );
        }
        internal static void Event_BindNormalEventInsertParameters(SqlCommand cmd, EventViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@Name", vm.Name);
            cmd.Parameters.AddWithValue("@StartTime", vm.StartTimePoint);
            if (vm.EndTimePoint.HasValue)
                cmd.Parameters.AddWithValue("@EndTime", vm.EndTimePoint.Value);
            else
                cmd.Parameters.AddWithValue("@EndTime", DBNull.Value);
            if (vm.CompleteTimePoint.HasValue)
                cmd.Parameters.AddWithValue("@CompleteTime", vm.CompleteTimePoint.Value);
            else
                cmd.Parameters.AddWithValue("@CompleteTime", DBNull.Value);
            cmd.Parameters.AddWithValue("@Content", vm.Content);
            cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
            if (!String.IsNullOrEmpty(vm.Assignee))
                cmd.Parameters.AddWithValue("@Assignee", vm.Assignee);
            else
                cmd.Parameters.AddWithValue("@Assignee", DBNull.Value);
            if (vm.RefRecurrID.HasValue)
                cmd.Parameters.AddWithValue("@RefRecurID", vm.RefRecurrID.Value);
            else
                cmd.Parameters.AddWithValue("@RefRecurID", DBNull.Value);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
        }

        internal static string Event_GetNormalEventUpdateString()
        {
            return @"UPDATE [dbo].[t_event]
                       SET [Name] = @Name
                          ,[StartTime] = @StartTime
                          ,[EndTime] = @EndTime
                          ,[CompleteTime] = @CompleteTime
                          ,[Content] = @Content
                          ,[IsPublic] = @IsPublic
                          ,[Assignee] = @Assignee
                          ,[RefRecurID] = @RefRecurID
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";
        }
        internal static void Event_BindNormalEventUpdateParameters(SqlCommand cmd, EventViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@Name", vm.Name);
            cmd.Parameters.AddWithValue("@StartTime", vm.StartTimePoint);
            if (vm.EndTimePoint.HasValue)
                cmd.Parameters.AddWithValue("@EndTime", vm.EndTimePoint.Value);
            else
                cmd.Parameters.AddWithValue("@EndTime", DBNull.Value);
            if (vm.CompleteTimePoint.HasValue)
                cmd.Parameters.AddWithValue("@CompleteTime", vm.CompleteTimePoint.Value);
            else
                cmd.Parameters.AddWithValue("@CompleteTime", DBNull.Value);
            cmd.Parameters.AddWithValue("@Content", vm.Content);
            cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
            if (!String.IsNullOrEmpty(vm.Assignee))
                cmd.Parameters.AddWithValue("@Assignee", vm.Assignee);
            else
                cmd.Parameters.AddWithValue("@Assignee", DBNull.Value);
            if (vm.RefRecurrID.HasValue)
                cmd.Parameters.AddWithValue("@RefRecurID", vm.RefRecurrID.Value);
            else
                cmd.Parameters.AddWithValue("@RefRecurID", DBNull.Value);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@ID", vm.ID);
        }
        internal static string Event_GetNormalEventMarkAsCompleteString()
        {
            return @"UPDATE [dbo].[t_event]
                       SET [CompleteTime] = @CompleteTime
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";
        }
        internal static void Event_BindNormalEventMarkAsCompleteParameters(SqlCommand cmd, DateTime dtComplete, String usrName, Int32 id)
        {
            cmd.Parameters.AddWithValue("@CompleteTime", dtComplete);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@ID", id);
        }

        internal static string Event_GetNormalEventDeletionString()
        {
            return @"DELETE FROM [dbo].[t_event] WHERE [HID] = @HID AND [ID] = @ID";
        }
        internal static void Event_BindNormalEventDeletionParameters(SqlCommand cmd, Int32 hid, Int32 id)
        {
            cmd.Parameters.AddWithValue("@HID", hid);
            cmd.Parameters.AddWithValue("@ID", id);
        }
        internal static string Event_GetNormalEventForRecurDeletionString()
        {
            return @"DELETE FROM [dbo].[t_event] WHERE [HID] = @HID AND [RefRecurID] = @RECURID ";
        }
        internal static void Event_BindNormalEventForRecurDeletionParameters(SqlCommand cmd, Int32 hid, Int32 recurID)
        {
            cmd.Parameters.AddWithValue("@HID", hid);
            cmd.Parameters.AddWithValue("@RECURID", recurID);
        }
        #endregion

        #region Recur Event
        internal static string Event_GetRecurEventQueryString(Boolean listmode, String usrid, Int32? hid = null, Int32? skip = null, Int32? top = null, Int32? id = null)
        {
            if (String.IsNullOrEmpty(usrid)
                || (listmode && !hid.HasValue)
                || (!listmode && !id.HasValue))
                throw new Exception("Invalid input!");

            StringBuilder sb = new StringBuilder();
            if (listmode)
                sb.AppendLine(@"SELECT count(*) FROM [dbo].[t_event_recur] WHERE[HID] = " + hid.ToString());

            sb.Append(@"; SELECT [ID]
                          ,[HID]
                          ,[STARTDATE]
                          ,[ENDDATE]
                          ,[RPTTYPE]
                          ,[NAME]");
            if (!listmode)
                sb.Append(@",[Content]");
            sb.Append(@",[ISPUBLIC]
                          ,[ASSIGNEE]
                          ,[CREATEDBY]
                          ,[CREATEDAT]
                          ,[UPDATEDBY]
                          ,[UPDATEDAT]
                      FROM [dbo].[t_event_recur] ");

            if (listmode)
            {
                sb.Append(" WHERE [HID] = " + hid.ToString());
                if (top.HasValue)
                    sb.Append(" ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue ? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;");
            }
            else
                sb.Append(" WHERE [ID] = " + id.Value.ToString());

            return sb.ToString();
        }
        internal static void Event_RecurDB2VM(SqlDataReader reader, RecurEventViewModel vm, Boolean listmode)
        {
            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.StartTimePoint = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                vm.EndTimePoint = reader.GetDateTime(idx++);
            else
                ++idx;
            vm.RptType = (RepeatFrequency)reader.GetByte(idx++);
            vm.Name = reader.GetString(idx++);
            if (!listmode)
                vm.Content = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                vm.IsPublic = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                vm.Assignee = reader.GetString(idx++);
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

        internal static string Event_GetRecurEventInsertString()
        {
            return @"INSERT INTO [dbo].[t_event_recur]
                   ([HID]
                   ,[STARTDATE]
                   ,[ENDDATE]
                   ,[RPTTYPE]
                   ,[NAME]
                   ,[CONTENT]
                   ,[ISPUBLIC]
                   ,[ASSIGNEE]
                   ,[CREATEDBY]
                   ,[CREATEDAT])
                 VALUES
                    (@HID
                    ,@StartTime
                    ,@EndTime
                    ,@RptType
                    ,@Name
                    ,@Content
                    ,@IsPublic
                    ,@Assignee
                    ,@CREATEDBY
                    ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }
        internal static void Event_BindRecurEventInsertParameters(SqlCommand cmd, RecurEventViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@StartTime", vm.StartTimePoint);
            cmd.Parameters.AddWithValue("@EndTime", vm.EndTimePoint);
            cmd.Parameters.AddWithValue("@RptType", vm.RptType);
            cmd.Parameters.AddWithValue("@Name", vm.Name);
            cmd.Parameters.AddWithValue("@Content", vm.Content);
            cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
            if (!String.IsNullOrEmpty(vm.Assignee))
                cmd.Parameters.AddWithValue("@Assignee", vm.Assignee);
            else
                cmd.Parameters.AddWithValue("@Assignee", DBNull.Value);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);

        }

        internal static string Event_GetRecurEventUpdateString()
        {
            return @"UPDATE [dbo].[t_event_recur]
                       SET [RPTTYPE] = @RptType
                          ,[StartTime] = @StartTime
                          ,[EndTime] = @EndTime
                          ,[Name] = @Name
                          ,[Content] = @Content
                          ,[IsPublic] = @IsPublic
                          ,[Assignee] = @Assignee
                          ,[UPDATEDBY] = @UPDATEDBY
                          ,[UPDATEDAT] = @UPDATEDAT
                     WHERE [ID] = @ID";
        }
        internal static void Event_BindRecurEventUpdateParameters(SqlCommand cmd, RecurEventViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@RptType", vm.RptType);
            cmd.Parameters.AddWithValue("@StartTime", vm.StartTimePoint);
            cmd.Parameters.AddWithValue("@EndTime", vm.EndTimePoint);
            cmd.Parameters.AddWithValue("@Name", vm.Name);
            cmd.Parameters.AddWithValue("@Content", vm.Content);
            cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
            if (!String.IsNullOrEmpty(vm.Assignee))
                cmd.Parameters.AddWithValue("@Assignee", vm.Assignee);
            else
                cmd.Parameters.AddWithValue("@Assignee", DBNull.Value);
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@ID", vm.ID);
        }
        internal static string Event_GetRecurEventDeletionString()
        {
            return @"DELETE FROM [dbo].[t_event_recur] WHERE [HID]= @hid AND [ID] = @id ";
        }
        internal static void Event_BindRecurEventDeletionParameters(SqlCommand cmd, Int32 hid, Int32 id)
        {
            cmd.Parameters.AddWithValue("@HID", hid);
            cmd.Parameters.AddWithValue("@ID", id);
        }
        #endregion

        #region Habit Event (with detail, check in statistics)
        internal static string Event_GetHabitDetailWithCheckInSearchString()
        {
            return @"SELECT [HID], [HabitID], [ExpAmount], [CHECKINAMT], [ASCORE], [StartDate], [EndDate], [Name],[Assignee],[IsPublic]
	                    FROM [v_event_habitdetail_withcheckin] as a
	                    WHERE (a.CHECKINAMT IS NULL OR a.CHECKINAMT < a.ExpAmount)
                        AND [HID] = @HID
	                    AND ( ( a.EndDate >= @STARTDATE AND a.ENDDATE < @ENDDATE) OR ( a.StartDate < @ENDDATE AND a.StartDate >= @STARTDATE ) )";
                        //ORDER BY (SELECT NULL)
                        //OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY";
        }
        internal static void Event_BindHabitDetailWithCheckInSearchParameter(SqlCommand cmd, Int32 hid, DateTime startdate, DateTime enddate)
        {
            cmd.Parameters.AddWithValue("@HID", hid);
            cmd.Parameters.AddWithValue("@ENDDATE", enddate);
            cmd.Parameters.AddWithValue("@STARTDATE", startdate);
        }
        internal static void Event_HabitDetailWithCheckInDB2VM(SqlDataReader reader, EventHabitDetailWithCheckInViewModel vm)
        {
            vm.HID = reader.GetInt32(0);
            vm.HabitID = reader.GetInt32(1);
            vm.ExpectAmount = reader.GetInt32(2);
            if (!reader.IsDBNull(3))
                vm.CheckInAmount = reader.GetInt32(3);
            else
                vm.CheckInAmount = 0;
            if (!reader.IsDBNull(4))
                vm.AverageScore = reader.GetInt32(4);
            else
                vm.AverageScore = 0;
            vm.StartDate = reader.GetDateTime(5);
            vm.EndDate = reader.GetDateTime(6);
            vm.Name = reader.GetString(7);
            if (!reader.IsDBNull(8))
                vm.Assignee = reader.GetString(8);
            if (!reader.IsDBNull(9))
                vm.IsPublic = reader.GetBoolean(9);
        }
        #endregion

        #region Event habit
        internal static string Event_GetEventHabitQueryString(Boolean listmode, String usrid, Int32? hid = null, Int32? skip = null, Int32? top = null, Int32? id = null)
        {
            if (String.IsNullOrEmpty(usrid)
                || (listmode && !hid.HasValue)
                || (!listmode && !id.HasValue))
                throw new Exception("Invalid input!");

            StringBuilder sb = new StringBuilder();
            if (listmode)
                sb.AppendLine(@"SELECT count(*) FROM [dbo].[t_event_habit] WHERE[HID] = " + hid.ToString());
            sb.Append(@"; SELECT [ID]
                          ,[HID]
                          ,[Name]
                          ,[StartDate]
                          ,[EndDate]
                          ,[RPTTYPE]
                          ,[IsPublic]");
            if (!listmode)
                sb.Append(@",[Content]");
            sb.Append(@",[Count]
                        ,[Assignee]
                        ,[DetailID]");
            if (!listmode)
                sb.Append(@",[DetailStartDate]
                        ,[DetailEndDate]");
            sb.Append(@",[CREATEDBY]
                        ,[CREATEDAT]
                        ,[UPDATEDBY]
                        ,[UPDATEDAT]
                      FROM [dbo].[v_event_habitdetail] ");

            if (listmode)
            {
                sb.Append(" WHERE [HID] = " + hid.ToString());
                if (top.HasValue)
                    sb.Append(" ORDER BY (SELECT NULL) OFFSET " + (skip.HasValue ? skip.Value.ToString() : "0") + " ROWS FETCH NEXT " + top.Value.ToString() + " ROWS ONLY;");
            }
            else
            {
                sb.Append(" WHERE [ID] = " + id.Value.ToString() + ";");
                // Checkin history
                sb.Append(@"SELECT [ID],[TranDate],[Score],[Comment],[CREATEDBY],[CREATEDAT],[UPDATEDBY],[UPDATEDAT] FROM [t_event_habit_checkin] WHERE [HabitID] = " + id.Value.ToString());
            }
                

            return sb.ToString();
        }
        internal static void Event_HabitDB2VM(SqlDataReader reader, EventHabitViewModel vm, EventHabitDetail detail, Boolean listMode)
        {
            System.Diagnostics.Debug.Assert(vm != null);
            System.Diagnostics.Debug.Assert(detail != null);

            Int32 idx = 0;
            vm.ID = reader.GetInt32(idx++);
            vm.HID = reader.GetInt32(idx++);
            vm.Name = reader.GetString(idx++);
            vm.StartDate = reader.GetDateTime(idx++);
            vm.EndDate = reader.GetDateTime(idx++);
            vm.RptType = (RepeatFrequency)reader.GetByte(idx++);
            if (!reader.IsDBNull(idx))
                vm.IsPublic = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!listMode)
                vm.Content = reader.GetString(idx++);
            vm.Count = reader.GetInt32(idx++);
            if (!reader.IsDBNull(idx))
                vm.Assignee = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                detail.ID = reader.GetInt32(idx++);
            else
                ++idx;
            if (!listMode)
            {
                if (!reader.IsDBNull(idx))
                    detail.StartDate = reader.GetDateTime(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    detail.EndDate = reader.GetDateTime(idx++);
                else
                    ++idx;
            }
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
        internal static string Event_GetEventHabitInsertString()
        {
            return @"INSERT INTO [dbo].[t_event_habit]
                   ([HID]
                   ,[Name]
                   ,[StartDate]
                   ,[EndDate]
                   ,[RPTTYPE]
                   ,[IsPublic]
                   ,[CONTENT]
                   ,[Count]
                   ,[Assignee]
                   ,[CREATEDBY]
                   ,[CREATEDAT])
             VALUES
                   (@HID
                   ,@Name
                   ,@StartDate
                   ,@EndDate
                   ,@RPTTYPE
                   ,@IsPublic
                   ,@CONTENT
                   ,@Count
                   ,@Assignee
                   ,@CREATEDBY
                   ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }
        internal static void Event_BindEventHabitInsertParameters(SqlCommand cmd, EventHabitViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HID);
            cmd.Parameters.AddWithValue("@Name", vm.Name);
            cmd.Parameters.AddWithValue("@StartDate", vm.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", vm.EndDate);
            cmd.Parameters.AddWithValue("@RPTTYPE", vm.RptType);
            cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
            if (!String.IsNullOrEmpty(vm.Content))
                cmd.Parameters.AddWithValue("@CONTENT", vm.Content);
            else
                cmd.Parameters.AddWithValue("@CONTENT", DBNull.Value);
            cmd.Parameters.AddWithValue("@Count", vm.Count);
            if (!String.IsNullOrEmpty(vm.Assignee))
                cmd.Parameters.AddWithValue("@Assignee", vm.Assignee);
            else
                cmd.Parameters.AddWithValue("@Assignee", DBNull.Value);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
        }
        internal static string Event_GetEventHabitDetailInsertString()
        {
            return @"INSERT INTO [dbo].[t_event_habit_detail]
                       ([HabitID]
                       ,[StartDate]
                       ,[EndDate])
                 VALUES
                       (@HabitID
                       ,@StartDate
                       ,@EndDate)";
        }
        internal static void Event_BindEventHabitDetailInsertParameter(SqlCommand cmd, EventHabitDetail detailvm)
        {
            cmd.Parameters.AddWithValue("@HabitID", detailvm.HabitID);
            cmd.Parameters.AddWithValue("@StartDate", detailvm.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", detailvm.EndDate);
        }
        internal static string Event_GetEventHabitDeleteString()
        {
            return @"DELETE FROM [dbo].[t_event_habit] WHERE [ID] = @id AND [HID] = @hid;";
        }
        internal static void Event_BindEventHabitDeleteParameters(SqlCommand cmd, Int32 id, Int32 hid)
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@hid", hid);
        }

        internal static string Event_GetEventHabitCheckInInsertString()
        {
            return @"INSERT INTO [dbo].[t_event_habit_checkin]
                   ([TranDate]
                   ,[HabitID]
                   ,[Score]
                   ,[Comment]
                   ,[CREATEDBY]
                   ,[CREATEDAT])
             VALUES
                   (@tranDate
                   ,@habitID
                   ,@score
                   ,@comment
                   ,@CREATEDBY
                   ,@CREATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }
        internal static void Event_BindEventHabitCheckInInsertParameters(SqlCommand cmd, EventHabitCheckInViewModel civm, String usrName)
        {
            System.Diagnostics.Debug.Assert(civm != null);
            cmd.Parameters.AddWithValue("@tranDate", civm.TranDate);
            cmd.Parameters.AddWithValue("@habitID", civm.HabitID);
            cmd.Parameters.AddWithValue("@score", civm.Score);
            if (String.IsNullOrEmpty(civm.Comment))
                cmd.Parameters.AddWithValue("@comment", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@comment", civm.Comment);
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
        }
        #endregion

        #region Tag
        internal static string GetTagInsertString()
        {
            return @"INSERT INTO [dbo].[t_tag]
                            ([HID]
                            ,[TagType]
                            ,[TagID]
                            ,[TagSubID]
                            ,[Term])
                        VALUES (@HID
                            ,@TagType
                            ,@TagID
                            ,@TagSubID
                            ,@Term)";
        }

        internal static void BindTagInsertParameter(SqlCommand cmd2, Int32 hid, HIHTagTypeEnum tagType, Int32 nTagID, String term, Int32? nTagSubID = null)
        {
            cmd2.Parameters.AddWithValue("@HID", hid);
            cmd2.Parameters.AddWithValue("@TagType", tagType);
            cmd2.Parameters.AddWithValue("@TagID", nTagID);
            if (nTagSubID.HasValue)
                cmd2.Parameters.AddWithValue("@TagSubID", nTagSubID.Value);
            else
                cmd2.Parameters.AddWithValue("@TagSubID", DBNull.Value);
            cmd2.Parameters.AddWithValue("@Term", term);
        }

        internal static string GetTagDeleteString(Boolean incSub = false)
        {
            String strSql = @"DELETE FROM [dbo].[t_tag]
                    WHERE [HID] = @HID AND [TagType] = @TagType AND [TagID] = @TagID ";

            if (incSub)
                return strSql + @" AND [TagSubID] = @TagSubID";

            return strSql;
        }

        internal static void BindTagDeleteParameter(SqlCommand cmd2, Int32 hid, HIHTagTypeEnum tagType, Int32 nTagID, Int32? nTagSubID = null)
        {
            cmd2.Parameters.AddWithValue("@HID", hid);
            cmd2.Parameters.AddWithValue("@TagType", tagType);
            cmd2.Parameters.AddWithValue("@TagID", nTagID);
            if (nTagSubID.HasValue)
                cmd2.Parameters.AddWithValue("@TagSubID", nTagSubID.Value);
        }

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
