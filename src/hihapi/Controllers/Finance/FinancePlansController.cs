using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;

namespace hihapi.Controllers
{
    [Authorize]
    public sealed class FinancePlansController : ODataController
    {
        private readonly hihDataContext _context;

        public FinancePlansController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HttpGet]
        //public IActionResult Get(ODataQueryOptions<FinancePlan> option)
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

            // Check whether User assigned with specified Home ID
            return Ok(from hmem in _context.HomeMembers
                        where hmem.User == usrName
                        select new { HomeID = hmem.HomeID } into hids
                        join ords in _context.FinancePlan on hids.HomeID equals ords.HomeID
                        select ords);

            //return Ok(option.ApplyTo(query));
        }

        [EnableQuery]
        [HttpGet]
        public SingleResult<FinancePlan> Get([FromODataUri] int key)
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

            // Check whether User assigned with specified Home ID
            return SingleResult.Create(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { HomeID = hmem.HomeID } into hids
                      join ords in _context.FinancePlan on hids.HomeID equals ords.HomeID
                      where ords.ID == key
                      select ords);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FinancePlan plan)
        {
            if (!ModelState.IsValid)
                HIHAPIUtility.HandleModalStateError(ModelState);

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

            // Check
            if (!plan.IsValid(this._context))
                throw new BadRequestException("Check IsValid failed");

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == plan.HomeID && p.User == usrName).Count();
            if (hms <= 0)
                throw new UnauthorizedAccessException();

            plan.Createdby = usrName;
            plan.CreatedAt = DateTime.Now;
            _context.FinancePlan.Add(plan);
            await _context.SaveChangesAsync();

            return Created(plan);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinancePlan update)
        {
            if (!ModelState.IsValid)
                HIHAPIUtility.HandleModalStateError(ModelState);

            if (key != update.ID)
                throw new BadRequestException("Inputted ID mismatched");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == update.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!update.IsValid(this._context))
                throw new BadRequestException("Inputted Object IsValid failed");

            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinancePlan.Any(p => p.ID == key))
                {
                    throw new NotFoundException("Inputted ID not found");
                }
                else
                {
                    throw new DBOperationException(exp.Message);
                }
            }

            return Updated(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinancePlan.FindAsync(key);
            if (cc == null)
                throw new NotFoundException("Inputted ID not found");

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
                throw new BadRequestException("Inputted ID IsDeleteAllowed failed");

            _context.FinancePlan.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
