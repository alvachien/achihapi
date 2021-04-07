using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using hihapi.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class CurrenciesController : ODataController
    {
        private readonly hihDataContext _context;

        public CurrenciesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /Currencies
        /// <summary>
        /// Adds support for getting currencies, for example:
        /// 
        /// GET /Currencies
        /// GET /Currencies?$filter=Curr eq 'Dollar'
        /// GET /Currencies?
        /// 
        /// <remarks>
        [EnableQuery]
        [ResponseCache(Duration = 3600)]
        public IQueryable<Currency> Get()
        {
            return _context.Currencies;
        }

        /// GET: /Currencies(:id)
        /// <summary>
        /// Adds support for getting a currency by key, for example:
        /// 
        /// GET /Currencies(1)
        /// </summary>
        /// <param name="curr">The key of the currency required</param>
        /// <returns>The currency</returns>
        [EnableQuery]
        public SingleResult<Currency> Get([FromODataUri] string curr)
        {
            return SingleResult.Create(_context.Currencies.Where(p => p.Curr == curr));
        }

        // POST: /Currencies
        /// <summary>
        /// Support for creating currency
        /// </summary>
        //public async Task<IActionResult> Post([FromBody] Currency currency)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        foreach (var value in ModelState.Values)
        //        {
        //            foreach(var err in value.Errors) 
        //            {
        //                System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
        //            }
        //        }

        //        return BadRequest();
        //    }

        //    _context.Currencies.Add(currency);
        //    await _context.SaveChangesAsync();

        //    return Created(currency);
        //}

        //// PUT: /Currencies/5
        ///// <summary>
        ///// Support for updating Currencies
        ///// </summary>
        //public async Task<IActionResult> Put([FromODataUri] string curr, [FromBody] Currency update)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (curr != update.Curr)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(update).State = EntityState.Modified;
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!_context.Currencies.Any(p => p.Curr == curr))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return Updated(update);
        //}

        //// DELETE: /Currencies/5
        ///// <summary>
        ///// Support for deleting currency by key.
        ///// </summary>
        //public async Task<IActionResult> Delete([FromODataUri] int key)
        //{
        //    var currency = await _context.Currencies.FindAsync(key);
        //    if (currency == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Currencies.Remove(currency);
        //    await _context.SaveChangesAsync();

        //    return StatusCode(204); // HttpStatusCode.NoContent
        //}

        //// PATCH: /Currencies
        ///// <summary>
        ///// Support for partial updates of Currencies
        ///// </summary>
        //public async Task<IActionResult> Patch([FromODataUri] string curr, [FromBody] Delta<Currency> currency)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var entity = await _context.Currencies.FindAsync(curr);
        //    if (entity == null)
        //    {
        //        return NotFound();
        //    }

        //    currency.Patch(entity);

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!_context.Currencies.Any(p => p.Curr == curr))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return Updated(entity);
        //}
    }
}
