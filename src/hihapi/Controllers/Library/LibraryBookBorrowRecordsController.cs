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
    public class LibraryBookBorrowRecordsController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryBookBorrowRecordsController(hihDataContext context)
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
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            return Ok(from record in _context.BookBorrowRecords
                      where record.User == usrName
                      select record);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryBookBorrowRecord Get([FromODataUri] Int32 key)
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

            return (from record in _context.BookBorrowRecords
                      where record.User == usrName && record.Id == key
                      select record).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryBookBorrowRecord tbc)
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
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == tbc.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!tbc.IsValid(_context))
            {
                throw new BadRequestException("Not a valid object");
            }

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            _context.BookBorrowRecords.Add(tbc);
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
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var tbd = await _context.BookBorrowRecords.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == tbd.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            _context.BookBorrowRecords.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
