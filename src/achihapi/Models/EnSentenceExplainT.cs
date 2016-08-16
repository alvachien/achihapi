using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnSentenceExplainT
    {
        public int SentenceId { get; set; }
        public int ExplainId { get; set; }
        public string LangId { get; set; }
        public string ExplainString { get; set; }

        public virtual EnSentenceExplain EnSentenceExplain { get; set; }
    }
}
