using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
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

        /// GET: /FinanceOrderSRules
        [Authorize]
        public IQueryable Get(ODataQueryOptions<FinanceDocumentItem> option)
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
                from hmem in _context.HomeMembers
                where hmem.User == usrName
                select new { HomeID = hmem.HomeID } into hids
                join orders in _context.FinanceOrder on hids.HomeID equals orders.HomeID
                select new { HomeID = orders.HomeID, ID = orders.ID } into orderids
                join srules in _context.FinanceOrderSRule on orderids.ID equals srules.OrderID
                select srules;

            return option.ApplyTo(rst);
        }
    }
}
