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
    [Authorize]
    public class RecurEventsController : ODataController
    {
        private readonly hihDataContext _context;

        public RecurEventsController(hihDataContext context)
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
                      join nrevt in this._context.RecurEvents
                        on hmems.HomeID equals nrevt.HomeID
                      select nrevt);
        }

        [EnableQuery]
        [HttpGet]
        public RecurEvent Get([FromODataUri] Int32 key)
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
            var eventquery = from ord in _context.RecurEvents
                             where ord.Id == key
                             select ord;
            var rstquery = from ord in eventquery
                           join hid in hidquery
                           on ord.HomeID equals hid.HomeID
                           select ord;

            return rstquery.SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RecurEvent tbc)
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

            // Generate the 
            var eventdates = CommonUtility.WorkoutRepeatedDates(new RepeatDatesCalculationInput
            {
                StartDate = tbc.StartDate,
                EndDate = tbc.EndDate,
                RepeatType = tbc.RecurType,
            });

            var dateidx = 1;
            foreach(var edate in eventdates)
            {
                var gevent = new NormalEvent();
                gevent.StartDate = edate.StartDate;
                gevent.EndDate = edate.EndDate;
                gevent.Name = tbc.Name + (dateidx++) + '/' + eventdates.Count;
                gevent.CreatedAt = DateTime.Now;
                gevent.Createdby = usrName;
                tbc.RelatedEvents.Add(gevent);
            }

            tbc.CreatedAt = DateTime.Now;
            tbc.Createdby = usrName;

            _context.RecurEvents.Add(tbc);
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

            var tbd = await _context.RecurEvents.FindAsync(key);
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

            _context.RecurEvents.Remove(tbd);
            // TBD. Delete the generated events
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }

        //[HttpPost]
        //public IActionResult GenerateNormalEvents([FromBody] ODataActionParameters parameters)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // User
        //    String usrName = String.Empty;
        //    try
        //    {
        //        usrName = HIHAPIUtility.GetUserID(this);
        //        if (String.IsNullOrEmpty(usrName))
        //        {
        //            throw new UnauthorizedAccessException();
        //        }
        //    }
        //    catch
        //    {
        //        throw new UnauthorizedAccessException();
        //    }
        //}
    }
}
