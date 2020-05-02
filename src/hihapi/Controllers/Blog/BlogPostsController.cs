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
using Microsoft.Extensions.Logging;
using Serilog;

namespace hihapi.Controllers
{
    public class BlogPostsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostsController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        [EnableQuery]
        public IQueryable<BlogPost> Get()
        {
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

            return _context.BlogPosts.Where(p => p.Owner == usrName);
        }

        [Authorize]
        [EnableQuery]
        public SingleResult<BlogPost> Get([FromODataUri] int id)
        {
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

            return SingleResult.Create(_context.BlogPosts.Where(p => p.ID == id && p.Owner == usrName));
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

            // Check setting
            var setting = _context.BlogUserSettings.SingleOrDefault(p => p.Owner == usrName);
            if (setting == null)
            {
                throw new BadRequestException(" User has no setting ");
            }

            post.CreatedAt = DateTime.Now;
            post.Owner = usrName;

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            if (post.Status == BlogPost.BlogPostStatus_PublishAsPublic)
            {
                try
                {
                    BlogDeliverUtility.DeliverPost(setting.DeployFolder, post, _context.BlogCollections.Where(p => p.Owner == usrName).ToList());
                }
                catch(Exception exp)
                {
                    //_logger.LogError("Post Deliver creation failed: " + exp.Message);
                    Log.Error("Post Deliver creation failed: " + exp.Message);
                }
            }

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

            // Check setting
            var setting = _context.BlogUserSettings.SingleOrDefault(p => p.Owner == usrName);
            if (setting == null)
            {
                throw new BadRequestException(" User has no setting ");
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

            try
            {
                if (update.Status == BlogPost.BlogPostStatus_PublishAsPublic)
                {
                    BlogDeliverUtility.DeliverPost(setting.DeployFolder, update, _context.BlogCollections.Where(p => p.Owner == usrName).ToList());
                }
                else
                {
                    BlogDeliverUtility.RevokePostDeliver(setting.DeployFolder, update.ID);
                }
            }
            catch (Exception exp)
            {
                //_logger.LogError("Post Deliver updated: " + exp.Message);
                Log.Error("Post Deliver updated: " + exp.Message);
            }

            return Ok(update);
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

            // Check setting
            var setting = _context.BlogUserSettings.SingleOrDefault(p => p.Owner == usrName);
            if (setting == null)
            {
                throw new BadRequestException(" User has no setting ");
            }

            //if (!cc.IsDeleteAllowed(this._context))
            //    return BadRequest();

            _context.BlogPosts.Remove(cc);
            await _context.SaveChangesAsync();

            try
            {
                BlogDeliverUtility.RevokePostDeliver(setting.DeployFolder, cc.ID);
            }
            catch(Exception exp)
            {
                //_logger.LogError("Post Deliver deletion failed: " + exp.Message);
                Log.Error("Post Deliver deletion failed: " + exp.Message);
            }

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}

