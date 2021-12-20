using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace hihapi.Controllers
{
    [Authorize]
    public class FinanceDocumentItemViewsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentItemViewsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get(ODataQueryOptions<FinanceDocumentItemView> option)
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
                select new { hmem.HomeID } into hids
                join items in _context.FinanceDocumentItemView on hids.HomeID equals items.HomeID
                select items;

            return Ok(option.ApplyTo(rst));
        }
    }
}