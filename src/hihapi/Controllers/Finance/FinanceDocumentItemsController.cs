using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class FinanceDocumentItemsController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentItemsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceDocumentItems
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
                select new { hmem.HomeID } into hids
                join docs in _context.FinanceDocument on hids.HomeID equals docs.HomeID
                select new { docs.HomeID, docs.ID } into docids
                join items in _context.FinanceDocumentItem on docids.ID equals items.DocID
                select items;

            return option.ApplyTo(rst);
        }
    }
}
