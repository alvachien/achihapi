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
    public class LanguagesController : ODataController
    {
        private readonly hihDataContext _context;

        public LanguagesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /Languages
        /// <summary>
        /// Adds support for getting languages, for example:
        /// 
        /// GET /Languages
        /// GET /Languages?$filter=NativeName eq 'English'
        /// GET /Languages?
        /// 
        /// <remarks>
        [EnableQuery]
        public IQueryable<Language> Get()
        {
            return _context.Languages;
        }
    }
}
