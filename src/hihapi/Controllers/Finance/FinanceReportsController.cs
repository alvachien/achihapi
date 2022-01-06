using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using hihapi.Models;
using hihapi.Utilities;


namespace hihapi.Controllers.Finance
{
    [Authorize]
    public class FinanceReportsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportsController(hihDataContext context)
        {
            _context = context;
        }

        // Action
        [HttpPost]
        public async Task<IActionResult> GetMonthlyFigures()
        {

        }
    }
}
