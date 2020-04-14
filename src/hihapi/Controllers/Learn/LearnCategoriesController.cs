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
    public sealed class LearnCategoriesController : ODataController
    {
        private readonly hihDataContext _context;

        public LearnCategoriesController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [Authorize]
        public IQueryable<LearnCategory> Get()
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
                return _context.LearnCategories.Where(p => p.HomeID == null);

            var rst0 = from ctgy in _context.LearnCategories
                       where ctgy.HomeID == null
                       select ctgy;
            var rst1 = from hmem in _context.HomeMembers
                       where hmem.User == usrName
                       select new { HomeID = hmem.HomeID } into hids
                       join ctgy in _context.LearnCategories on hids.HomeID equals ctgy.HomeID
                       select ctgy;

            return rst0.Union(rst1);
        }

        [EnableQuery]
        [Authorize]
        public SingleResult<LearnCategory> Get([FromODataUri] int id)
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
                return SingleResult.Create(_context.LearnCategories.Where(p => p.ID == id && p.HomeID == null));

            var rst = from hmem in _context.HomeMembers.Where(p => p.User == usrName)
                      from acntctgy in _context.LearnCategories.Where(p => p.ID == id && (p.HomeID == null || p.HomeID == hmem.HomeID))
                      select acntctgy;

            return SingleResult.Create(rst);
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody] LearnCategory ctgy)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Check
            if (!ctgy.IsValid(this._context) || !ctgy.HomeID.HasValue)
            {
                throw new BadRequestException("Inputted Object IsValid Failed");
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

            ctgy.CreatedAt = DateTime.Now;
            ctgy.Createdby = usrName;
            _context.LearnCategories.Add(ctgy);
            await _context.SaveChangesAsync();

            return Created(ctgy);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] LearnCategory update)
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

            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.LearnCategories.Any(p => p.ID == key))
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

        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.LearnCategories.FindAsync(key);
            if (cc == null)
            {
                throw new NotFoundException("Inputted Object Not Found");
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
                throw new BadRequestException("Inputted Object IsDeleteAllowed");

            _context.LearnCategories.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
