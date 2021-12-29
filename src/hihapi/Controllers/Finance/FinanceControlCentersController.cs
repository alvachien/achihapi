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

namespace hihapi.Controllers
{
    [Authorize]
    public sealed class FinanceControlCentersController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceControlCentersController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceControlCenters
        [EnableQuery]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinanceControlCenter> option)
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

            //var query = from hmem in _context.HomeMembers
            //            where hmem.User == usrName
            //            select new { HomeID = hmem.HomeID } into hids
            //            join ccs in _context.FinanceControlCenter on hids.HomeID equals ccs.HomeID
            //            select ccs;

            // Check whether User assigned with specified Home ID
            return Ok(from hmem in _context.HomeMembers
                        where hmem.User == usrName
                        select new { hmem.HomeID, hmem.User, hmem.IsChild } into hmems
                        join ccs in _context.FinanceControlCenter
                          on hmems.HomeID equals ccs.HomeID
                        where (hmems.IsChild == true && hmems.User == ccs.Owner)
                            || !hmems.IsChild.HasValue
                            || hmems.IsChild == false
                        select ccs);
            //return Ok(option.ApplyTo(query));
        }

        [EnableQuery]
        [HttpGet]
        public FinanceControlCenter Get(int key)
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
            var ccquery = from cc in _context.FinanceControlCenter
                          where cc.ID == key
                          select cc;
            return (from cc in ccquery
                    join hid in hidquery
                    on cc.HomeID equals hid.HomeID
                    select cc).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]FinanceControlCenter controlCenter)
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == controlCenter.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();        
            }

            if (!controlCenter.IsValid(this._context))
                return BadRequest();

            controlCenter.CreatedAt = DateTime.Now;
            controlCenter.Createdby = usrName;
            _context.FinanceControlCenter.Add(controlCenter);
            await _context.SaveChangesAsync();

            return Created(controlCenter);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody]FinanceControlCenter update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("Inputted ID mismatched");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == update.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();        
            }

            if (!update.IsValid(this._context))
                return BadRequest();

            update.Updatedby = usrName;
            update.UpdatedAt = DateTime.Now;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinanceControlCenter.Any(p => p.ID == key))
                {
                    return NotFound();
                }
                else
                {
                    throw new DBOperationException(exp.Message);
                }
            }

            return Updated(update);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<FinanceControlCenter> coll)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            var entity = await _context.FinanceControlCenter.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            // User
            string usrName;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
                if (String.CompareOrdinal(entity.Owner, usrName) != 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Patch it
            coll.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FinanceControlCenter.Any(p => p.ID == key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(entity);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinanceControlCenter.FindAsync(key);
            if (cc == null)
            {
                return NotFound();
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == cc.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!cc.IsDeleteAllowed(this._context))
                return BadRequest();

            _context.FinanceControlCenter.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
