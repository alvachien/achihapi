using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{    
    public class WordExplainViewModel
    {
        //public int WordID { get; set; }
        public int ExplainID { get; set; }
        public String POSAbb { get; set; }
        public string LangID { get; set; }
        public string ExplainString { get; set; }
    }

    public class WordViewModel
    {
        public int WordID { get; set; }
        public string WordString { get; set; }
        public string Tags { get; set; }
        public List<WordExplainViewModel> Explains { get; } = new List<WordExplainViewModel>();
    }
}
