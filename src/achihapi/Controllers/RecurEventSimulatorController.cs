using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/RecurEventSimulator")]
    public class RecurEventSimulatorController : Controller
    {
        // GET: api/RecurEventSimulator
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/RecurEventSimulator/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/RecurEventSimulator
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/RecurEventSimulator/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
