using hihapi.Models;
using Microsoft.AspNet.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    public sealed class FinanceReportByAccountsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByAccountsController(hihDataContext context)
        {
            _context = context;
        }
    }
}
