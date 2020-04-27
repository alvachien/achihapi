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
using Microsoft.AspNetCore.Authorization;
using System;
using hihapi.Exceptions;

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

        [Authorize]
        public async Task<IActionResult> Post([FromBody]BlogPost post)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // User
            string usrName;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
                if (String.CompareOrdinal(usrName, post.Owner) != 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            post.CreatedAt = DateTime.Now;
            post.Owner = usrName;

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            return Created(post);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody]BlogPost update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("ID mismatched");
            }

            // User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
                if (String.CompareOrdinal(usrName, update.Owner) != 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            update.UpdatedAt = DateTime.Now;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinanceAccount.Any(p => p.ID == key))
                {
                    return NotFound();
                }
                else
                {
                    throw new DBOperationException(exp.Message);
                }
            }

            return Updated(update);
        }

        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.BlogPosts.FindAsync(key);
            if (cc == null)
            {
                return NotFound();
            }

            // User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
                if (String.CompareOrdinal(cc.Owner, usrName) == 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            //if (!cc.IsDeleteAllowed(this._context))
            //    return BadRequest();

            _context.BlogPosts.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}

