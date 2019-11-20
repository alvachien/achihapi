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
using hihapi.Models;

namespace hihapi.Controllers
{
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
        public IQueryable<HomeDefine> Get()
        {
            return _context.HomeDefines;
        }
    }
}
