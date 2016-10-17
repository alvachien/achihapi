using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class FinanceAccountCategoryController : Controller
    {
        // GET: api/financeaccountcateogry
        [HttpGet]
        public IEnumerable<FinanceAccountCtgyViewModel> Get()
        {
            List<FinanceAccountCtgyViewModel> listVm = new List<FinanceAccountCtgyViewModel>();

            return listVm;
        }

        // GET api/financeaccountcateogry/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/financeaccountcateogry
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/financeaccountcateogry/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/financeaccountcateogry/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
