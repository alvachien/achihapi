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
        public POSController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET api/pos
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

        // GET api/pos/adj.
        [HttpGet("{id}", Name = "GetPOS")]
        public IActionResult Get(String id)
        {
            var poslist = from p1 in _dbContext.ENPOS
                          join p2 in _dbContext.EnPOST on p1.POSAbb equals p2.POSAbb                          
                          into view
                          from item in view.DefaultIfEmpty()
                          where p1.POSAbb == id
                          select new POSViewModel { POSAbb = item.POSAbb, POSName = item.POSName, LangID = item.LangID, POSNativeName = p1.POSName }
                          ;

            if (poslist.Count() <= 0)
            {
                return NotFound();
            }

            return new ObjectResult(poslist.First());
        }
    }
}
