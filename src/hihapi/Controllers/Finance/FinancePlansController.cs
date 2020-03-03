using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    public sealed class FinancePlansController : ODataController
    {
        private readonly hihDataContext _context;

        public FinancePlansController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        public IQueryable Get(ODataQueryOptions<FinancePlan> option)
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check whether User assigned with specified Home ID
            var query = from hmem in _context.HomeMembers
                        where hmem.User == usrName
                        select new { HomeID = hmem.HomeID } into hids
                        join ords in _context.FinancePlan on hids.HomeID equals ords.HomeID
                        select ords;

            return option.ApplyTo(query);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody] FinancePlan plan)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest();
            }

            // Check
            if (!plan.IsValid(this._context))
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == plan.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.FinancePlan.Add(plan);
            await _context.SaveChangesAsync();

            return Created(plan);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinancePlan update)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

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

            if (!update.IsValid(this._context))
                return BadRequest();

            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinancePlan.Any(p => p.ID == key))
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
            var cc = await _context.FinancePlan.FindAsync(key);
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
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == cc.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!cc.IsDeleteAllowed(this._context))
                return BadRequest();

            _context.FinancePlan.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
