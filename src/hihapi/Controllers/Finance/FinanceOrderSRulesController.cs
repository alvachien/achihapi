using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;

namespace hihapi.Controllers
{
    [Authorize]
    public sealed class FinanceOrderSRulesController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceOrderSRulesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceOrderSRules
        [EnableQuery]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinanceDocumentItem> option)
        public IActionResult Get()
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

            return Ok(from hmem in _context.HomeMembers
                where hmem.User == usrName
                select new { HomeID = hmem.HomeID } into hids
                join orders in _context.FinanceOrder on hids.HomeID equals orders.HomeID
                select new { HomeID = orders.HomeID, ID = orders.ID } into orderids
                join srules in _context.FinanceOrderSRule on orderids.ID equals srules.OrderID
                select srules);

            //return Ok(option.ApplyTo(rst));
        }
    }
}
