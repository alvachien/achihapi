using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class EnSentenceWord
    {
        public int SentenceId { get; set; }
        public int WordId { get; set; }

        public virtual EnSentence Sentence { get; set; }
        public virtual EnWord Word { get; set; }
    }
}
