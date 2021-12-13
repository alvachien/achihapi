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
using hihapi.Models;
using Microsoft.AspNetCore.Authorization;

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
        public IQueryable<HomeMember> Get()
        {
            return _context.HomeMembers;
        }
    }
}
