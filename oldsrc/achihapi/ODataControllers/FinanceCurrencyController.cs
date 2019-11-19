using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.Controllers;
using achihapi.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace achihapi.ODataControllers
{
    public class FinanceCurrencyController : ODataController
    {
        private readonly achihdbContext _context;
        private IMemoryCache _cache;

        public FinanceCurrencyController(achihdbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [EnableQuery]
        public IQueryable<TFinCurrency> Get()
        {
            DbSet<TFinCurrency> listRst;
            if (_cache.TryGetValue<DbSet<TFinCurrency>>(CacheKeys.FinCurrency, out listRst))
            {
                // DO nothing!
            }
            else
            {
                listRst = _context.TFinCurrency;
                _cache.Set<DbSet<TFinCurrency>>(CacheKeys.FinCurrency, listRst, TimeSpan.FromSeconds(1200));                
            }
            return listRst;

            //// If you have any security filters you should apply them before returning then from this method.
            //return _context.TFinCurrency;
        }

        [EnableQuery]
        public SingleResult<TFinCurrency> Get([FromODataUri] string key)
        {
            // return BadRequest();
            return SingleResult.Create(_context.TFinCurrency.Where(p => p.Curr == key));
        }

        // POST
        public async Task<IActionResult> Post([FromBody] TFinCurrency fincurr)
        {
            return BadRequest();
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //_context.TFinCurrency.Add(fincurr);
            //await _context.SaveChangesAsync();

            //return Created(fincurr);
        }

        // PUT
        public async Task<IActionResult> Put([FromODataUri] string key, [FromBody] TFinCurrency update)
        {
            return BadRequest();
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //if (key != update.Curr)
            //{
            //    return BadRequest();
            //}

            //_context.Entry(update).State = EntityState.Modified;
            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!_context.TFinCurrency.Any(p => p.Curr == key))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //return Updated(update);
        }

        // PATCH
        public async Task<IActionResult> Patch([FromODataUri] string key, [FromBody] Delta<TFinCurrency> fincurr)
        {
            return BadRequest();
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //var entity = await _context.TFinCurrency.FindAsync(key);
            //if (entity == null)
            //{
            //    return NotFound();
            //}

            //fincurr.Patch(entity);

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!_context.TFinCurrency.Any(p => p.Curr == key))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //return Updated(entity);
        }

        // DELETE
        public async Task<IActionResult> Delete([FromODataUri] string key)
        {
            return BadRequest();
            //var knowledge = await _context.TFinCurrency.FindAsync(key);
            //if (knowledge == null)
            //{
            //    return NotFound();
            //}

            //_context.TFinCurrency.Remove(knowledge);
            //await _context.SaveChangesAsync();

            //return StatusCode(204); // HttpStatusCode.NoContent
        }

        private bool CurrencyExists(string id)
        {
            return _context.TFinCurrency.Any(e => e.Curr == id);
        }
    }
}
