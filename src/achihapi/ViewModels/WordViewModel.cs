using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{    
    public class WordExplainViewModel
    {
        //public int WordID { get; set; }
        public int ExplainID { get; set; }
        [Required]
        [StringLength(10)]
        public String POSAbb { get; set; }
        [Required]
        [StringLength(5)]
        public string LangID { get; set; }
        [Required]
        [StringLength(100)]
        public string ExplainString { get; set; }
        public String POSName { get; set; }
    }

    public class WordViewModel
    {
        public int WordID { get; set; }
        [Required]
        [StringLength(100)]
        public string WordString { get; set; }
        [StringLength(100)]
        public string Tags { get; set; }
        public List<WordExplainViewModel> Explains { get; } = new List<WordExplainViewModel>();
    }
}
