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
    public class LibraryOrganizationTypesController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryOrganizationTypesController(hihDataContext context)
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
                return Ok(_context.FinAccountCategories.Where(p => p.HomeID == null));

            var rst0 = from ctgy in _context.OrganizationTypes
                       where ctgy.HomeID == null
                       select ctgy;
            var rst1 = from hmem in _context.HomeMembers
                       where hmem.User == usrName
                       select new { HomeID = hmem.HomeID } into hids
                       join ctgy in _context.OrganizationTypes on hids.HomeID equals ctgy.HomeID
                       select ctgy;

            return Ok(rst0.Union(rst1));
        }

        [EnableQuery]
        [HttpGet]
        public LibraryOrganizationType Get([FromODataUri] Int32 key)
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
                return _context.OrganizationTypes.Where(p => p.Id == key && p.HomeID == null).SingleOrDefault();

            return (from ctgy in _context.OrganizationTypes
                    join hmem in _context.HomeMembers
                      on ctgy.HomeID equals hmem.HomeID into hmem2
                    from nhmem in hmem2.DefaultIfEmpty()
                    where ctgy.Id == key && (nhmem == null || nhmem.User == usrName)
                    select ctgy).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryOrganizationType tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

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

            _context.OrganizationTypes.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
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

            var tbd = await _context.OrganizationTypes.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            _context.OrganizationTypes.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
