using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class POSController : Controller
    {
        public POSController(alvachiendbContext context)
        {
            _dbContext = context;
        }

        private alvachiendbContext _dbContext = null;

        // GET api/values
        [HttpGet]
        public IEnumerable<POSViewModel> Get()
        {
            var poslist = from p1 in _dbContext.ENPOS
                          join p2 in _dbContext.EnPOST on p1.POSAbb equals p2.POSAbb
                          into view
                          from item in view.DefaultIfEmpty()                          
                          select new POSViewModel { POSAbb = item.POSAbb, POSName = item.POSName, LangID = item.LangID, POSNativeName = p1.POSName };
            return poslist;
        }
    }
}
