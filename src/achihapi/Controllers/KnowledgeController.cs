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
    public class KnowledgeController : Controller
    {
        public KnowledgeController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET: api/knowledge
        [HttpGet]
        public IEnumerable<KnowledgeViewModel> Get()
        {
            List<KnowledgeViewModel> vms = new List<KnowledgeViewModel>();
            var dbs = from kl in _dbContext.Knowledge
                      join kt in _dbContext.KnowledgeType
                        on kl.ContentType equals kt.Id
                      into klt
                      from kl2 in klt.DefaultIfEmpty()
                      select new { kl.Id, kl.ContentType, ContentTypeName = kl2.Name, ContentTypeParent = kl2.ParentId, kl.Title, kl.Content, kl.Tags, kl.CreatedAt, kl.ModifiedAt };

            foreach(var db in dbs)
            {
                KnowledgeViewModel vm = new KnowledgeViewModel();
                vm.Content = db.Content;
                vm.CreatedAt = db.CreatedAt;
                vm.ID = db.Id;
                vm.ModifiedAt = db.ModifiedAt;
                vm.Title = db.Title;
                vm.TypeID = db.ContentType;

                vms.Add(vm);
            }

            return vms;
        }

        // GET api/knowledge/5
        [HttpGet("{id}", Name = "GetKnowledge")]
        public IActionResult Get(int id)
        {
            var dbs = from kl in _dbContext.Knowledge
                      join kt in _dbContext.KnowledgeType
                        on kl.ContentType equals kt.Id
                      into klt
                      from kl2 in klt.DefaultIfEmpty()
                      where kl.Id == id
                      select new { kl.Id, kl.ContentType, ContentTypeName = kl2.Name, ContentTypeParent = kl2.ParentId, kl.Title, kl.Content, kl.Tags, kl.CreatedAt, kl.ModifiedAt };
            var db = dbs.FirstOrDefault();
            if (db == null)
                return NotFound();

            KnowledgeViewModel vm = new KnowledgeViewModel();
            vm.Content = db.Content;
            vm.CreatedAt = db.CreatedAt;
            vm.ID = db.Id;
            vm.ModifiedAt = db.ModifiedAt;
            vm.Title = db.Title;
            vm.TypeID = db.ContentType;

            return new ObjectResult(vm);
        }

        // POST api/knowledge
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]KnowledgeViewModel vm)
        {
            // Create a new knowledge
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
            Knowledge word = new Knowledge();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //word.Id = vm.ID;
                    word.Content = vm.Content;
                    if (vm.TypeID.HasValue)
                    {
                        word.ContentType = (short) vm.TypeID.Value;
                    } 
                    else
                    {
                        word.ContentType = null;
                    }
                    word.Title = vm.Title;
                    word.ModifiedAt = DateTime.Now;
                    word.CreatedAt = word.ModifiedAt;
                    word.Tags = vm.Tags;
                    _dbContext.Knowledge.Add(word);
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

            return CreatedAtRoute("GetKnowledge", new { controller = "Knowledge", id = word.Id }, word);
        }

        // PUT api/knowledge/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]KnowledgeViewModel vm)
        {
            var db = _dbContext.Knowledge.Single(x => x.Id == id);
            if (db == null)
            {
                return NotFound();
            }

            db.Content = vm.Content;
            if (vm.TypeID.HasValue)
            {
                db.ContentType = (short)vm.TypeID.Value;
            }
            else
            {
                db.ContentType = null;
            }
            db.ModifiedAt = DateTime.Now;
            db.CreatedAt = vm.CreatedAt;
            db.Tags = vm.Tags;
            _dbContext.Knowledge.Update(db);
            _dbContext.SaveChangesAsync().Wait();

            return new NoContentResult();
        }

        // DELETE api/knowledge/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var db = _dbContext.Knowledge.Single(x => x.Id == id);
            if (db == null)
            {
                return NotFound();
            }

            _dbContext.Knowledge.Remove(db);
            _dbContext.SaveChangesAsync().Wait();

            return new NoContentResult();
        }
    }
}
