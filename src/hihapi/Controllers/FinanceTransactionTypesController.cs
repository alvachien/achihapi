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
using hihapi.Models;

namespace hihapi.Controllers
{
    public class FinanceTransactionTypesController: ODataController
    {        
        private readonly hihDataContext _context;
        
        public FinanceTransactionTypesController(hihDataContext context)
        {
            _context = context;
        }
        
        /// GET: /FinanceAssertCategories
        [EnableQuery]
        public IQueryable<FinanceTransactionType> Get()
        {
            return _context.FinTransactionType;
        }
    }
}
