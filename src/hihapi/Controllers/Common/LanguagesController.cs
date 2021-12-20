using System.Linq;
using Microsoft.AspNetCore.Mvc;
using hihapi.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class LanguagesController : ODataController
    {
        private readonly hihDataContext _context;

        public LanguagesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /Languages
        /// <summary>
        /// Adds support for getting languages, for example:
        /// 
        /// GET /Languages
        /// GET /Languages?$filter=NativeName eq 'English'
        /// GET /Languages?
        /// 
        /// <remarks>
        [EnableQuery]
        [ResponseCache(Duration = 86400)]
        public IActionResult Get(ODataQueryOptions<Language> option)
        {
            return Ok(option.ApplyTo(_context.Languages));
        }

        [EnableQuery]
        [HttpGet]
        public Language Get([FromODataUri] int key)
        {
            return _context.Languages.Where(p => p.Lcid == key).SingleOrDefault();
        }
    }
}
