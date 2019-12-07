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
using hihapi.Exceptions;

namespace hihapi.Controllers
{
    public sealed class FinanceOrderSRulesController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceOrderSRulesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceOrders
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceOrderSRule> Get(Int32 hid, Int32 orderid)
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var rst =
                from hmem in _context.HomeMembers.Where(p => p.User == usrName && p.HomeID == hid)
                from ord in _context.FinanceOrder.Where(p => p.ID == orderid && p.HomeID == hmem.HomeID)
                from items in _context.FinanceOrderSRule.Where(p => p.OrderID == ord.ID)
                select items;

            // return rst;
            return rst;
        }
    }
}
