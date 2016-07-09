using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class EnSentenceExplain
    {
        public EnSentenceExplain()
        {
            EnSentenceExplainT = new HashSet<EnSentenceExplainT>();
        }

        public int SentenceId { get; set; }
        public int ExplainId { get; set; }

        public virtual ICollection<EnSentenceExplainT> EnSentenceExplainT { get; set; }
        public virtual EnSentence Sentence { get; set; }
    }
}
