using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Models;
using Microsoft.AspNet.OData;

namespace hihapi.Controllers
{
    public sealed class FinanceReportByControlCentersController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByControlCentersController(hihDataContext context)
        {
            _context = context;
        }
    }
}
