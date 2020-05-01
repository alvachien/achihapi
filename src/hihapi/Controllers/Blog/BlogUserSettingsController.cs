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
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace hihapi.Controllers
{
    public class BlogUserSettingsController : ODataController
    {
        private readonly hihDataContext _context;

        public BlogUserSettingsController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        [EnableQuery]
        public IQueryable<BlogUserSetting> Get()
        {
            return _context.BlogUserSettings;
        }
    }
}