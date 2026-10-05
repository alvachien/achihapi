using System;
using System.Linq;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

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

        [EnableQuery(MaxNodeCount = 200)]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinanceDocumentItemView> option)
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

            // Restricted ("lite") members see only the items on the accounts they own.
            return Ok(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { hmem.HomeID, hmem.User, hmem.IsLite } into hmems
                      join items in _context.FinanceDocumentItemView on hmems.HomeID equals items.HomeID
                      where !hmems.IsLite.HasValue
                          || hmems.IsLite == false
                          || (from acnt in _context.FinanceAccount
                              where acnt.ID == items.AccountID && acnt.Owner == hmems.User
                              select acnt.ID).Any()
                      select items);

            //return Ok(option.ApplyTo(rst));
        }
    }
}
