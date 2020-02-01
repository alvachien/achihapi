using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNet.OData.Query;

namespace hihapi.Controllers
{
    public class FinanceDocumentItemViewController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceDocumentItemViewController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        public IQueryable Get(ODataQueryOptions<FinanceDocumentItemView> option)
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

            return option.ApplyTo(rst);
        }
    }
}