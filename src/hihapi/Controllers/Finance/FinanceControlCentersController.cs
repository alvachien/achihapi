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

namespace hihapi.Controllers
{
    public sealed class FinanceControlCentersController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceControlCentersController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /FinanceControlCenters
        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceControlCenter> Get(Int32 hid)
        {
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
            }
            catch
            {
                throw new ODataError();
            }

            if (String.IsNullOrEmpty(usrName))
            {
                throw new Exception("Unauthorized");
            }

            var rst =
                from hmem in _context.HomeMembers.Where(p => p.User == usrName && p.HomeID == hid)
                from acntctgy in _context.FinanceControlCenter.Where(p => p.HomeID == hmem.HomeID)
                select acntctgy;

            return rst;
        }
    
        public async Task<IActionResult> Post([FromBody]FinanceControlCenter controlCenter)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach(var err in value.Errors) 
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }

            // User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
            }
            catch
            {
                // Do nothing
            }

            if (String.IsNullOrEmpty(usrName))
            {
                throw new Exception("Unauthorized");
            }

            // Check whether User assigned with specified Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == controlCenter.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new Exception("Unauthorized");
            }

            _context.FinanceControlCenter.Add(controlCenter);
            await _context.SaveChangesAsync();

            return Created(controlCenter);
        }
    }
}
