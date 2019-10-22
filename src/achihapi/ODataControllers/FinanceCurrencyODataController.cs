using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.Context;
using achihapi.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace achihapi.ODataControllers
{
    public class FinanceCurrencyODataController : ODataController
    {
        private readonly achihdbContext _context;

        public FinanceCurrencyODataController(achihdbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<FinanceCurrencyModel> Get()
        {
            // If you have any security filters you should apply them before returning then from this method.
            return _context.Financecurrency;
        }

        [EnableQuery]
        public SingleResult<FinanceCurrencyModel> Get([FromODataUri] string key)
        {
            return SingleResult.Create(_context.Financecurrency.Where(p => p.Curr == key));
        }

        // POST
        public async Task<IActionResult> Post([FromBody] FinanceCurrencyModel fincurr)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Financecurrency.Add(fincurr);
            await _context.SaveChangesAsync();

            return Created(fincurr);
        }

        // PUT
        public async Task<IActionResult> Put([FromODataUri] string key, [FromBody] FinanceCurrencyModel update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != update.Curr)
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
                if (!_context.Financecurrency.Any(p => p.Curr == key))
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

        // PATCH
        public async Task<IActionResult> Patch([FromODataUri] string key, [FromBody] Delta<FinanceCurrencyModel> fincurr)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _context.Financecurrency.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            fincurr.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Financecurrency.Any(p => p.Curr == key))
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

        // DELETE
        public async Task<IActionResult> Delete([FromODataUri] string key)
        {
            var knowledge = await _context.Financecurrency.FindAsync(key);
            if (knowledge == null)
            {
                return NotFound();
            }

            _context.Financecurrency.Remove(knowledge);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        private bool CurrencyExists(string id)
        {
            return _context.Financecurrency.Any(e => e.Curr == id);
        }
    }
}
