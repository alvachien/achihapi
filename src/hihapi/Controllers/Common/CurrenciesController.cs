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
using hihapi.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class CurrenciesController : ODataController
    {
        private readonly hihDataContext _context;

        public CurrenciesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /Currencies
        /// <summary>
        /// Adds support for getting currencies, for example:
        /// 
        /// GET /Currencies
        /// GET /Currencies?$filter=Curr eq 'Dollar'
        /// GET /Currencies?
        /// 
        /// <remarks>
        [EnableQuery]
        [ResponseCache(Duration = 3600)]
        public IActionResult Get()
        {
            return Ok(_context.Currencies);
        }

        /// GET: /Currencies(:id)
        /// <summary>
        /// Adds support for getting a currency by key, for example:
        /// 
        /// GET /Currencies(1)
        /// </summary>
        /// <param name="key">The key of the currency required</param>
        /// <returns>The currency</returns>
        [EnableQuery]
        public IActionResult Get(string key)
        {
            return Ok(_context.Currencies.FirstOrDefault(p => p.Curr == key));
        }
    }
}
