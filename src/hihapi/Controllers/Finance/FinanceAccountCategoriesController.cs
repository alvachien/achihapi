using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class FinanceAccountCategoriesController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceAccountCategoriesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceAccountCategories
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceAccountCategory> Get()
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
                return _context.FinAccountCategories.Where(p => p.HomeID == null);

            var rst0 = from acntctgy in _context.FinAccountCategories
                       where acntctgy.HomeID == null
                       select acntctgy;
            var rst1 = from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { HomeID = hmem.HomeID } into hids
                      join acntctgy in _context.FinAccountCategories on hids.HomeID equals acntctgy.HomeID
                      select acntctgy;

            return rst0.Union(rst1);
        }

        /// GET: /FinanceAccountCategories(:id)
        //[EnableQuery]
        //[Authorize]
        //public SingleResult<FinanceAccountCategory> Get([FromODataUri] int id)
        //{
        //    String usrName = String.Empty;
        //    try
        //    {
        //        usrName = HIHAPIUtility.GetUserID(this);
        //    }
        //    catch
        //    {
        //        // Do nothing
        //        usrName = String.Empty;
        //    }

        //    if (String.IsNullOrEmpty(usrName))
        //        return SingleResult.Create(_context.FinAccountCategories.Where(p => p.ID == id && p.HomeID == null));

        //    var rst = from hmem in _context.HomeMembers.Where(p => p.User == usrName)
        //              from acntctgy in _context.FinAccountCategories.Where(p => p.ID == id && (p.HomeID == null || p.HomeID == hmem.HomeID))
        //              select acntctgy;

        //    return SingleResult.Create(rst);
        //}

        [Authorize]
        public async Task<IActionResult> Post([FromBody] FinanceAccountCategory ctgy)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Check
            if (!ctgy.IsValid(this._context) || !ctgy.HomeID.HasValue)
            {
                throw new BadRequestException("Inputted object IsValid failed");
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

            ctgy.Createdby = usrName;
            ctgy.CreatedAt = DateTime.Now;
            _context.FinAccountCategories.Add(ctgy);
            await _context.SaveChangesAsync();

            return Created(ctgy);
        }
    
        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceAccountCategory update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (key != update.ID)
            {
                throw new BadRequestException("ID mismatched");
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

            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinAccountCategories.Any(p => p.ID == key))
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
            var cc = await _context.FinAccountCategories.FindAsync(key);
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

            _context.FinAccountCategories.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
