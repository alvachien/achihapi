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
    public class LibraryPersonsController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryPersonsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceAccounts
        [EnableQuery]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinanceAccount> option)
        public IActionResult Get()
        {
            //String usrName = String.Empty;
            //try
            //{
            //    usrName = HIHAPIUtility.GetUserID(this);
            //    if (String.IsNullOrEmpty(usrName))
            //        throw new UnauthorizedAccessException();
            //}
            //catch
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //// Check whether User assigned with specified Home ID
            //return Ok(from hmem in _context.HomeMembers
            //          where hmem.User == usrName
            //          select new { hmem.HomeID, hmem.User, hmem.IsChild } into hmems
            //          join acnts in _context.FinanceAccount
            //            on hmems.HomeID equals acnts.HomeID
            //          where (hmems.IsChild == true && hmems.User == acnts.Owner)
            //              || !hmems.IsChild.HasValue
            //              || hmems.IsChild == false
            //          select acnts);
            return Ok(_context.Persons);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryPerson Get([FromODataUri] Int32 key)
        {
            //String usrName = String.Empty;
            //try
            //{
            //    usrName = HIHAPIUtility.GetUserID(this);
            //    if (String.IsNullOrEmpty(usrName))
            //        throw new UnauthorizedAccessException();
            //}
            //catch
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //var hidquery = from hmem in _context.HomeMembers
            //               where hmem.User == usrName
            //               select new { HomeID = hmem.HomeID };
            //var acntquery = from acnt in _context.FinanceAccount
            //                where acnt.ID == key
            //                select acnt;
            //return (from acnt in acntquery
            //        join hid in hidquery
            //        on acnt.HomeID equals hid.HomeID
            //        select acnt).SingleOrDefault();
            return (from p in _context.Persons where p.Id == key select p).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryPerson tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // User
            //String usrName = String.Empty;
            //try
            //{
            //    usrName = HIHAPIUtility.GetUserID(this);
            //    if (String.IsNullOrEmpty(usrName))
            //    {
            //        throw new UnauthorizedAccessException();
            //    }
            //}
            //catch
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //// Check whether User assigned with specified Home ID
            //var hms = _context.HomeMembers.Where(p => p.HomeID == account.HomeID && p.User == usrName).Count();
            //if (hms <= 0)
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //if (!account.IsValid(this._context))
            //    return BadRequest();

            //account.CreatedAt = DateTime.Now;
            //account.Createdby = usrName;
            //_context.FinanceAccount.Add(account);
            _context.Persons.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var tbd = await _context.Persons.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            _context.Persons.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
