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
                          select new { POSAbb = item.POSAbb, POSNativeName = item.POSName, LangID = item.LangID, POSName = p1.POSName };

            List<POSViewModel> listPOS = new List<POSViewModel>();
            try
            {
                foreach (var pos2 in poslist)
                {
                    POSViewModel vm = new POSViewModel();
                    vm.POSAbb = pos2.POSAbb;
                    vm.POSName = pos2.POSName;
                    vm.LangID = pos2.LangID;
                    vm.POSNativeName = pos2.POSNativeName;

                    listPOS.Add(vm);
                }
            }
            catch(Exception exp)
            {
                // Do nothing!
            }

            // 1. First Try
            //for (int i = 0; i < poslist.Count(); i++)
            //{
            //    POSViewModel vm = new POSViewModel();
            //    vm.POSAbb = poslist.ElementAt(i).POSAbb;
            //    vm.POSName = poslist.ElementAt(i).POSName;
            //    vm.LangID = poslist.ElementAt(i).LangID;
            //    vm.POSNativeName = poslist.ElementAt(i).POSNativeName;

            //    listPOS.Add(vm);
            //}
            // 2. Second Try
            //poslist.ToList().ForEach(pos =>
            //{
            //    POSViewModel vm = new POSViewModel();
            //    vm.POSAbb = pos.POSAbb;
            //    vm.POSName = pos.POSName;
            //    vm.LangID = pos.LangID;
            //    vm.POSNativeName = pos.POSNativeName;

            //    listPOS.Add(vm);
            //});
            return listPOS;
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
