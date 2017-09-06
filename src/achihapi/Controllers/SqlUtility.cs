using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    internal class SqlUtility
    {
        #region Home define
        internal static string getHomeDefQueryString(String strUserID = null)
        {
            String strSQL = @"SELECT [ID]
                          ,[NAME]
                          ,[HOST]
                          ,[DETAILS]
                          ,[USER]
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
                if (!reader.IsDBNull(idx))
                    vm.Details = reader.GetString(idx++);
                else
                    ++idx;
                // User ID - skipping
                idx++;
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
                   ,[CREATEDBY]
                   ,[CREATEDAT])
                   VALUES
                   (@NAME
                   ,@DETAILS
                   ,@HOST
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
            cmd.Parameters.AddWithValue("@UPDATEDBY", usrName);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DateTime.Now);
        }
        #endregion

        #region Home member
        internal static string getHomeMemQueryString(Int32? hid)
        {
            return @"SELECT [HID]
                          ,[USER]
                          ,[USERID]
                          ,[PRIV_LRN_OBJ]
                          ,[PRIV_LRN_HIST]
                          ,[PRIV_LRN_AWD]
                          ,[PRIV_LRN_PLAN]
                          ,[PRIV_LRN_CTGY]
                          ,[PRIV_FIN_SET]
                          ,[PRIV_FIN_CUR]
                          ,[PRIV_FIN_ACNT]
                          ,[PRIV_FIN_DOC]
                          ,[PRIV_FIN_CC]
                          ,[PRIV_FIN_ORD]
                          ,[PRIV_FIN_RPT]
                          ,[PRIV_EVENT]
                          ,[PRIV_LIB_BOOK]
                          ,[PRIV_LIB_MOV]
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
                       ,[USERID]
                       ,[PRIV_LRN_OBJ]
                       ,[PRIV_LRN_HIST]
                       ,[PRIV_LRN_AWD]
                       ,[PRIV_LRN_PLAN]
                       ,[PRIV_LRN_CTGY]
                       ,[PRIV_FIN_SET]
                       ,[PRIV_FIN_CUR]
                       ,[PRIV_FIN_ACNT]
                       ,[PRIV_FIN_DOC]
                       ,[PRIV_FIN_CC]
                       ,[PRIV_FIN_ORD]
                       ,[PRIV_FIN_RPT]
                       ,[PRIV_EVENT]
                       ,[PRIV_LIB_BOOK]
                       ,[PRIV_LIB_MOV]
                       ,[CREATEDBY]
                       ,[CREATEDAT])
                 VALUES
                       (@HID
                       ,@USER
                       ,@USERID
                       ,@PRIV_LRN_OBJ
                       ,@PRIV_LRN_HIST
                       ,@PRIV_LRN_AWD
                       ,@PRIV_LRN_PLAN
                       ,@PRIV_LRN_CTGY
                       ,@PRIV_FIN_SET
                       ,@PRIV_FIN_CUR
                       ,@PRIV_FIN_ACNT
                       ,@PRIV_FIN_DOC
                       ,@PRIV_FIN_CC
                       ,@PRIV_FIN_ORD
                       ,@PRIV_FIN_RPT
                       ,@PRIV_EVENT
                       ,@PRIV_LIB_BOOK
                       ,@PRIV_LIB_MOV
                       ,@CREATEDBY
                       ,@CREATEDAT
                        )";
        }

        internal static void bindHomeMemInsertParameter(SqlCommand cmd, HomeMemViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@HID", vm.HomeID);
            cmd.Parameters.AddWithValue("@USER", vm.User);
            if (String.IsNullOrEmpty(vm.UserID))
                cmd.Parameters.AddWithValue("@USERID", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@USERID", vm.UserID);
            if (String.IsNullOrEmpty(vm.Priv_LearnObject))
                cmd.Parameters.AddWithValue("@PRIV_LRN_OBJ", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LRN_OBJ", vm.Priv_LearnObject);
            if (String.IsNullOrEmpty(vm.Priv_LearnHist))
                cmd.Parameters.AddWithValue("@PRIV_LRN_HIST", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LRN_HIST", vm.Priv_LearnHist);
            if (String.IsNullOrEmpty(vm.Priv_LearnAward))
                cmd.Parameters.AddWithValue("@PRIV_LRN_AWD", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LRN_AWD", vm.Priv_LearnAward);
            if (String.IsNullOrEmpty(vm.Priv_LearnPlan))
                cmd.Parameters.AddWithValue("@PRIV_LRN_PLAN", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LRN_PLAN", vm.Priv_LearnPlan);
            if (String.IsNullOrEmpty(vm.Priv_LearnCategory))
                cmd.Parameters.AddWithValue("@PRIV_LRN_CTGY", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LRN_CTGY", vm.Priv_LearnCategory);

            if (String.IsNullOrEmpty(vm.Priv_FinanceSetting))
                cmd.Parameters.AddWithValue("@PRIV_FIN_SET", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_SET", vm.Priv_FinanceSetting);
            if (String.IsNullOrEmpty(vm.Priv_FinanceCurrency))
                cmd.Parameters.AddWithValue("@PRIV_FIN_CUR", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_CUR", vm.Priv_FinanceCurrency);
            if (String.IsNullOrEmpty(vm.Priv_FinanceAccount))
                cmd.Parameters.AddWithValue("@PRIV_FIN_ACNT", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_ACNT", vm.Priv_FinanceAccount);
            if (String.IsNullOrEmpty(vm.Priv_FinanceDocument))
                cmd.Parameters.AddWithValue("@PRIV_FIN_DOC", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_DOC", vm.Priv_FinanceDocument);
            if (String.IsNullOrEmpty(vm.Priv_FinanceControlCenter))
                cmd.Parameters.AddWithValue("@PRIV_FIN_CC", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_CC", vm.Priv_FinanceControlCenter);
            if (String.IsNullOrEmpty(vm.Priv_FinanceOrder))
                cmd.Parameters.AddWithValue("@PRIV_FIN_ORD", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_ORD", vm.Priv_FinanceOrder);
            if (String.IsNullOrEmpty(vm.Priv_FinanceReport))
                cmd.Parameters.AddWithValue("@PRIV_FIN_RPT", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_FIN_RPT", vm.Priv_FinanceReport);
            if (String.IsNullOrEmpty(vm.Priv_Event))
                cmd.Parameters.AddWithValue("@PRIV_EVENT", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_EVENT", vm.Priv_Event);
            if (String.IsNullOrEmpty(vm.Priv_LibBook))
                cmd.Parameters.AddWithValue("@PRIV_LIB_BOOK", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LIB_BOOK", vm.Priv_LibBook);
            if (String.IsNullOrEmpty(vm.Priv_LibMovie))
                cmd.Parameters.AddWithValue("@PRIV_LIB_MOV", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@PRIV_LIB_MOV", vm.Priv_LibMovie);

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
                    vm.UserID = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LearnObject = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LearnHist = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LearnAward = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LearnPlan = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LearnCategory = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceSetting = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceCurrency = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceAccount = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceDocument = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceControlCenter = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceOrder = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_FinanceReport = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_Event = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LibBook = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.Priv_LibMovie = reader.GetString(idx++);
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
                System.Diagnostics.Debug.WriteLine(String.Format("Error occurred: HID {0}, index {1}, {2}", vm.HomeID, idx, exp.Message));
                throw exp;
            }
        }

        internal static String getHomeMemUpdateString()
        {
            return @"UPDATE [dbo].[t_homemem]
                       SET [USERID] = @USERID
                          ,[PRIV_LRN_OBJ] = @PRIV_LRN_OBJ
                          ,[PRIV_LRN_HIST] = @PRIV_LRN_HIST
                          ,[PRIV_LRN_AWD] = @PRIV_LRN_AWD
                          ,[PRIV_LRN_PLAN] = @PRIV_LRN_PLAN
                          ,[PRIV_LRN_CTGY] = @PRIV_LRN_CTGY
                          ,[PRIV_FIN_SET] = @PRIV_FIN_SET
                          ,[PRIV_FIN_CUR] = @PRIV_FIN_CUR
                          ,[PRIV_FIN_ACNT] = @PRIV_FIN_ACNT
                          ,[PRIV_FIN_DOC] = @PRIV_FIN_DOC
                          ,[PRIV_FIN_CC] = @PRIV_FIN_CC
                          ,[PRIV_FIN_ORD] = @PRIV_FIN_ORD
                          ,[PRIV_FIN_RPT] = @PRIV_FIN_RPT
                          ,[PRIV_EVENT] = @PRIV_EVENT
                          ,[PRIV_LIB_BOOK] = @PRIV_LIB_BOOK
                          ,[PRIV_LIB_MOV] = @PRIV_LIB_MOV
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

        #region Learn History
        internal static string getLearnHistoryQueryString(String strUser = "")
        {
            String strSQL = @"SELECT [t_learn_hist].[USERID]
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
                            INNER JOIN [dbo].[t_learn_obj] ON [t_learn_hist].[OBJECTID] = [t_learn_obj].[ID] ";
            if (!String.IsNullOrEmpty(strUser))
            {
                strSQL += " WHERE [t_learn_hist].[USERID] = N'" + strUser + "'";
            }

            return strSQL;
        }
        #endregion

        #region Finance Account
        internal static string getFinanceAccountQueryString(String strOwner = "")
        {
            String strSQL = @"SELECT [t_fin_account].[ID]
                      ,[t_fin_account].[CTGYID]
                      ,[t_fin_account_ctgy].[NAME] as [CTGYNAME]
                      ,[t_fin_account].[NAME]
                      ,[t_fin_account].[COMMENT]
                      ,[t_fin_account].[OWNER]
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
                  LEFT OUTER JOIN [dbo].[t_fin_account_ctgy]
                       ON [t_fin_account].[CTGYID] = [t_fin_account_ctgy].[ID]
                  LEFT OUTER JOIN [dbo].[t_fin_account_ext_dp]
                       ON [t_fin_account].[ID] = [t_fin_account_ext_dp].[ACCOUNTID] ";
            if (!String.IsNullOrEmpty(strOwner))
            {
                strSQL += " WHERE [t_fin_account].[OWNER] = N'" + strOwner + "'";
            }

            return strSQL;
        }

        internal static string getFinanceAccountInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_account]
                                ([CTGYID]
                                ,[NAME]
                                ,[COMMENT]
                                ,[OWNER]
                                ,[CREATEDBY]
                                ,[CREATEDAT]
                                ,[UPDATEDBY]
                                ,[UPDATEDAT])
                            VALUES
                                (@CTGYID
                                ,@NAME
                                ,@COMMENT
                                ,@OWNER
                                ,@CREATEDBY
                                ,@CREATEDAT
                                ,@UPDATEDBY
                                ,@UPDATEDAT
                            ); SELECT @Identity = SCOPE_IDENTITY();";
        }

        internal static void bindFinAccountParameter(SqlCommand cmd, FinanceAccountViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@CTGYID", vm.CtgyID);
            cmd.Parameters.AddWithValue("@NAME", vm.Name);
            cmd.Parameters.AddWithValue("@COMMENT", String.IsNullOrEmpty(vm.Comment) ? String.Empty : vm.Comment);
            if (String.IsNullOrEmpty(vm.Owner))
            {
                cmd.Parameters.AddWithValue("@OWNER", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@OWNER", vm.Owner);
            }
            cmd.Parameters.AddWithValue("@CREATEDBY", usrName);
            cmd.Parameters.AddWithValue("@CREATEDAT", DateTime.Now);
            cmd.Parameters.AddWithValue("@UPDATEDBY", DBNull.Value);
            cmd.Parameters.AddWithValue("@UPDATEDAT", DBNull.Value);
        }
        
        internal static void FinAccount_DB2VM(SqlDataReader reader, FinanceAccountUIViewModel vm)
        {
            Int32 idx = 0;

            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.CtgyID = reader.GetInt32(idx++);
                if (!reader.IsDBNull(idx))
                    vm.CtgyName = reader.GetString(idx++);
                else
                    ++idx;
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

                if (vm.CtgyID == FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment)
                {
                    vm.AdvancePaymentInfo = new FinanceAccountExtDPViewModel();
                    // Advance payment
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.Direct = reader.GetBoolean(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.StartDate = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.EndDate = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.RptType = reader.GetByte(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.RefDocID = reader.GetInt32(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.DefrrDays = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.AdvancePaymentInfo.Comment = reader.GetString(idx++);
                    else
                        ++idx;
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
        internal static string getFinanceAccountADPInsertString()
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

        internal static void bindFinAccountADPParameter(SqlCommand cmd, FinanceAccountExtDPViewModel vm, Int32 nNewDocID, Int32 nNewAccountID, String usrName)
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

        #region Finance document List
        internal static string getFinanceDocListQueryString(Int32 top, Int32 skip)
        {
            String strSQL = @"SELECT count(*) FROM [dbo].[v_fin_document];";
            strSQL += @" SELECT [v_fin_document].[ID]
                                ,[v_fin_document].[DOCTYPE]
	                            ,[v_fin_document].[DOCTYPENAME]
                                ,[v_fin_document].[TRANDATE]
                                ,[v_fin_document].[TRANCURR]
                                ,[v_fin_document].[DESP]
                                ,[v_fin_document].[EXGRATE]
                                ,[v_fin_document].[EXGRATE_PLAN]
                                ,[v_fin_document].[TRANCURR2]
                                ,[v_fin_document].[EXGRATE2]
                                ,[v_fin_document].[EXGRATE_PLAN2]
                                ,[v_fin_document].[CREATEDBY]
                                ,[v_fin_document].[CREATEDAT]
                                ,[v_fin_document].[UPDATEDBY]
                                ,[v_fin_document].[UPDATEDAT]
                                ,[v_fin_document].[TRANAMOUNT]
                            FROM [dbo].[v_fin_document]
                            ORDER BY [TRANDATE] DESC";

            return strSQL;
        }

        internal static void FinDocList_DB2VM(SqlDataReader reader, FinanceDocumentUIViewModel vm)
        {
            Int32 idx = 0;
            try
            {
                vm.ID = reader.GetInt32(idx++);
                vm.DocType = reader.GetInt16(idx++);
                vm.DocTypeName = reader.GetString(idx++);
                vm.TranDate = reader.GetDateTime(idx++);
                vm.TranCurr = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Desp = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.TranCurr2 = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate2 = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan2 = reader.GetByte(idx++);
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

        internal static string getFinanceDocADPQueryString(Int32 nid)
        {
            String strSQL = @"SELECT [ID]
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
                        FROM [v_fin_document_item1] WHERE [DOCID] = " + nid.ToString() + @"; SELECT [t_fin_account].[ID]
                            ,[t_fin_account].[CTGYID]
                            ,[t_fin_account_ctgy].[NAME] as [CTGYNAME]
                            ,[t_fin_account].[NAME]
                            ,[t_fin_account].[COMMENT]
                            ,[t_fin_account].[OWNER]
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
                        LEFT OUTER JOIN [dbo].[t_fin_account_ctgy]
                            ON [t_fin_account].[CTGYID] = [t_fin_account_ctgy].[ID]
                        LEFT OUTER JOIN [dbo].[t_fin_account_ext_dp]
                            ON [t_fin_account].[ID] = [t_fin_account_ext_dp].[ACCOUNTID]
                        WHERE [t_fin_account].[CTGYID] = "
                        + FinanceAccountCtgyViewModel.AccountCategory_AdvancePayment.ToString()
                        + " AND [t_fin_account_ext_dp].[REFDOCID] = " + nid.ToString() + @"; SELECT  [dbo].[t_fin_tmpdoc_dp].[DOCID]
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
        #endregion

        #region Finance document Header

        internal static string getFinDocHeaderInsertString()
        {
            return @"INSERT INTO [dbo].[t_fin_document]
                                           ([DOCTYPE]
                                           ,[TRANDATE]
                                           ,[TRANCURR]
                                           ,[DESP]
                                           ,[EXGRATE]
                                           ,[EXGRATE_PLAN]
                                           ,[TRANCURR2]
                                           ,[EXGRATE2]
                                           ,[CREATEDBY]
                                           ,[CREATEDAT]
                                           ,[UPDATEDBY]
                                           ,[UPDATEDAT])
                                     VALUES
                                           (@DOCTYPE
                                           ,@TRANDATE
                                           ,@TRANCURR
                                           ,@DESP
                                           ,@EXGRATE
                                           ,@EXGRATE_PLAN
                                           ,@TRANCURR2
                                           ,@EXGRATE2
                                           ,@CREATEDBY
                                           ,@CREATEDAT
                                           ,@UPDATEDBY
                                           ,@UPDATEDAT); SELECT @Identity = SCOPE_IDENTITY();";
        }

        internal static void bindFinDocHeaderParameter(SqlCommand cmd, FinanceDocumentViewModel vm, String usrName)
        {
            cmd.Parameters.AddWithValue("@DOCTYPE", vm.DocType);
            cmd.Parameters.AddWithValue("@TRANDATE", vm.TranDate);
            cmd.Parameters.AddWithValue("@TRANCURR", vm.TranCurr);
            cmd.Parameters.AddWithValue("@DESP", vm.Desp);
            if (vm.ExgRate > 0)
                cmd.Parameters.AddWithValue("@EXGRATE", vm.ExgRate);
            else
                cmd.Parameters.AddWithValue("@EXGRATE", DBNull.Value);
            cmd.Parameters.AddWithValue("@EXGRATE_PLAN", vm.ExgRate_Plan);
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
                vm.DocType = reader.GetInt16(idx++);
                vm.TranDate = reader.GetDateTime(idx++);
                vm.TranCurr = reader.GetString(idx++);
                if (!reader.IsDBNull(idx))
                    vm.Desp = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.TranCurr2 = reader.GetString(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate2 = reader.GetByte(idx++);
                else
                    ++idx;
                if (!reader.IsDBNull(idx))
                    vm.ExgRate_Plan2 = reader.GetByte(idx++);
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
                                ([REFDOCID]
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
                                (@REFDOCID
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
                      WHERE [CONTROLCENTERID] = " + nOrderID.ToString() +
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
                    vm = new FinanceDocumentUIViewModel();

                    vm.ID = nCurrentID;
                    vm.DocType = reader.GetInt16(idx++);
                    vm.DocTypeName = reader.GetString(idx++);
                    vm.TranDate = reader.GetDateTime(idx++);
                    vm.TranCurr = reader.GetString(idx++);
                    if (!reader.IsDBNull(idx))
                        vm.Desp = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate = reader.GetByte(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate_Plan = reader.GetByte(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.TranCurr2 = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        vm.ExgRate2 = reader.GetByte(idx++);
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
