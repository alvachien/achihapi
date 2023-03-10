using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using System;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    [Authorize]
    public class BlogPostsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogPostsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
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

            return Ok(_context.BlogPosts.Where(p => p.Owner == usrName));
        }

        [EnableQuery]
        [HttpGet]
        public BlogPost Get([FromODataUri] int key)
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

            return _context.BlogPosts.Where(p => p.ID == key && p.Owner == usrName).SingleOrDefault();
        }

        [HttpPost]
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

            return Created(post);
        }

        [HttpPut]
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

            // Tags
            var tagInDBs = _context.BlogPostTags.Where(p => p.PostID == update.ID).ToList();
            foreach (var tag in update.BlogPostTags)
            {
                var tagindb = tagInDBs.Find(p => p.PostID == update.ID && p.Tag == tag.Tag);
                if (tagindb == null)
                {
                    _context.BlogPostTags.Add(tag);
                }
            }
            foreach (var tag in tagInDBs)
            {
                var ntag = update.BlogPostTags.FirstOrDefault(p => p.PostID == update.ID && p.Tag == tag.Tag);
                if (ntag == null)
                {
                    _context.BlogPostTags.Remove(tag);
                }
            }

            // Collection
            var collInDBs = _context.BlogPostCollections.Where(p => p.PostID == update.ID).ToList();
            foreach (var coll in update.BlogPostCollections)
            {
                var collindb = collInDBs.Find(p => p.PostID == update.ID && p.CollectionID == coll.CollectionID);
                if (collindb == null)
                {
                    _context.BlogPostCollections.Add(coll);
                }
            }
            foreach (var coll in collInDBs)
            {
                var ncoll = update.BlogPostCollections.FirstOrDefault(p => p.PostID == update.ID && p.CollectionID == coll.CollectionID);
                if (ncoll == null)
                {
                    _context.BlogPostCollections.Remove(coll);
                }
            }

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

            return Ok(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.BlogPosts.FindAsync(key);
            if (cc == null)
            {
                throw new NotFoundException("HIHAPI: Record not found");
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
                if (String.CompareOrdinal(cc.Owner, usrName) != 0)
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

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        [HttpGet]
        public async Task<IActionResult> Deploy(int key)
        {
            var cc = _context.BlogPosts.Find(key);
            if (cc == null)
            {
                throw new NotFoundException("HIHAPI: Record not found");
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
                if (String.CompareOrdinal(cc.Owner, usrName) != 0)
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

            // Collections
            cc.BlogPostCollections = _context.BlogPostCollections.Where(p => p.PostID == cc.ID).ToList();
            // Tags
            cc.BlogPostTags = _context.BlogPostTags.Where(p => p.PostID == cc.ID).ToList();

            var errstr = "";
            try
            {
                if (cc.Status == BlogPost.BlogPostStatus_PublishAsPublic)
                {
                    await BlogDeployUtility.DeployPost(setting.DeployFolder, cc, _context.BlogCollections.Where(p => p.Owner == usrName).ToList());
                }
                else
                {
                    BlogDeployUtility.RevokePostDeliver(setting.DeployFolder, cc.ID);
                }
            }
            catch (Exception exp)
            {
                errstr = exp.Message;
            }

            if (!string.IsNullOrEmpty(errstr))
            {
                throw new Exception(errstr);
            }

            return Ok("done");
        }

        [HttpGet]
        public IActionResult ClearDeploy(int key)
        {
            // User
            String usrName = String.Empty;
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

            // Check setting
            var setting = _context.BlogUserSettings.SingleOrDefault(p => p.Owner == usrName);
            if (setting == null)
            {
                throw new BadRequestException(" User has no setting ");
            }

            var errstr = "";
            try
            {
                BlogDeployUtility.RevokePostDeliver(setting.DeployFolder, key);
            }
            catch (Exception exp)
            {
                errstr = exp.Message;
            }

            // Return
            if (!string.IsNullOrEmpty(errstr))
            {
                throw new Exception(errstr);
            }

            return Ok("");
        }
    }
}

