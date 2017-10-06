using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/FinanceLoanDocument")]
    public class FinanceLoanDocumentController : Controller
    {
        // GET: api/FinanceLoanDocument
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            return BadRequest();
        }

        // GET: api/FinanceLoanDocument/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id, [FromQuery]Int32 hid = 0)
        {
            if (hid <= 0)
                return BadRequest("Not HID inputted");

            var usrObj = HIHAPIUtility.GetUserClaim(this);
            var usrName = usrObj.Value;
            if (String.IsNullOrEmpty(usrName))
                return BadRequest("User info cannot fetch");
            return BadRequest();
        }
        
        // POST: api/FinanceLoanDocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]string value)
        {
            return BadRequest();
        }
        
        // PUT: api/FinanceLoanDocument/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody]string value)
        {
            return BadRequest();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            return BadRequest();
        }
    }
}
