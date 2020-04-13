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
    public class BlogPostCollectionsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostCollectionsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<BlogPostCollection> Get()
        {
            return _context.BlogPostCollections;
        }
    }
}
