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
using Microsoft.AspNetCore.OData.Deltas;

namespace hihapi.Controllers
{
    public sealed class FinanceAccountsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceAccountsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceAccounts
        [Authorize]
        public IQueryable Get(ODataQueryOptions<FinanceAccount> option)
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
            var query = from hmem in _context.HomeMembers where hmem.User == usrName
                        select new { hmem.HomeID, hmem.User, hmem.IsChild } into hmems
                        join acnts in _context.FinanceAccount
                          on hmems.HomeID equals acnts.HomeID
                        where ( hmems.IsChild == true && hmems.User == acnts.Owner )
                            || !hmems.IsChild.HasValue
                            || hmems.IsChild == false
                        select acnts;

#if DEBUG
            // For testing purpose
            //var query1 = from hmem in _context.HomeMembers
            //             where hmem.User == usrName
            //             select hmem;
            //var query1rst = query1.ToList<HomeMember>();

            //var query2 = from hmem in query1                         
            //             join acnts in _context.FinanceAccount
            //               on hmem.HomeID equals acnts.HomeID
            //             where (hmem.IsChild == true && hmem.User == acnts.Owner) 
            //                   || !hmem.IsChild.HasValue
            //                   || hmem.IsChild == false
            //             select acnts;

            //var queryrst = query2.ToList<FinanceAccount>();
            //if (queryrst.Count <= 0)
            //{
            //}
#endif

            return option.ApplyTo(query);
        }

        // The Route will never reach following codes...
        // 
        //[EnableQuery]
        //[Authorize]
        //public SingleResult<FinanceAccount> Get([FromODataUri]Int32 acntid)
        //{
        //    String usrName = String.Empty;
        //    try
        //    {
        //        usrName = HIHAPIUtility.GetUserID(this);
        //        if (String.IsNullOrEmpty(usrName))
        //            throw new UnauthorizedAccessException();
        //    }
        //    catch
        //    {
        //        throw new UnauthorizedAccessException();
        //    }

        //    var hidquery = from hmem in _context.HomeMembers
        //                   where hmem.User == usrName
        //                   select new { HomeID = hmem.HomeID };
        //    var acntquery = from acnt in _context.FinanceAccount
        //                    where acnt.ID == acntid
        //                    select acnt;
        //    var rstquery = from acnt in acntquery
        //                   join hid in hidquery
        //                   on acnt.HomeID equals hid.HomeID
        //                   select acnt;

        //    return SingleResult.Create(rstquery);
        //}

        [Authorize]
        public async Task<IActionResult> Post([FromBody]FinanceAccount account)
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == account.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            if (!account.IsValid(this._context))
                return BadRequest();

            account.CreatedAt = DateTime.Now;
            account.Createdby = usrName;
            _context.FinanceAccount.Add(account);
            await _context.SaveChangesAsync();

            return Created(account);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody]FinanceAccount update)
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

            update.Updatedby = usrName;
            update.UpdatedAt = DateTime.Now;
            _context.Entry(update).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinanceAccount.Any(p => p.ID == key))
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
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<FinanceAccount> coll)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            var entity = await _context.FinanceAccount.FindAsync(key);
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
                if (!_context.FinanceAccount.Any(p => p.ID == key))
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

        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var cc = await _context.FinanceAccount.FindAsync(key);
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

            _context.FinanceAccount.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
