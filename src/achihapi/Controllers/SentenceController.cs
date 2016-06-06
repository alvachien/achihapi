using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class SentenceController : Controller
    {
        public SentenceController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET api/sentence
        [HttpGet]
        public IEnumerable<SentenceViewModel> Get()
        {
            List<SentenceViewModel> listSents = new List<SentenceViewModel>();
            var poslist = from p1 in _dbContext.EnSentence
                          join p2 in _dbContext.EnSentenceExplain
                            on p1.SentenceID equals p2.SentenceID into p12
                          from p3 in p12.DefaultIfEmpty()
                          join p8 in _dbContext.EnSentenceExplainT
                            on new { p3.SentenceID, p3.ExplainID } equals new { p8.SentenceID, p8.ExplainID }
                          orderby p3.SentenceID, p3.ExplainID, p8.LangID
                          select new { p3.SentenceID, p3.ExplainID, p1.SentenceString, p1.Tags, p8.LangID, p8.ExplainString }
                        ;
            foreach (var sentdb in poslist)
            {
                SentenceViewModel vm = null;

                if (listSents.Count >= 1)
                {
                    if (listSents.Count == 1)
                    {
                        vm = listSents[0];
                    }
                    else
                    {
                        vm = listSents.Find(x => x.SentenceID == sentdb.SentenceID);
                    }
                }

                if (vm == null)
                {
                    vm = new SentenceViewModel();
                    vm.SentenceID = sentdb.SentenceID;
                    vm.SentenceString = sentdb.SentenceString;
                }

                SentenceExplainViewModel expvm = null;
                if (expvm == null)
                {
                    expvm = new SentenceExplainViewModel();
                    expvm.ExplainID = sentdb.ExplainID;
                    expvm.LangID = sentdb.LangID;
                    expvm.ExplainString = sentdb.ExplainString;
                    vm.Explains.Add(expvm);
                }

                listSents.Add(vm);
            }

            return listSents;
        }

        // GET api/sentence/5
        [HttpGet("{id}", Name = "GetSentence")]
        public IActionResult Get(int id)
        {
            var word = _dbContext.EnSentence.Single(x => x.SentenceID == id);
            if (word == null)
            {
                return NotFound();
            }

            List<SentenceViewModel> listSents = new List<SentenceViewModel>();
            var poslist = from p1 in _dbContext.EnSentence
                          join p2 in _dbContext.EnSentenceExplain
                            on p1.SentenceID equals p2.SentenceID into p12
                          from p3 in p12.DefaultIfEmpty()
                          join p8 in _dbContext.EnSentenceExplainT
                            on new { p3.SentenceID, p3.ExplainID } equals new { p8.SentenceID, p8.ExplainID }
                          orderby p3.SentenceID, p3.ExplainID, p8.LangID
                          where p1.SentenceID == id
                          select new { p3.SentenceID, p3.ExplainID, p1.SentenceString, p1.Tags, p8.LangID, p8.ExplainString }
                        ;
            SentenceViewModel vm = null;
            vm = new SentenceViewModel();

            if (poslist.Count() > 0)
            {
                var sentdb = poslist.First();

                vm.SentenceID = sentdb.SentenceID;
                vm.SentenceString = sentdb.SentenceString;

                SentenceExplainViewModel expvm = null;
                expvm = new SentenceExplainViewModel();
                expvm.ExplainID = sentdb.ExplainID;
                expvm.LangID = sentdb.LangID;
                expvm.ExplainString = sentdb.ExplainString;
                vm.Explains.Add(expvm);
            }

            return new ObjectResult(vm);
        }

        // POST api/sentence
        [HttpPost]
        //public IActionResult Create([FromBody] WordViewModel ch)
        //public async Task<IActionResult> Create([FromBody] dynamic data)
        public async Task<IActionResult> Create([FromBody] SentenceViewModel ch)
        {
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
            EnSentence sent = new EnSentence();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    sent.SentenceString = ch.SentenceString;
                    sent.Tags = ch.Tags;
                    _dbContext.EnSentence.Add(sent);
                    _dbContext.SaveChanges();

                    Int32 i = 1;
                    foreach (var exp in ch.Explains)
                    {
                        EnSentenceExplain ewexp = new EnSentenceExplain();
                        ewexp.ExplainID = i++;
                        ewexp.SentenceID = sent.SentenceID;
                        _dbContext.EnSentenceExplain.Add(ewexp);

                        EnSentenceExplainT ewexpt = new EnSentenceExplainT();
                        ewexpt.ExplainID = ewexp.ExplainID;
                        ewexpt.LangID = exp.LangID;
                        ewexpt.SentenceID = sent.SentenceID;
                        ewexpt.ExplainString = exp.ExplainString;
                        _dbContext.EnSentenceExplainT.Add(ewexpt);
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

            return CreatedAtRoute("GetSentence", new { controller = "Sentence", id = sent.SentenceID }, ch);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sent = _dbContext.EnSentence.Single(x => x.SentenceID == id);
            if (sent == null)
            {
                return NotFound();
            }

            _dbContext.EnSentence.Remove(sent);
            _dbContext.SaveChangesAsync().Wait();

            return new NoContentResult();
        }
    }
}
