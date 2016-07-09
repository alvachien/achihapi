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

            return new ObjectResult(vm);
        }

        // POST api/knowledgetype
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return BadRequest();
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
            return BadRequest();
        }
    }
}
