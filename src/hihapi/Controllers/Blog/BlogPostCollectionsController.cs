using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace hihapi.Controllers
{
    public class BlogPostCollectionsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostCollectionsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.BlogPostCollections);
        }
    }
}
