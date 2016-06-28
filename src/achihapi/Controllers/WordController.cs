using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class WordController : Controller
    {
        public WordController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET api/word
        [HttpGet]
        public IEnumerable<WordViewModel> Get(String lang="en")
        {
            List<WordViewModel> listWords = new List<WordViewModel>();
            var poslist = from p1 in _dbContext.EnWord
                          join p2 in _dbContext.EnWordExplain
                            on p1.WordID equals p2.WordID 
                            into p12

                          from p3 in p12.DefaultIfEmpty()
                          join p4 in _dbContext.ENPOS on p3.POSAbb equals p4.POSAbb 
                          join p6 in _dbContext.EnPOST on p4.POSAbb equals p6.POSAbb 
                            into p14

                          from p7 in p14.DefaultIfEmpty()
                          join p8 in _dbContext.EnWordExplainT
                            on new { p7.LangID, p3.WordID, p3.ExplainID }  equals new { p8.LangID, p8.WordID, p8.ExplainID }
                          orderby p3.WordID, p3.ExplainID
                          select new { p3.WordID, p3.ExplainID, p1.WordString, p1.Tags, p3.POSAbb, p7.LangID, p7.POSName, p8.ExplainString }                            
                        ;

            foreach(var worddb in poslist)
            {
                WordViewModel vm = null;

                if (listWords.Count >= 1)
                {
                    if (listWords.Count == 1)
                    {
                        vm = listWords[0];
                    }
                    else
                    {
                        vm = listWords.Find(x => x.WordID == worddb.WordID);
                    }
                }

                if (vm == null)
                {
                    vm = new WordViewModel();
                    vm.WordID = worddb.WordID;
                    vm.WordString = worddb.WordString;
                }

                WordExplainViewModel expvm = null;
                //if (vm.Explains.Count >= 1)
                //{
                //    expvm = vm.Explains.Find(x => x.ExplainID == worddb.ExplainID);
                //}

                if (expvm == null)
                {
                    expvm = new WordExplainViewModel();
                    expvm.ExplainID = worddb.ExplainID;
                    expvm.LangID = worddb.LangID;
                    expvm.POSAbb = worddb.POSAbb;
                    expvm.ExplainString = worddb.ExplainString;
                    vm.Explains.Add(expvm);
                }

                listWords.Add(vm);
            }

            return listWords;
        }

        // GET api/word/5
        [HttpGet("{id}", Name = "GetWord")]
        public IActionResult Get(int id)
        {
            var word = _dbContext.EnWord.Single(x => x.WordID == id);
            if (word == null)
            {
                return NotFound();
            }

            List<WordViewModel> listWords = new List<WordViewModel>();
            var poslist = from p1 in _dbContext.EnWord
                          join p2 in _dbContext.EnWordExplain
                            on p1.WordID equals p2.WordID
                          into p12
                          from p3 in p12.DefaultIfEmpty()
                          join p4 in _dbContext.ENPOS
                            on p3.POSAbb equals p4.POSAbb
                          into p13
                          from p5 in p13.DefaultIfEmpty()
                          join p6 in _dbContext.EnPOST
                            on p5.POSAbb equals p6.POSAbb
                          into p14
                          from p7 in p14.DefaultIfEmpty()
                          join p8 in _dbContext.EnWordExplainT
                            on new { p7.LangID, p3.WordID, p3.ExplainID } equals new { p8.LangID, p8.WordID, p8.ExplainID }
                          where p1.WordID == id
                          select new { p3.WordID, p3.ExplainID, p1.WordString, p1.Tags, p3.POSAbb, p7.LangID, p7.POSName, p8.ExplainString }
                        ;
            WordViewModel vm = null;
            vm = new WordViewModel();

            if (poslist.Count() > 0)
            {
                var worddb = poslist.First();

                vm.WordID = worddb.WordID;
                vm.WordString = worddb.WordString;

                WordExplainViewModel expvm = null;
                expvm = new WordExplainViewModel();
                expvm.ExplainID = worddb.ExplainID;
                expvm.LangID = worddb.LangID;
                expvm.POSAbb = worddb.POSAbb;
                expvm.ExplainString = worddb.ExplainString;
                vm.Explains.Add(expvm);
            }

            return new ObjectResult(vm);
        }

        // POST api/word
        [HttpPost]
        //public IActionResult Create([FromBody] WordViewModel ch)
        //public async Task<IActionResult> Create([FromBody] dynamic data)
        public async Task<IActionResult> Create([FromBody] WordViewModel ch)
        {
            //JObject jObject = JObject.Parse(data);
            //Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(data));
            if (ch == null || ch.Explains.Count <= 0)
            {
                return BadRequest("No data is inputted or Explains is empty");
            }

            if (TryValidateModel(ch))
            {
                // Do nothing here
            }
            else
            {
                return BadRequest();
            }

            // Add it into the database
            EnWord word = new EnWord();
            
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    word.WordString = ch.WordString;
                    word.Tags = ch.Tags;
                    _dbContext.EnWord.Add(word);
                    _dbContext.SaveChanges();

                    Int32 i = 1;
                    foreach (var exp in ch.Explains)
                    {
                        EnWordExplain ewexp = new EnWordExplain();
                        ewexp.POSAbb = exp.POSAbb;
                        ewexp.ExplainID = i++;
                        ewexp.WordID = word.WordID;
                        _dbContext.EnWordExplain.Add(ewexp);

                        EnWordExplainT ewexpt = new EnWordExplainT();
                        ewexpt.ExplainID = ewexp.ExplainID;
                        ewexpt.LangID = exp.LangID;
                        ewexpt.WordID = word.WordID;
                        ewexpt.ExplainString = exp.ExplainString;
                        _dbContext.EnWordExplainT.Add(ewexpt);
                    }
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

            return CreatedAtRoute("GetWord", new { controller = "Word", id = word.WordID }, ch);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]WordViewModel ch)
        {
            if (ch == null || ch.WordID != id)
            {
                return BadRequest();
            }

            var oldCH = _dbContext.EnWord.Single(chit => chit.WordID== id);
            if (oldCH == null)
            {
                return NotFound();
            }

            //_dbContext.CommandHit.Update(ch);
            _dbContext.SaveChangesAsync().Wait();

            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var word = _dbContext.EnWord.Single(x => x.WordID == id);
            if (word == null)
            {
                return NotFound();
            }

            _dbContext.EnWord.Remove(word);
            _dbContext.SaveChangesAsync().Wait();

            return new NoContentResult();
        }
    }
}
