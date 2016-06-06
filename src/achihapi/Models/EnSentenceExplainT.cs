using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnSentenceExplainT
    {
        public int SentenceID { get; set; }
        public int ExplainID { get; set; }
        public string LangID { get; set; }
        public string ExplainString { get; set; }

        public virtual EnSentenceExplain EnSentenceExplain { get; set; }
    }
}
