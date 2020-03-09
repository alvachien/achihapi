using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Models;
using Microsoft.AspNet.OData;

namespace hihapi.Controllers
{
    public sealed class FinanceReportByOrdersController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByOrdersController(hihDataContext context)
        {
            _context = context;
        }
    }
}
