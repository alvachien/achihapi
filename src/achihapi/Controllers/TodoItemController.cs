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
            List<TodoItemViewModel> todos = new List<TodoItemViewModel>();
            var tdis = from p1 in _dbContext.TodoItem
                          select p1;
            foreach(var tdi in tdis)
            {
                TodoItemViewModel vm = new TodoItemViewModel(tdi);
                todos.Add(vm);
            }

            return todos;
        }

        // GET api/todoitem/5
        [HttpGet("{id}", Name = "GetTodoItem")]
        public IActionResult Get(int id)
        {
            var tdi = _dbContext.TodoItem.Single(x => x.ToDoID == id);
            if (tdi == null)
            {
                return NotFound();
            }

            TodoItemViewModel vm = new TodoItemViewModel(tdi);

            return new ObjectResult(vm);
        }

        // POST api/todoitem
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]TodoItemViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted or Explains is empty");
            }

            if (TryValidateModel(vm))
            {
                // Do nothing here
            }
            else
            {
                return BadRequest();
            }

            TodoItem tdi = new TodoItem();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    tdi = vm.Convert2DB();
                    _dbContext.TodoItem.Add(tdi);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    Console.WriteLine(exp.Message);
#endif

                    transaction.Rollback();
                    return BadRequest();
                }
            }

            return CreatedAtRoute("GetTodoItem", new { controller = "TodoItem", id = vm.TodoID}, vm);
        }

        // PUT api/todoitem/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]string value)
        {
            var tdi = _dbContext.TodoItem.Single(x => x.ToDoID == id);
            if (tdi == null)
            {
                return NotFound();
            }

            // Todo

            return new NoContentResult();
        }

        // DELETE api/todoitem/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tdi = _dbContext.TodoItem.Single(x => x.ToDoID == id);
            if (tdi == null)
            {
                return NotFound();
            }

            return new NoContentResult();
        }
    }
}
