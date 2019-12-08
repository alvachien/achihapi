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
    public class FinanceDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceDocuments
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceDocument> Get(Int32 hid)
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
                from acnt in _context.FinanceDocument.Where(p => p.HomeID == hmem.HomeID)
                select acnt;

            return rst;
        }
    }
}
