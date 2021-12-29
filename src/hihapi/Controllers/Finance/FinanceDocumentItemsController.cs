using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace hihapi.Controllers
{
    [Authorize]
    public class FinanceDocumentItemsController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentItemsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceDocumentItems
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
                select new { hmem.HomeID } into hids
                join docs in _context.FinanceDocument on hids.HomeID equals docs.HomeID
                select new { docs.HomeID, docs.ID } into docids
                join items in _context.FinanceDocumentItem on docids.ID equals items.DocID
                select items);

            //return Ok(option.ApplyTo(rst));
        }
    }
}
