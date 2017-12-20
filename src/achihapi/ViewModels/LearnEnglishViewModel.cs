using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public sealed class LearnEnWordViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [Required]
        [StringLength(100)]
        public String Word { get; set; }
        [StringLength(50)]
        public String UKPron { get; set; }
        [StringLength(50)]
        public String USPron { get; set; }
        [StringLength(100)]
        public String UKPronFile { get; set; }
        [StringLength(100)]
        public String USPronFile { get; set; }

        public List<LearnEnWordExpViewModel> Explains { get; private set; }

        public LearnEnWordViewModel()
        {
            this.Explains = new List<LearnEnWordExpViewModel>();
        }
    }

    public class LearnEnWordExpViewModel
    {
        public Int32 WordID { get; set; }
        public Int32 ExpID { get; set; }
        [StringLength(10)]
        public String POSAbb { get; set; }
        [Required]
        public String LanguageKey { get; set; }
        [Required]
        [StringLength(100)]
        public String Detail { get; set; }
    }

    public sealed class LearnEnSentenceViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [Required]
        [StringLength(100)]
        public String Sentence { get; set; }

        public List<LearnEnSentenceExpViewModel> Explains { get; private set; }
        public List<Int32> RelatedWordIDs { get; private set; }

        public LearnEnSentenceViewModel()
        {
            this.Explains = new List<LearnEnSentenceExpViewModel>();
            this.RelatedWordIDs = new List<Int32>();
        }
    }

    public class LearnEnSentenceExpViewModel
    {
        public Int32 SentenceID { get; set; }
        public Int32 ExpID { get; set; }
        [Required]
        public String LanguageKey { get; set; }
        [Required]
        [StringLength(100)]
        public String Detail { get; set; }
    }

    public class LearnEnglishViewModel
    {
    }

    public class WordRefSent
    {
        public string EnSent { get; set; }
        public string CnSent { get; set; }
    }

    public class WordResult
    {
        public string WordString { get; set; }
        public string WordPronUK { get; set; }
        public string WordPronUS { get; set; }
        public string WordPronUKFile { get; set; }
        public string WordPronUSFile { get; set; }
        public List<String> WordForms { get; set; }
        public List<String> WordExplains { get; private set; }
        public List<WordRefSent> WordSentences { get; private set; }

        public WordResult()
        {
            this.WordExplains = new List<string>();
            this.WordSentences = new List<WordRefSent>();
            this.WordForms = new List<string>();
        }
    }
}
