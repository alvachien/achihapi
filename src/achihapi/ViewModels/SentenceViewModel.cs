using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class SentenceExplainViewModel
    {
        public Int32 ExplainID { get; set; }
        [Required]
        [StringLength(5)]
        public String LangID { get; set; }
        [Required]
        [StringLength(100)]
        public String ExplainString { get; set; }
    }
    public class SentenceViewModel
    {
        public Int32 SentenceID { get; set; }
        [Required]
        [StringLength(200)]
        public String SentenceString { get; set; }
        [StringLength(100)]
        public String Tags { get; set; }

        public List<SentenceExplainViewModel> Explains = new List<SentenceExplainViewModel>();
    }
}
