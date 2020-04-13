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
using hihapi.Utilities;

namespace hihapi.Controllers
{
    public class BlogPostTagsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostTagsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<BlogPostTag> Get()
        {
            return _context.BlogPostTags;
        }

        //[EnableQuery]
        //public SingleResult<BlogPostTag> Get([FromODataUri] int id)
        //{
        //    return SingleResult.Create(_context.BlogPostTags.Where(p => p.ID == id));
        //}
    }
}
