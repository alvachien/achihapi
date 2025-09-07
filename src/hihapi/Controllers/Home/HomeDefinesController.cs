using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    [Authorize]
    public class HomeDefinesController : ODataController
    {
        private readonly hihDataContext _context;

        public HomeDefinesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /HomeDefines
        /// <summary>
        /// Adds support for getting home def., for example:
        /// 
        /// GET /HomeDefines
        /// GET /HomeDefines?$filter=Host eq 'abc'
        /// GET /HomeDefines?
        /// 
        /// <remarks>
        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            String usrName = "";
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

            var hids = (from hmem in _context.HomeMembers
                       where hmem.User == usrName
                       select hmem.HomeID).ToList();

            return Ok(from hd in _context.HomeDefines
                      where hids.Contains(hd.ID)
                      select hd);
        }

        /// GET: /HomeDefines(:id)
        /// <summary>
        /// Adds support for getting a home define by key, for example:
        /// 
        /// GET /HomeDefines(1)
        /// </summary>
        /// <param name="id">The key of the home define required</param>
        /// <returns>The home define</returns>
        [HttpGet]
        [EnableQuery]
        public HomeDefine Get([FromODataUri] int key)
        {
            String usrName = "";
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);

                if (string.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var hids = (from hmem in _context.HomeMembers
                        where hmem.User == usrName && hmem.HomeID == key
                        select hmem.HomeID).ToList();
            if (hids.Count == 0)
            {
                throw new NotFoundException("Not found");
            }

            return (from hdef in _context.HomeDefines
                    where hdef.ID == key
                    select hdef).FirstOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]HomeDefine homedef)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            if (!homedef.IsValid(this._context))
                throw new BadRequestException("Inputted object IsValid Failed");

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

            homedef.Createdby = usrName;
            homedef.CreatedAt = DateTime.Now;
            foreach(var hmem in homedef.Members)
            {
                hmem.CreatedAt = homedef.CreatedAt;
                hmem.Createdby = usrName;
            }
            _context.HomeDefines.Add(homedef);

            await _context.SaveChangesAsync();

            return Created(homedef);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] HomeDefine update)
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == update.ID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!update.IsValid(this._context))
                throw new BadRequestException("Inputted Object IsValid Failed");

            // Find out the home define
            var existinghd = _context.HomeDefines.Find(key);

            if (existinghd == null)
            {
                throw new NotFoundException("Inputted Object Not Found");
            }
            else
            {
                update.Updatedby = usrName;
                update.UpdatedAt = DateTime.Now;
                update.CreatedAt = existinghd.CreatedAt;
                update.Createdby = existinghd.Createdby;
                _context.Entry(existinghd).CurrentValues.SetValues(update);

                var dbmems = _context.HomeMembers.Where(p => p.HomeID == key).ToList();
                foreach (var mem in update.Members)
                {
                    var memindb = dbmems.Find(p => p.HomeID == key && p.User == mem.User);
                    if (memindb == null)
                    {
                        mem.Createdby = usrName;
                        mem.CreatedAt = DateTime.Now;
                        _context.HomeMembers.Add(mem);
                    }
                    else
                    {
                        mem.CreatedAt = memindb.CreatedAt;
                        mem.Createdby = memindb.Createdby;
                        _context.Entry(memindb).CurrentValues.SetValues(mem);
                    }
                }
                foreach (var mem in dbmems)
                {
                    var nmem = update.Members.FirstOrDefault(p => p.User == mem.User);
                    if (nmem == null)
                    {
                        _context.HomeMembers.Remove(mem);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.HomeDefines.Any(p => p.ID == key))
                {
                    throw new NotFoundException("Inputted Object Not Found");
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

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == key && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            var cc = await _context.HomeDefines.FindAsync(key);
            if (cc == null)
            {
                throw new NotFoundException("Inputted Object Not Found");
            }

            // Perform the checks - no need any more
            //if (!cc.IsDeleteAllowed(this._context))
            //    throw new BadRequestException("Inputted Object IsDeleteAllowed Failed");

            // Documents, accounts, control centers and order
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_DOCUMENT WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_TMPDOC_DP WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_TMPDOC_LOAN WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_ACCOUNT WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_CONTROLCENTER WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_ORDER WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_PLAN WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_ACCOUNT_CTGY WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_ASSET_CTGY WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_DOC_TYPE WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_FIN_TRAN_TYPE WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_EVENT WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_EVENT_RECUR WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_LIB_BOOK_BORROW_RECORD WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_LIB_BOOK_DEF WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_LIB_BOOKCTGY_DEF WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE t_lib_bookloc_def WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_LIB_ORG_DEF WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_LIB_ORGTYPE_DEF WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_LIB_PERSON_DEF WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE t_lib_personrole_def WHERE HID={key}");

            await _context.Database.ExecuteSqlAsync($"DELETE T_HOMEMEM WHERE HID={key}");
            await _context.Database.ExecuteSqlAsync($"DELETE T_HOMEDEF WHERE ID={key}");


            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
