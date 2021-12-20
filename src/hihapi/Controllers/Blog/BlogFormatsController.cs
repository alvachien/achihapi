using System.Linq;
using Microsoft.AspNetCore.Mvc;
using hihapi.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
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
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.BlogFormats);
        }

        [EnableQuery]
        [HttpGet]
        public BlogFormat Get([FromODataUri] int key)
        {
            return _context.BlogFormats.Where(p => p.ID == key).SingleOrDefault();
        }
    }
}
