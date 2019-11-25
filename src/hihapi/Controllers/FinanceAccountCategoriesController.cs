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
    public class FinanceAccountCategoriesController: ODataController
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
        public IQueryable<FinanceAccountCategory> Get()
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

            var rst = from acntctgy in _context.FinAccountCategories
                join hmem in _context.HomeMembers
                on new {    
                    acntctgy.HID == null || acntctgy.HID
                    key2: acntctgy.HID                    
                } equals new {
                    key1: true,
                    key2: 
                }
                select acntctgy;

            return _context.FinAccountCategories;
        }
    }
}
