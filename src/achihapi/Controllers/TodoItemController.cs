using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class TodoItemController : Controller
    {
        public TodoItemController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET: api/todoitem
        [HttpGet]
        public IEnumerable<TodoItemViewModel> Get()
        {
            return new List<TodoItemViewModel>();
        }

        // GET api/todoitem/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/todoitem
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/todoitem/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/todoitem/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
