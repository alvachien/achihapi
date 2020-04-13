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
    public class BlogCollectionsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogCollectionsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<BlogCollection> Get()
        {
            return _context.BlogCollections;
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
        public async Task<IActionResult> Post([FromBody] BlogCollection coll)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            _context.BlogCollections.Add(coll);
            await _context.SaveChangesAsync();

            return Created(coll);
        }

        // PUT: /BlogCollections/5
        /// <summary>
        /// Support for updating BlogCollections
        /// </summary>
        public async Task<IActionResult> Put([FromODataUri] int id, [FromBody] BlogCollection update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var currency = await _context.BlogCollections.FindAsync(key);
            if (currency == null)
            {
                return NotFound();
            }

            _context.BlogCollections.Remove(currency);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        // PATCH: /BlogCollections
        /// <summary>
        /// Support for partial updates of BlogCollections
        /// </summary>
        public async Task<IActionResult> Patch([FromODataUri] int id, [FromBody] Delta<BlogCollection> coll)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _context.BlogCollections.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
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
