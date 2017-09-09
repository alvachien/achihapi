using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public abstract class BaseViewModel
    {
        [StringLength(40)]
        public String CreatedBy { get; set;  }
        public DateTime CreatedAt { get; set; }
        [StringLength(40)]
        public String UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BaseListViewModel<T> where T : BaseViewModel
    {
        // Runtime information
        public Int32 TotalCount { get; set; }
        public List<T> ContentList = new List<T>();

        public void Add(T tObj)
        {
            this.ContentList.Add(tObj);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return this.ContentList.GetEnumerator();
        }
    }

    internal static class HIHAPIUtility
    {
        internal static System.Security.Claims.Claim GetUserClaim(Microsoft.AspNetCore.Mvc.ControllerBase ctrl)
        {
            var usrObj = ctrl.User.FindFirst(c => c.Type == "sub");
            if (usrObj == null)
                throw new Exception();

            return usrObj;
        }

        internal static System.Security.Claims.Claim GetScopeClaim(Microsoft.AspNetCore.Mvc.ControllerBase ctrl, String strScope)
        {
            var scopeObj = ctrl.User.FindFirst(c => c.Type == strScope);
            if (scopeObj == null)
                throw new Exception();

            return scopeObj;
        }

        internal static String GetScopeSQLFilter(String scopeStr, String usrStr)
        {
            if (String.CompareOrdinal(scopeStr, HIHAPIConstants.All) == 0)
            {
                scopeStr = String.Empty;
            }
            else if (String.CompareOrdinal(scopeStr, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
            {
                scopeStr = usrStr;
            }
            else if (String.CompareOrdinal(scopeStr, HIHAPIConstants.OnlyOwnerFullControl) == 0)
            {
                scopeStr = usrStr;
            }

            return scopeStr;
        }
    }

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

    internal enum HIHHomeMemberRelationship
    {
        Self = 0,
        Couple = 1,
        Child = 2,
        Parent = 3
    }
}
