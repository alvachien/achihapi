using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;

namespace hihapi.Controllers
{
    public class FinanceAccountCategoriesController : ODataController
    {
        public const Int32 AccountCategory_AdvancePayment = 8;
        public const Int32 AccountCategory_Asset = 7;
        public const Int32 AccountCategory_BorrowFrom = 9;
        public const Int32 AccountCategory_LendTo = 10;
        public const Int32 AccountCategory_AdvanceReceive = 11;
        public const Int32 AccountCategory_Insurance = 12;
        private readonly hihDataContext _context;

        public FinanceAccountCategoriesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceAccountCategories
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceAccountCategory> Get(Int32? hid = null)
        {
            if (hid.HasValue)
            {
                String usrName = String.Empty;
                try
                {
                    usrName = HIHAPIUtility.GetUserID(this);
                }
                catch
                {
                    // Do nothing
                }

                if (String.IsNullOrEmpty(usrName))
                    return _context.FinAccountCategories.Where(p => p.HID == null);

                var rst =
                    from hmem in _context.HomeMembers.Where(p => p.User == usrName)
                    from acntctgy in _context.FinAccountCategories.Where(p => p.HID == null || p.HID == hmem.HomeID)
                    select acntctgy;

                return rst;
            }
            
            return _context.FinAccountCategories.Where(p => p.HID == null);
        }
    }
}
