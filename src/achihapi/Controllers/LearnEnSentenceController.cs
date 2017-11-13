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
    [Route("api/LearnEnSentence")]
    [Authorize]
    public class LearnEnSentenceController : Controller
    {
        // GET: api/LearnEnSentence
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LearnEnSentence/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/LearnEnSentence
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/LearnEnSentence/5
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
