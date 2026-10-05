using System;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

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

            // Restricted ("lite") members only see plans whose target is one of their
            // own accounts / control centers; home-level plans (account category,
            // transaction type) stay invisible to them.
            var ownAcntIDs = _context.FinanceAccount
                .Where(acnt => acnt.Owner == usrName)
                .Select(acnt => acnt.ID);
            var ownCcIDs = _context.FinanceControlCenter
                .Where(cc => cc.Owner == usrName)
                .Select(cc => cc.ID);

            // Check whether User assigned with specified Home ID
            return Ok(from hmem in _context.HomeMembers
                      where hmem.User == usrName
                      select new { HomeID = hmem.HomeID, IsLite = hmem.IsLite } into hids
                      join ords in _context.FinancePlan on hids.HomeID equals ords.HomeID
                      where !hids.IsLite.HasValue
                          || hids.IsLite == false
                          || (ords.PlanType == FinancePlanTypeEnum.Account && ords.AccountID != null && ownAcntIDs.Contains(ords.AccountID.Value))
                          || (ords.PlanType == FinancePlanTypeEnum.ControlCenter && ords.ControlCenterID != null && ownCcIDs.Contains(ords.ControlCenterID.Value))
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

            // Same lite-scope as the collection query above: a restricted member must
            // not reach another member's plan by guessing its key.
            var ownAcntIDs = _context.FinanceAccount
                .Where(acnt => acnt.Owner == usrName)
                .Select(acnt => acnt.ID);
            var ownCcIDs = _context.FinanceControlCenter
                .Where(cc => cc.Owner == usrName)
                .Select(cc => cc.ID);

            // Check whether User assigned with specified Home ID
            return SingleResult.Create(from hmem in _context.HomeMembers
                                       where hmem.User == usrName
                                       select new { HomeID = hmem.HomeID, IsLite = hmem.IsLite } into hids
                                       join ords in _context.FinancePlan on hids.HomeID equals ords.HomeID
                                       where ords.ID == key
                                          && (!hids.IsLite.HasValue
                                              || hids.IsLite == false
                                              || (ords.PlanType == FinancePlanTypeEnum.Account && ords.AccountID != null && ownAcntIDs.Contains(ords.AccountID.Value))
                                              || (ords.PlanType == FinancePlanTypeEnum.ControlCenter && ords.ControlCenterID != null && ownCcIDs.Contains(ords.ControlCenterID.Value)))
                                       select ords);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FinancePlan plan)
        {
            if (!ModelState.IsValid)
                HIHAPIUtility.HandleModelStateError(ModelState);

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

            // Check whether User assigned with specified Home ID; restricted ("lite")
            // members may not create plans - a grown-up sets the goal for them.
            var mem = await (from hmem in _context.HomeMembers
                             where hmem.HomeID == plan.HomeID && hmem.User == usrName
                             select new { hmem.IsLite }).FirstOrDefaultAsync();
            if (mem == null || mem.IsLite == true)
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
                HIHAPIUtility.HandleModelStateError(ModelState);

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

            // Find the existing record first - membership is checked against the EXISTING home,
            // not the HomeID in the request body (prevents cross-tenant mass-assignment).
            var existing = await _context.FinancePlan.FindAsync(key);
            if (existing == null)
            {
                throw new NotFoundException("Inputted ID not found");
            }

            // Check whether User assigned with the existing Home ID; restricted ("lite")
            // members may not change plans.
            var mem = await (from hmem in _context.HomeMembers
                             where hmem.HomeID == existing.HomeID && hmem.User == usrName
                             select new { hmem.IsLite }).FirstOrDefaultAsync();
            if (mem == null || mem.IsLite == true)
            {
                throw new UnauthorizedAccessException();
            }

            // Reject HomeID changes via PUT
            if (update.HomeID != existing.HomeID)
            {
                throw new BadRequestException("HomeID cannot be changed via PUT.");
            }

            if (!update.IsValid(this._context))
                throw new BadRequestException("Inputted Object IsValid failed");

            update.CreatedAt = existing.CreatedAt;
            update.Createdby = existing.Createdby;
            update.UpdatedAt = DateTime.Now;
            update.Updatedby = usrName;
            _context.Entry(existing).CurrentValues.SetValues(update);
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

            // Check whether User assigned with specified Home ID; restricted ("lite")
            // members may not delete plans.
            var mem = await (from hmem in _context.HomeMembers
                             where hmem.HomeID == cc.HomeID && hmem.User == usrName
                             select new { hmem.IsLite }).FirstOrDefaultAsync();
            if (mem == null || mem.IsLite == true)
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
