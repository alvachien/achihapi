using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using System;
using hihapi.Exceptions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    [Authorize]
    public class BlogUserSettingsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogUserSettingsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<BlogUserSetting> Get()
        {
            string usrName = "";
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

            return _context.BlogUserSettings.Where(p => p.Owner == usrName);
        }

        [EnableQuery]
        public SingleResult<BlogUserSetting> Get([FromODataUri] string owner)
        {
            string usrName;
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

            return SingleResult.Create(_context.BlogUserSettings.Where(p => p.Owner == owner));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BlogUserSetting newsetting)
        {
            // Not Yet possible
            return Forbid();
        }

        public async Task<IActionResult> Put([FromODataUri]string owner, [FromBody] BlogUserSetting update)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
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
                if (String.CompareOrdinal(update.Owner, usrName) != 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            //// Check setting
            //var setting = _context.BlogUserSettings.SingleOrDefault(p => p.Owner == usrName);
            //if (setting == null)
            //{
            //    throw new BadRequestException("User has no setting ");
            //}

            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.BlogUserSettings.Any(p => p.Owner == update.Owner))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(update);
        }

        [HttpGet]
        public IActionResult Deploy([FromODataUri]string owner)
        {
            // User
            string usrName = "";
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

            if (!string.IsNullOrEmpty(owner))
            {                
                if (String.CompareOrdinal(owner, usrName) != 0)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            var setting = _context.BlogUserSettings.SingleOrDefault(p => p.Owner == usrName);
            if (setting == null)
            {
                throw new NotFoundException("Owner not found");
            }

            var errstr = "";
            try
            {
                BlogDeployUtility.UpdatePostSetting(setting);
            }
            catch(Exception exp)
            {
                errstr = exp.Message;
            }

            // Return
            if (!string.IsNullOrEmpty(errstr))
            {
                throw new Exception(errstr);
            }

            return Ok("");
        }
    }
}