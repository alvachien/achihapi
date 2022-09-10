﻿using System;
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
    public class LibraryPersonRolesController : ODataController
    {
        private readonly hihDataContext _context;

        public LibraryPersonRolesController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.PersonRoles);
        }

        [EnableQuery]
        [HttpGet]
        public LibraryPersonRole Get([FromODataUri] Int32 key)
        {
            return (from p in _context.PersonRoles where p.Id == key select p).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LibraryPersonRole tbc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }
            _context.PersonRoles.Add(tbc);
            await _context.SaveChangesAsync();

            return Created(tbc);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var tbd = await _context.PersonRoles.FindAsync(key);
            if (tbd == null)
            {
                return NotFound();
            }

            //// User
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
            //var hms = _context.HomeMembers.Where(p => p.HomeID == cc.HomeID && p.User == usrName).Count();
            //if (hms <= 0)
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //if (!cc.IsDeleteAllowed(this._context))
            //    return BadRequest();

            _context.PersonRoles.Remove(tbd);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}