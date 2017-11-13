using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LearnEnWord")]
    [Authorize]
    public class LearnEnWordController : Controller
    {
        // GET: api/LearnEnWord
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LearnEnWord/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/LearnEnWord
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/LearnEnWord/5
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
