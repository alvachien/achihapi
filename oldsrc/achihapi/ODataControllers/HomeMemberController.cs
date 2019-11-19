using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using achihapi.Controllers;
using achihapi.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.ODataControllers
{
    public class HomeMemberController : ODataController
    {
        private readonly achihdbContext _context;

        public HomeMemberController(achihdbContext context, IMemoryCache cache)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<THomemem> Get()
        {
            //// If you have any security filters you should apply them before returning then from this method.
            return _context.THomemem;
        }

        [EnableQuery]
        public SingleResult<THomemem> Get([FromODataUri] int hid)
        {
            // return BadRequest();
            return SingleResult.Create(_context.THomemem.Where(p => p.Hid == hid));
        }
    }
}
