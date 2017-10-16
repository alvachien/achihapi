﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Produces("application/json")]
    [Route("api/LibMovie")]
    public class LibMovieController : Controller
    {
        // GET: api/LibMovie
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LibMovie/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/LibMovie
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/LibMovie/5
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