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
using hihapi.Utilities;
using Microsoft.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace hihapi.Controllers
{
    public class HomeDefinesController : ODataController
    {
        private readonly hihDataContext _context;
        // public static string SubjectId(this ClaimsPrincipal user) { 
        //     return user?.Claims?.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value; 
        // }

        public HomeDefinesController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /HomeDefines
        /// <summary>
        /// Adds support for getting home def., for example:
        /// 
        /// GET /HomeDefines
        /// GET /HomeDefines?$filter=Host eq 'abc'
        /// GET /HomeDefines?
        /// 
        /// <remarks>
        [EnableQuery]
        [Authorize]
        public IActionResult Get()
        {
            DbSet<HomeMember> listRst = null;

            String scopeFilter = String.Empty;

            String usrName = "";
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                //var usrObj = HIHAPIUtility.GetUserClaim(this);
                //usrName = usrObj.Value;

                // Disabled scope check just make it work, 2017.10.1
                scopeFilter = usrName;

                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                //if (String.IsNullOrEmpty(scopeFilter))
                //    scopeFilter = usrName;
                if (String.IsNullOrEmpty(scopeFilter))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var rst = (from hd in _context.HomeDefines
                    join hm in _context.HomeMembers
                        on hd.ID equals hm.HomeID
                    where hm.User == scopeFilter
                    select hd);

            return Ok(rst);
        }
    }
}
