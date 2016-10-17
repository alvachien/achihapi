using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class ModuleController : Controller
    {
        // GET: api/module
        [HttpGet]
        public IEnumerable<ModuleViewModel> Get()
        {
            List<ModuleViewModel> listVMs = new List<ModuleViewModel>();
            return listVMs;
        }

        // GET api/module/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/module
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/module/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/module/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
