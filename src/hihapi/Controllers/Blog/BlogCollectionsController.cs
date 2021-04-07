using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Deltas;

namespace hihapi.Controllers
{
    public class BlogCollectionsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogCollectionsController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        [EnableQuery]
        public IQueryable<BlogCollection> Get()
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

            return _context.BlogCollections.Where(p => p.Owner == usrName);
        }

        //[EnableQuery]
        //public SingleResult<BlogCollection> Get([FromODataUri] int id)
        //{
        //    return SingleResult.Create(_context.BlogCollections.Where(p => p.ID == id));
        //}

        // POST: /BlogCollections
        /// <summary>
        /// Support for creating BlogCollections
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Post([FromBody] BlogCollection coll)
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
                if (coll.Owner != null)
                {
                    if (String.CompareOrdinal(coll.Owner, usrName) != 0)
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                else
                {
                    coll.Owner = usrName;
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

            _context.BlogCollections.Add(coll);
            await _context.SaveChangesAsync();

            return Created(coll);
        }

        // PUT: /BlogCollections/5
        /// <summary>
        /// Support for updating BlogCollections
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int id, [FromBody] BlogCollection update)
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
                if (String.CompareOrdinal(update.Owner, usrName) != 0)
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

            // Check ID
            if (id != update.ID)
            {
                return BadRequest();
            }

            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.BlogCollections.Any(p => p.ID == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(update);
        }

        // DELETE: /BlogCollections/5
        /// <summary>
        /// Support for deleting BlogCollections by key.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var record = await _context.BlogCollections.FindAsync(key);
            if (record == null)
            {
                return NotFound();
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
                if (String.CompareOrdinal(record.Owner, usrName) != 0)
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

            _context.BlogCollections.Remove(record);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        // PATCH: /BlogCollections
        /// <summary>
        /// Support for partial updates of BlogCollections
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Patch([FromODataUri] int id, [FromBody] Delta<BlogCollection> coll)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState); 
            }

            var entity = await _context.BlogCollections.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
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
                if (String.CompareOrdinal(entity.Owner, usrName) != 0)
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

            coll.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.BlogCollections.Any(p => p.ID == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(entity);
        }
    }
}
