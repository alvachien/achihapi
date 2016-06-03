using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class WordController : Controller
    {
        public WordController(alvachiendbContext context)
        {
            _dbContext = context;
        }

        private alvachiendbContext _dbContext = null;

        // GET api/word
        [HttpGet]
        public IEnumerable<WordViewModel> Get()
        {
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
                            on new { p7.LangID, p3.WordID, p3.ExplainID }  equals new { p8.LangID, p8.WordID, p8.ExplainID }
                          select new { p3.WordID, p3.ExplainID, p1.WordString, p1.Tags, p3.POSAbb, p7.LangID, p7.POSName, p8.ExplainString }                            
                        ;
            foreach(var worddb in poslist)
            {

            }
            return listWords;
        }

        // GET api/commandhist/5
        [HttpGet("{id}", Name = "GetWord")]
        public IActionResult Get(int id)
        {
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

            return new ObjectResult(poslist.First());
        }

        // POST api/word
        [HttpPost]
        //public IActionResult Create([FromBody] WordViewModel ch)
        public async Task<IActionResult> Create([FromBody] WordViewModel ch)
        {
            if (ch == null)
            {
                return BadRequest();
            }

            // Add it into the database
            EnWord word = new EnWord();
            List<EnWordExplain> listexp = new List<EnWordExplain>();
            List<EnWordExplainT> listexpt = new List<EnWordExplainT>();

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
                }
            }

            return CreatedAtRoute("GetWord", new { controller = "Word", id = word.WordID }, ch);
        }

    }
}
