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
using hihapi.Models.Event;

namespace hihapi.Controllers.Event
{
    public class NormalEventsController : ODataController
    {
        private readonly hihDataContext _context;

        public NormalEventsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            //String usrName = String.Empty;
            //try
            //{
            //    usrName = HIHAPIUtility.GetUserID(this);
            //}
            //catch
            //{
            //    // Do nothing
            //    usrName = String.Empty;
            //}

            //if (String.IsNullOrEmpty(usrName))
            //    return Ok(_context.PersonRoles.Where(p => p.HomeID == null));

            //var rst0 = from ctgy in _context.PersonRoles
            //           where ctgy.HomeID == null
            //           select ctgy;
            //var rst1 = from hmem in _context.HomeMembers
            //           where hmem.User == usrName
            //           select new { HomeID = hmem.HomeID } into hids
            //           join ctgy in _context.PersonRoles on hids.HomeID equals ctgy.HomeID
            //           select ctgy;

            //return Ok(rst0.Union(rst1));
            return Ok(this._context.NormalEvents);
        }

        [EnableQuery]
        [HttpGet]
        public NormalEvent Get([FromODataUri] Int32 key)
        {
            return this._context.NormalEvents.Where(p => p.Id == key).SingleOrDefault();
        }
    }
}
