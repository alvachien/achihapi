using System;
using System.Linq;
using System.Security.Claims;
using hihapi.Models;

namespace hihapi.Utilities
{
    internal class HIHAPIConstants
    {
        public const String OnlyOwnerAndDispaly = "OnlyOwnerAndDisplay";
        public const String OnlyOwnerFullControl = "OnlyOwnerFullControl";
        public const String OnlyOwner = "OnlyOwner";
        public const String Display = "Display";
        public const String All = "All";

        internal const String HomeDefScope = "HomeDefScope";
        internal const String FinanceAccountScope = "FinanceAccountScope";
        internal const String FinanceDocumentScope = "FinanceDocumentScope";
        internal const String LearnHistoryScope = "LearnHistoryScope";
        internal const String LearnObjectScope = "LearnObjectScope";

        internal const String DateFormatPattern = "yyyy-MM-dd";
    }

    internal static class HIHAPIUtility
    {
        internal static String GetUserID(Microsoft.AspNetCore.Mvc.ControllerBase ctrl)
        {
            return ctrl.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

/*
        internal static void CheckHIDAssignment(hihDataContext context, Int32 hid, String usrName)
        {
            var ncnt = context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).Count();
            if (ncnt <= 0)
                throw new Exception("No Home Definition found");
        }
        */
        // internal static System.Security.Claims.Claim GetUserClaim(Microsoft.AspNetCore.Mvc.ControllerBase ctrl)
        // {
        //     var usrObj = ctrl.User.FindFirst(c => c.Type == "sub");
        //     if (usrObj == null)
        //         throw new Exception();

        //     return usrObj;
        // }

        // internal static System.Security.Claims.Claim GetScopeClaim(Microsoft.AspNetCore.Mvc.ControllerBase ctrl, String strScope)
        // {
        //     var scopeObj = ctrl.User.FindFirst(c => c.Type == strScope);
        //     if (scopeObj == null)
        //         throw new Exception();

        //     return scopeObj;
        // }

        // internal static String GetScopeSQLFilter(String scopeStr, String usrStr)
        // {
        //     if (String.CompareOrdinal(scopeStr, HIHAPIConstants.All) == 0)
        //     {
        //         scopeStr = String.Empty;
        //     }
        //     else if (String.CompareOrdinal(scopeStr, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
        //     {
        //         scopeStr = usrStr;
        //     }
        //     else if (String.CompareOrdinal(scopeStr, HIHAPIConstants.OnlyOwnerFullControl) == 0)
        //     {
        //         scopeStr = usrStr;
        //     }

        //     return scopeStr;
        // }

//         internal static void CheckHIDAssignment(SqlConnection conn, Int32 hid, String usrName)
//         {
//             if (hid == 0 || conn == null || String.IsNullOrEmpty(usrName))
//                 throw new Exception("Inputted parameter invalid");

//             String strHIDCheck = @"SELECT TOP (1) [HID] FROM [dbo].[t_homemem] WHERE [HID]= @hid AND [USER] = @user";
//             SqlCommand cmdHIDCheck = null;
//             SqlDataReader readHIDCheck = null;

//             try
//             {
//                 cmdHIDCheck = new SqlCommand(strHIDCheck, conn);                
//                 cmdHIDCheck.Parameters.AddWithValue("@hid", hid);
//                 cmdHIDCheck.Parameters.AddWithValue("@user", usrName);
//                 readHIDCheck = cmdHIDCheck.ExecuteReader();
//                 if (!readHIDCheck.HasRows)
//                     throw new Exception("No Home Definition found");
//             }
//             catch (Exception exp)
//             {
// #if DEBUG
//                  System.Diagnostics.Debug.WriteLine(exp.Message);
// #endif
//                 // Re-throw the exception
//                 throw exp;
//             }
//             finally
//             {
//                 if (readHIDCheck != null)
//                 {
//                     readHIDCheck.Dispose();
//                     readHIDCheck = null;
//                 }

//                 if (cmdHIDCheck != null)
//                 {
//                     cmdHIDCheck.Dispose();
//                     cmdHIDCheck = null;
//                 }
//             }
//         }
    }
}