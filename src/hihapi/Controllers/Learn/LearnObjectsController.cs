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
using Microsoft.AspNet.OData.Query;

namespace hihapi.Controllers
{
    public sealed class LearnObjectsController : ODataController
    {
        private readonly hihDataContext _context;

        public LearnObjectsController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        public IQueryable Get(ODataQueryOptions<LearnObject> option)
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
                        join objs in _context.LearnObjects on hids.HomeID equals objs.HomeID
                        select objs;

            return option.ApplyTo(query);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody] LearnObject obj)
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
            if (!obj.IsValid(this._context))
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == obj.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.LearnObjects.Add(obj);
            await _context.SaveChangesAsync();

            return Created(obj);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] LearnObject update)
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
                if (!_context.LearnObjects.Any(p => p.ID == key))
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
            var cc = await _context.LearnObjects.FindAsync(key);
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

            _context.LearnObjects.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
