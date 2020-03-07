using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;

namespace hihapi.Controllers
{
    public class FinanceAssetCategoriesController: ODataController
    {        
        private readonly hihDataContext _context;
        
        public FinanceAssetCategoriesController(hihDataContext context)
        {
            _context = context;
        }
        
        /// GET: /FinanceAssertCategories
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceAssetCategory> Get(Int32? hid = null)
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
                return _context.FinAssetCategories.Where(p => p.HomeID == null);

            var rst0 = from acntctgy in _context.FinAssetCategories
                       where acntctgy.HomeID == null
                       select acntctgy;
            var rst1 = from hmem in _context.HomeMembers
                       where hmem.User == usrName
                       select new { HomeID = hmem.HomeID } into hids
                       join acntctgy in _context.FinAssetCategories on hids.HomeID equals acntctgy.HomeID
                       select acntctgy;

            return rst0.Union(rst1);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody] FinanceAssetCategory ctgy)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Check
            if (!ctgy.IsValid(this._context) || !ctgy.HomeID.HasValue)
            {
                throw new BadRequestException("Inputted category IsValid failed");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == ctgy.HomeID.Value && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!ctgy.IsValid(this._context))
                return BadRequest();

            _context.FinAssetCategories.Add(ctgy);
            await _context.SaveChangesAsync();

            return Created(ctgy);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceAssetCategory update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("Key is mismatch");
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

            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinAssetCategories.Any(p => p.ID == key))
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

        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinAssetCategories.FindAsync(key);
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

            _context.FinAssetCategories.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
