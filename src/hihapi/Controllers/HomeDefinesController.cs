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
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using hihapi.Exceptions;

namespace hihapi.Controllers
{
    public class HomeDefinesController : ODataController
    {
        private readonly hihDataContext _context;

        public HomeDefinesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /HomeDefines
        /// <summary>
        /// Adds support for getting home def., for example:
        /// 
        /// GET /HomeDefines
        /// GET /HomeDefines?$filter=Host eq 'abc'
        /// GET /HomeDefines?
        /// 
        /// <remarks>
        [EnableQuery]
        [Authorize]
        public IActionResult Get()
        {
            String usrName = "";
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

            var rst = (from hd in _context.HomeDefines
                    join hm in _context.HomeMembers
                        on hd.ID equals hm.HomeID
                    where hm.User == usrName
                       select hd);

            return Ok(rst);
        }

        /// GET: /HomeDefines(:id)
        /// <summary>
        /// Adds support for getting a home define by key, for example:
        /// 
        /// GET /HomeDefines(1)
        /// </summary>
        /// <param name="id">The key of the home define required</param>
        /// <returns>The home define</returns>
        [EnableQuery]
        [Authorize]
        public SingleResult<HomeDefine> Get([FromODataUri] int id)
        {
            String usrName = "";
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);

                if (string.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var hidquery = from hmem in _context.HomeMembers
                           join hdef in _context.HomeDefines on hmem.HomeID equals hdef.ID
                           where hmem.User == usrName && hmem.HomeID == id
                           select hdef;

            return SingleResult.Create(hidquery);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody]HomeDefine homedef)
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

            if (!homedef.IsValid(this._context))
                return BadRequest();

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

            _context.HomeDefines.Add(homedef);

            await _context.SaveChangesAsync();

            _context.Entry(homedef).State = EntityState.Detached;
            return Created(homedef);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] HomeDefine update)
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == update.ID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!update.IsValid(this._context))
                return BadRequest();

            _context.Entry(update).State = EntityState.Modified;
            foreach(var mem in update.HomeMembers)
            {

            }
            try
            {
                await _context.SaveChangesAsync();
                _context.Entry(update).State = EntityState.Detached;
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
            var cc = await _context.HomeDefines.FindAsync(key);
            if (cc == null)
            {
                return NotFound();
            }

            // Perform the checks
            if (!cc.IsDeleteAllowed(this._context))
                return BadRequest();

            _context.HomeDefines.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
