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
                         orderby kt.ParentId
                         select kt
                         ;
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
        public IActionResult Get(Int16 id)
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
                // Validate the ID
                if (vm.ID == 0 || vm.ID == -1)
                {
                    return BadRequest("ID is invalid");
                }

                // Check existence
                Boolean bExists = _dbContext.KnowledgeType.Any(x => x.Id == vm.ID);
                if (bExists)
                {
                    return BadRequest("ID exists already");
                }
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
                    db.Id = vm.ID;
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
                    return BadRequest(exp.Message);
                }
            }

            return CreatedAtRoute("GetKnowledgeType", new { id = db.Id });
        }

        // PUT api/knowledgetype/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Int16 id, [FromBody]KnowledgeTypeViewModel vm)
        {
            // Create a new knowledge type
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
                // Verify the existence
                Boolean bExists = _dbContext.KnowledgeType.Any(x => x.Id == vm.ID);
                if (!bExists)
                    return BadRequest("ID not exists!");
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
                    db.Id = vm.ID;
                    db.Name = vm.Name;
                    if (vm.ParentID.HasValue && vm.ParentID != 0)
                        db.ParentId = (short)vm.ParentID.Value;
                    else
                        db.ParentId = null;
                    db.Comment = vm.Comment;
                    _dbContext.KnowledgeType.Update(db);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    Console.WriteLine(exp.Message);
#endif

                    transaction.Rollback();
                    return BadRequest(exp.Message);
                }
            }

            //return Ok(vm);
            return new ObjectResult(vm);
        }

        // DELETE api/knowledgetype/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Int16 id)
        {
            var dbkt = _dbContext.KnowledgeType.Single(x => x.Id == id);
            if (dbkt == null)
            {
                return NotFound();
            }

            _dbContext.KnowledgeType.Remove(dbkt);
            _dbContext.SaveChangesAsync().Wait();

            return Ok(id);
        }
    }
}
