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
    public class DBVersionsController: ODataController
    {
        private readonly hihDataContext _context;

        public DBVersionsController(hihDataContext context)
        {
            _context = context;
        }
        
        /// GET: /DBVersions
        [EnableQuery]
        public IQueryable<DBVersion> Get()
        {
            return _context.DBVersions;
        }
    }
}
