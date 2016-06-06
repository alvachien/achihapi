using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnSentenceExplain
    {
        public EnSentenceExplain()
        {
            EnSentenceExplainT = new HashSet<EnSentenceExplainT>();
        }

        public int SentenceID { get; set; }
        public int ExplainID { get; set; }

        public virtual ICollection<EnSentenceExplainT> EnSentenceExplainT { get; set; }
        public virtual EnSentence Sentence { get; set; }
    }
}
