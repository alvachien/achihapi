﻿using System;
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

        internal static String GetScopeForCurrentUser(Microsoft.AspNetCore.Mvc.ControllerBase ctrl, System.Security.Claims.Claim usrObj, String strScope)
        {
            var scopeStr = ctrl.User.FindFirst(c => c.Type == strScope).Value;
            if (String.CompareOrdinal(scopeStr, HIHAPIConstants.All) == 0)
            {
                scopeStr = String.Empty;
            }
            else if (String.CompareOrdinal(scopeStr, HIHAPIConstants.OnlyOwnerAndDispaly) == 0)
            {
                scopeStr = usrObj.Value;
            }
            else if (String.CompareOrdinal(scopeStr, HIHAPIConstants.OnlyOwnerFullControl) == 0)
            {
                scopeStr = usrObj.Value;
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

        internal const String FinanceAccountScope = "FinanceAccountScope";
    }
}
