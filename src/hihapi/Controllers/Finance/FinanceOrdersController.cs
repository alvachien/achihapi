using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    public sealed class FinanceOrdersController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceOrdersController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceOrders
        [Authorize]
        public IQueryable Get(ODataQueryOptions<FinanceOrder> option)
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
            var query = from hmem in _context.HomeMembers
                        where hmem.User == usrName
                        select new { HomeID = hmem.HomeID } into hids
                        join ords in _context.FinanceOrder on hids.HomeID equals ords.HomeID
                        select ords;

            return option.ApplyTo(query);
        }

        // The Route will never reach following codes...
        // 
        //[EnableQuery]
        //[Authorize]
        //public SingleResult<FinanceOrder> Get([FromODataUri]Int32 ordid)
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
        //    var ordquery = from ord in _context.FinanceOrder
        //                    where ord.ID == ordid
        //                    select ord;
        //    var rstquery = from ord in ordquery
        //                   join hid in hidquery
        //                   on ord.HomeID equals hid.HomeID
        //                   select ord;

        //    return SingleResult.Create(rstquery);
        //}

        [Authorize]
        public async Task<IActionResult> Post([FromBody] FinanceOrder order)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            // Check
            if (!order.IsValid(this._context))
            {
                throw new BadRequestException("Inputted Object IsValid failed");
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
            var hms = _context.HomeMembers.Where(p => p.HomeID == order.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            order.CreatedAt = DateTime.Now;
            order.Createdby = usrName;
            _context.FinanceOrder.Add(order);
            await _context.SaveChangesAsync();

            return Created(order);
        }

        [Authorize]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] FinanceOrder update)
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

            // SRules.
            var rulesInDB = _context.FinanceOrderSRule.Where(p => p.OrderID == update.ID).ToList();
            foreach (var rule in update.SRule)
            {
                var itemindb = rulesInDB.Find(p => p.OrderID == update.ID && p.RuleID == rule.RuleID);
                if (itemindb == null)
                {
                    _context.FinanceOrderSRule.Add(rule);
                }
                else
                {
                    // Update
                    _context.Entry(itemindb).State = EntityState.Modified;
                }
            }
            foreach (var rule in rulesInDB)
            {
                var nitem = update.SRule.FirstOrDefault(p => p.OrderID == update.ID && p.RuleID == rule.RuleID);
                if (nitem == null)
                {
                    _context.FinanceOrderSRule.Remove(rule);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exp)
            {
                if (!_context.FinanceOrder.Any(p => p.ID == key))
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
        public async Task<IActionResult> Patch([FromODataUri] int id, [FromBody] Delta<FinanceOrder> doc)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            var entity = await _context.FinanceOrder.FindAsync(id);
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
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // Patch it
            doc.Patch(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FinanceOrder.Any(p => p.ID == id))
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
            var cc = await _context.FinanceOrder.FindAsync(key);
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
                throw new BadRequestException("Inputted Object IsDeleteAllowed failed");

            _context.FinanceOrder.Remove(cc);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
