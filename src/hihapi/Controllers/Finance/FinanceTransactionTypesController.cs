using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;

namespace hihapi.Controllers
{
    [Authorize]
    public class FinanceTransactionTypesController: ODataController
    {        
        private readonly hihDataContext _context;
        
        public FinanceTransactionTypesController(hihDataContext context)
        {
            _context = context;
        }
        
        /// GET: /FinanceAssertCategories
        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
            }
            catch
            {
                // Do nothing
                usrName = String.Empty;
            }

            if (String.IsNullOrEmpty(usrName))
                return Ok(_context.FinTransactionType.Where(p => p.HomeID == null));

            var rst0 = from acntctgy in _context.FinTransactionType
                       where acntctgy.HomeID == null
                       select acntctgy;
            var rst1 = from hmem in _context.HomeMembers
                       where hmem.User == usrName
                       select new { HomeID = hmem.HomeID } into hids
                       join acntctgy in _context.FinTransactionType on hids.HomeID equals acntctgy.HomeID
                       select acntctgy;

            return Ok(rst0.Union(rst1));
        }

        [EnableQuery]
        [HttpGet]
        public FinanceTransactionType Get([FromODataUri] int key)
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
            }
            catch
            {
                // Do nothing
                usrName = String.Empty;
            }

            if (String.IsNullOrEmpty(usrName))
                return _context.FinTransactionType.Where(p => p.ID == key && p.HomeID == null).SingleOrDefault();

            return (from ctgy in _context.FinTransactionType
                    join hmem in _context.HomeMembers
                      on ctgy.HomeID equals hmem.HomeID into hmem2
                    from nhmem in hmem2.DefaultIfEmpty()
                    where ctgy.ID == key && (nhmem == null || nhmem.User == usrName)
                    select ctgy).SingleOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FinanceTransactionType ctgy)
        {
            if (!ModelState.IsValid)
                HIHAPIUtility.HandleModalStateError(ModelState);

            // Check
            if (!ctgy.IsValid(this._context) || !ctgy.HomeID.HasValue)
                throw new BadRequestException("Inputted Object IsValid failed");

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
            var hms = _context.HomeMembers.Where(p => p.HomeID == ctgy.HomeID.Value && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            ctgy.CreatedAt = DateTime.Now;
            ctgy.Createdby = usrName;
            _context.FinTransactionType.Add(ctgy);
            await _context.SaveChangesAsync();

            return Created(ctgy);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceTransactionType update)
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
                if (!_context.FinTransactionType.Any(p => p.ID == key))
                {
                    throw new NotFoundException("Inputted Object Not found");
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

            var cc = await _context.FinTransactionType.FindAsync(key);
            if (cc == null)
                return NotFound();

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == cc.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!cc.IsDeleteAllowed(this._context))
                throw new BadRequestException("Inputted Object IsDeleteAllowed failed");

            _context.FinTransactionType.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
