using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using hihapi.Utilities;
using hihapi.Models;

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
            var usrName = HIHAPIUtility.GetAuthenticatedUserName(this);
            return Ok(from mem in _context.HomeMembers where mem.User == usrName select mem);
        }
    }
}
