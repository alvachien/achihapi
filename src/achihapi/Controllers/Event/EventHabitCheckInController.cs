using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/EventHabitCheckIn")]
    public class EventHabitCheckInController : Controller
    {
        // GET: api/EventHabitCheckIn
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/EventHabitCheckIn/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/EventHabitCheckIn
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/EventHabitCheckIn/5
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
