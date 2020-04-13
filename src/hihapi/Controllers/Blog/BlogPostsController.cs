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
    public class BlogPostsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<BlogPost> Get()
        {
            return _context.BlogPosts;
        }

        [EnableQuery]
        public SingleResult<BlogPost> Get([FromODataUri] int id)
        {
            return SingleResult.Create(_context.BlogPosts.Where(p => p.ID == id));
        }
    }
}

