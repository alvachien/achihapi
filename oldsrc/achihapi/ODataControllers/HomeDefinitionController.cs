using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using achihapi.Controllers;
using achihapi.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using achihapi.ViewModels;
using Microsoft.Data.OData;

namespace achihapi.ODataControllers
{
    public class HomeDefinitionController: ODataController
    {
        private readonly achihdbContext _context;
        private IMemoryCache _cache;

        public HomeDefinitionController(achihdbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [EnableQuery]
        public IQueryable<THomedef> Get()
        {
            DbSet<THomedef> listRst;

            String scopeFilter = String.Empty;

            String usrName = "";
            try
            {
                var usrObj = HIHAPIUtility.GetUserClaim(this);
                usrName = usrObj.Value;

                // Disabled scope check just make it work, 2017.10.1
                scopeFilter = usrName;

                //var scopeObj = HIHAPIUtility.GetScopeClaim(this, HIHAPIConstants.HomeDefScope);

                //scopeFilter = HIHAPIUtility.GetScopeSQLFilter(scopeObj.Value, usrName);
                //if (String.IsNullOrEmpty(scopeFilter))
                //    scopeFilter = usrName;
            }
            catch
            {
                scopeFilter = String.Empty;
            }

            if (String.IsNullOrEmpty(scopeFilter))
            {
                var err = new ODataError();
                err.ErrorCode = "400";
                err.Message = "Not valid HTTP HEAD: User and Scope Failed!";
                throw new ODataErrorException(err);
            }

            var rst = (from hd in _context.THomedef
                    join hm in _context.THomemem 
                        on hd.Id equals hm.Hid
                    where hm.User == scopeFilter
                    select hd);
            return rst;

            //var cacheKey = String.Format(CacheKeys.HomeDefList, scopeFilter);
            //if (_cache.TryGetValue<DbSet<THomedef>>(cacheKey, out listRst))
            //{
            //    // DO nothing!
            //}
            //else
            //{
            //    //listRst = _context.THomedef.Find(p => .
            //    // var knowledge = await _context.THomemem.();
            //    listRst = (from hd in _context.THomedef
            //               join hm in _context.THomemem on hd.Id equals hm.Hid 
            //               select hd);
            //    _cache.Set<DbSet<THomedef>>(cacheKey, listRst, TimeSpan.FromMinutes(10));
            //}
            //return listRst;
        }

        [EnableQuery]
        public SingleResult<THomedef> Get([FromODataUri] int key)
        {
            return SingleResult.Create(_context.THomedef.Where(p => p.Id == key));
        }

    }
}
