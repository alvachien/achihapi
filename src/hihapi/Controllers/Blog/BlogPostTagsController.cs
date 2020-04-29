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
using System;

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
            // User
            string usrName;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var tags = from post in _context.BlogPosts
                       where post.Owner == usrName
                       select new { PostID = post.ID } into postids
                       join posttags in _context.BlogPostTags
                        on postids.PostID equals posttags.PostID
                       select posttags;

            return tags;
        }

        //[EnableQuery]
        //public SingleResult<BlogPostTag> Get([FromODataUri] int id)
        //{
        //    return SingleResult.Create(_context.BlogPostTags.Where(p => p.ID == id));
        //}
    }
}
