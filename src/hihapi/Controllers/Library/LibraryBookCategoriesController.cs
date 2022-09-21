using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Deltas;
using hihapi.Models.Library;

namespace hihapi.Controllers.Library
{
    [Authorize]
    public class LibraryBookCategoriesController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryBookCategoriesController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
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
                return Ok(_context.BookCategories.Where(p => p.HomeID == null));

            var rst0 = from ctgy in _context.BookCategories
                       where ctgy.HomeID == null
                       select ctgy;
            var rst1 = from hmem in _context.HomeMembers
                       where hmem.User == usrName
                       select new { HomeID = hmem.HomeID } into hids
                       join ctgy in _context.BookCategories on hids.HomeID equals ctgy.HomeID
                       select ctgy;

            return Ok(rst0.Union(rst1));
        }

        [EnableQuery]
        [HttpGet]
        public LibraryBookCategory Get([FromODataUri] Int32 key)
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
                return _context.BookCategories.Where(p => p.Id == key && p.HomeID == null).SingleOrDefault();

            return (from ctgy in _context.BookCategories
                    join hmem in _context.HomeMembers
                      on ctgy.HomeID equals hmem.HomeID into hmem2
                    from nhmem in hmem2.DefaultIfEmpty()
                    where ctgy.Id == key && (nhmem == null || nhmem.User == usrName)
                    select ctgy).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryBookCategory tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == tbc.HomeID.Value && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            _context.BookCategories.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        [HttpDelete]
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

            var tbd = await _context.BookCategories.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == tbd.HomeID.Value && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.BookCategories.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

    }
}
