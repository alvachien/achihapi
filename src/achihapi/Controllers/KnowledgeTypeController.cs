using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.Models;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class KnowledgeTypeController : Controller
    {
        public KnowledgeTypeController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET: api/knowledgetype
        [HttpGet]
        public IEnumerable<KnowledgeTypeViewModel> Get()
        {
            List<KnowledgeTypeViewModel> listVMs = new List<KnowledgeTypeViewModel>();

            var ktlist = from kt in _dbContext.KnowledgeType
                         select kt;
            foreach(var dbkt in ktlist)
            {
                KnowledgeTypeViewModel vm = new KnowledgeTypeViewModel();
                vm.ID = dbkt.Id;
                vm.Name = dbkt.Name;
                vm.ParentID = dbkt.ParentId;
                vm.Comment = dbkt.Comment;

                listVMs.Add(vm);
            }

            return listVMs;
        }

        // GET api/knowledgetype/5
        [HttpGet("{id}", Name = "GetKnowledgeType")]
        public IActionResult Get(int id)
        {
            var dbkt = _dbContext.KnowledgeType.Single(x => x.Id == id);
            if (dbkt == null)
            {
                return NotFound();
            }

            KnowledgeTypeViewModel vm = new KnowledgeTypeViewModel();
            vm.ID = dbkt.Id;
            vm.Name = dbkt.Name;
            vm.ParentID = dbkt.ParentId;
            vm.Comment = dbkt.Comment;

            return new ObjectResult(vm);
        }

        // POST api/knowledgetype
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]KnowledgeTypeViewModel vm)
        {
            // Create a new knowledge type
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
                // Do nothing here
            }
            else
            {
                return BadRequest();
            }

            // Add it into the database
            KnowledgeType db = new KnowledgeType();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    db.Name = vm.Name;
                    if (vm.ParentID.HasValue && vm.ParentID != 0)
                        db.ParentId = (short)vm.ParentID.Value;
                    else
                        db.ParentId = null;
                    db.Comment = vm.Comment;
                    _dbContext.KnowledgeType.Add(db);
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

            return CreatedAtRoute("GetKnowledgeType", new { controller = "KnowledgeType", id = db.Id }, db);
        }

        // PUT api/knowledgetype/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }

        // DELETE api/knowledgetype/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var dbkt = _dbContext.KnowledgeType.Single(x => x.Id == id);
            if (dbkt == null)
            {
                return NotFound();
            }

            _dbContext.KnowledgeType.Remove(dbkt);
            _dbContext.SaveChangesAsync().Wait();

            return new NoContentResult();
        }
    }
}
