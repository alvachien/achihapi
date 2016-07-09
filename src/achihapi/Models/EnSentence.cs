using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class EnSentence
    {
        public EnSentence()
        {
            EnSentenceExplain = new HashSet<EnSentenceExplain>();
            EnSentenceWord = new HashSet<EnSentenceWord>();
        }

        public int SentenceId { get; set; }
        public string SentenceString { get; set; }
        public string Tags { get; set; }

        public virtual ICollection<EnSentenceExplain> EnSentenceExplain { get; set; }
        public virtual ICollection<EnSentenceWord> EnSentenceWord { get; set; }
    }
}
