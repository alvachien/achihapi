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
    public class LibraryBooksController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryBooksController(hihDataContext context)
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

            return Ok(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { hmem.HomeID, hmem.User, hmem.IsChild } into hmems
                      join book in _context.Books
                        on hmems.HomeID equals book.HomeID
                      select book);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryBook Get([FromODataUri] Int32 key)
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

            var hidquery = from hmem in _context.HomeMembers
                           where hmem.User == usrName
                           select new { HomeID = hmem.HomeID };
            var ordquery = from ord in _context.Books
                           where ord.Id == key
                           select ord;
            var rstquery = from ord in ordquery
                           join hid in hidquery
                           on ord.HomeID equals hid.HomeID
                           select ord;

            return rstquery.SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryBook tbc)
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == tbc.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            _context.Books.Add(tbc);
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

            var tbd = await _context.Books.FindAsync(key);
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_author WHERE BOOK_ID = " + key.ToString());
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_ctgy WHERE BOOK_ID = " + key.ToString());
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_location WHERE BOOK_ID = " + key.ToString());
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_press WHERE BOOK_ID = " + key.ToString());
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_translator WHERE BOOK_ID = " + key.ToString());
                _context.Database.ExecuteSqlRaw("DELETE FROM t_lib_book_def WHERE ID = " + key.ToString());

                await transaction.CommitAsync();
            }
            catch (Exception exp)
            {
                transaction.Rollback();
            }
            //_context.Books.Remove(tbd);
            //await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
