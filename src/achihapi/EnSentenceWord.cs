using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnSentenceWord
    {
        public int SentenceID { get; set; }
        public int WordID { get; set; }

        public virtual EnSentence Sentence { get; set; }
        public virtual EnWord Word { get; set; }
    }
}
