using System;
using System.Linq;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace hihapi.Controllers
{
    [Authorize]
    public class BlogPostTagsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostTagsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
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

            return Ok(from post in _context.BlogPosts
                      where post.Owner == usrName
                      select new { PostID = post.ID } into postids
                      join posttags in _context.BlogPostTags
                       on postids.PostID equals posttags.PostID
                      select posttags);
        }
    }
}
