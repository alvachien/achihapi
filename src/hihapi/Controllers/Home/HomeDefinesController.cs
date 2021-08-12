using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
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
        public IQueryable<HomeDefine> Get()
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

            return from hd in _context.HomeDefines
                    join hm in _context.HomeMembers
                        on hd.ID equals hm.HomeID
                    where hm.User == usrName
                       select hd;
        }

        ///// GET: /HomeDefines(:id)
        ///// <summary>
        ///// Adds support for getting a home define by key, for example:
        ///// 
        ///// GET /HomeDefines(1)
        ///// </summary>
        ///// <param name="id">The key of the home define required</param>
        ///// <returns>The home define</returns>
        //[EnableQuery]
        //[Authorize]
        //public SingleResult<HomeDefine> Get([FromODataUri] int id)
        //{
        //    String usrName = "";
        //    try
        //    {
        //        usrName = HIHAPIUtility.GetUserID(this);

        //        if (string.IsNullOrEmpty(usrName))
        //        {
        //            throw new UnauthorizedAccessException();
        //        }
        //    }
        //    catch
        //    {
        //        throw new UnauthorizedAccessException();
        //    }

        //    var hidquery = from hmem in _context.HomeMembers
        //                   join hdef in _context.HomeDefines on hmem.HomeID equals hdef.ID
        //                   where hmem.User == usrName && hmem.HomeID == id
        //                   select hdef;

        //    return SingleResult.Create(hidquery);
        //}

        [Authorize]
        public async Task<IActionResult> Post([FromBody]HomeDefine homedef)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (!homedef.IsValid(this._context))
                throw new BadRequestException("Inputted object IsValid Failed");

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

            homedef.Createdby = usrName;
            homedef.CreatedAt = DateTime.Now;
            foreach(var hmem in homedef.HomeMembers)
            {
                hmem.CreatedAt = homedef.CreatedAt;
                hmem.Createdby = usrName;
            }
            _context.HomeDefines.Add(homedef);

            await _context.SaveChangesAsync();

            return Created(homedef);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] HomeDefine update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("Inputted ID mismatched");
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
                throw new BadRequestException("Inputted Object IsValid Failed");

            // Find out the home define
            var existinghd = _context.HomeDefines.Find(key);

            if (existinghd == null)
            {
                throw new NotFoundException("Inputted Object Not Found");
            }
            else
            {
                update.Updatedby = usrName;
                update.UpdatedAt = DateTime.Now;
                update.CreatedAt = existinghd.CreatedAt;
                update.Createdby = existinghd.Createdby;
                _context.Entry(existinghd).CurrentValues.SetValues(update);

                var dbmems = _context.HomeMembers.Where(p => p.HomeID == key).ToList();
                foreach (var mem in update.HomeMembers)
                {
                    var memindb = dbmems.Find(p => p.HomeID == key && p.User == mem.User);
                    if (memindb == null)
                    {
                        mem.Createdby = usrName;
                        mem.CreatedAt = DateTime.Now;
                        _context.HomeMembers.Add(mem);
                    }
                    else
                    {
                        mem.CreatedAt = memindb.CreatedAt;
                        mem.Createdby = memindb.Createdby;
                        _context.Entry(memindb).CurrentValues.SetValues(mem);
                    }
                }
                foreach (var mem in dbmems)
                {
                    var nmem = update.HomeMembers.FirstOrDefault(p => p.User == mem.User);
                    if (nmem == null)
                    {
                        _context.HomeMembers.Remove(mem);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.HomeDefines.Any(p => p.ID == key))
                {
                    throw new NotFoundException("Inputted Object Not Found");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == key && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var cc = await _context.HomeDefines.FindAsync(key);
            if (cc == null)
            {
                throw new NotFoundException("Inputted Object Not Found");
            }

            // Perform the checks
            if (!cc.IsDeleteAllowed(this._context))
                throw new BadRequestException("Inputted Object IsDeleteAllowed Failed");

            _context.HomeDefines.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
