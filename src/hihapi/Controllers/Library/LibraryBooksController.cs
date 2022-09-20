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
            return Ok(_context.Books);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryBook Get([FromODataUri] Int32 key)
        {
            return (from p in _context.Books where p.Id == key select p).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryBook tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }
            _context.Books.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var tbd = await _context.Books.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
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
