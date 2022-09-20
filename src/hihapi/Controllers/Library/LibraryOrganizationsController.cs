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
    public class LibraryOrganizationsController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryOrganizationsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Organizations);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryOrganization Get([FromODataUri] Int32 key)
        {
            return (from p in _context.Organizations where p.Id == key select p).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryOrganization tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }
            _context.Organizations.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var tbd = await _context.Organizations.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            _context.Organizations.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

    }
}
