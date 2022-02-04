using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using hihapi.Utilities;

namespace hihapi.Controllers
{
    [Authorize]
    public class HomeMembersController : ODataController
    {
        private readonly hihDataContext _context;

        public HomeMembersController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /HomeMembers
        /// <summary>
        /// Adds support for getting home member, for example:
        /// 
        /// GET /HomeMembers
        /// GET /HomeMembers?$filter=Host eq 'abc'
        /// GET /HomeMembers?
        /// 
        /// <remarks>
        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            String usrName = "";
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);

                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            return Ok(_context.HomeMembers);
        }
    }
}
