using System;
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
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;

namespace hihapi.Controllers
{
    public class FinanceAccountCategoriesController : ODataController
    {
        public const Int32 AccountCategory_AdvancePayment = 8;
        public const Int32 AccountCategory_Asset = 7;
        public const Int32 AccountCategory_BorrowFrom = 9;
        public const Int32 AccountCategory_LendTo = 10;
        public const Int32 AccountCategory_AdvanceReceive = 11;
        public const Int32 AccountCategory_Insurance = 12;
        private readonly hihDataContext _context;

        public FinanceAccountCategoriesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceAccountCategories
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceAccountCategory> Get(Int32? hid = null)
        {
            if (hid.HasValue)
            {
                String usrName = String.Empty;
                try
                {
                    usrName = HIHAPIUtility.GetUserID(this);
                }
                catch
                {
                    // Do nothing
                    usrName = String.Empty;
                }

                if (String.IsNullOrEmpty(usrName))
                    return _context.FinAccountCategories.Where(p => p.HomeID == null);

                var rst =
                   from hmem in _context.HomeMembers.Where(p => p.User == usrName && p.HomeID == hid.Value)
                   from acntctgy in _context.FinAccountCategories.Where(p => p.HomeID == null || p.HomeID == hmem.HomeID)
                   select acntctgy;

                return rst;
            }

            return _context.FinAccountCategories.Where(p => p.HomeID == null);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody] FinanceAccountCategory ctgy)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }

            // Check
            if (!ctgy.IsValid() || !ctgy.HomeID.HasValue)
            {
                return BadRequest();
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
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == ctgy.HomeID.Value && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.FinAccountCategories.Add(ctgy);
            await _context.SaveChangesAsync();

            return Created(ctgy);
        }
    
        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceAccountCategory update)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }
            if (key != update.ID)
            {
                return BadRequest();
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
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == update.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinAccountCategories.Any(p => p.ID == key))
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
            var cc = await _context.FinAccountCategories.FindAsync(key);
            if (cc == null)
            {
                return NotFound();
            }

            _context.FinAccountCategories.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
