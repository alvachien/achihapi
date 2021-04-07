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
using hihapi.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class BlogFormatsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogFormatsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<BlogFormat> Get()
        {
            return _context.BlogFormats;
        }

        [EnableQuery]
        public SingleResult<BlogFormat> Get([FromODataUri] int id)
        {
            return SingleResult.Create(_context.BlogFormats.Where(p => p.ID == id));
        }
    }
}
