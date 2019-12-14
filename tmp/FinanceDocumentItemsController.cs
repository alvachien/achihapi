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
    public class FinanceDocumentItemsController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentItemsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceDocumentItems
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceDocumentItem> Get(Int32 hid, Int32 docid)
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
                from doc in _context.FinanceDocument.Where(p => p.ID == docid && p.HomeID == hmem.HomeID)
                from items in _context.FinanceDocumentItem.Where(p => p.DocID == doc.ID)
                select items;

            return rst;
        }
    }
}
