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
        // GET api/values
        [HttpGet]
        public IEnumerable<POSViewModel> Get()
        {
            return null;
        }
    }
}
