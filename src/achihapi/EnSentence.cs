using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnSentence
    {
        public EnSentence()
        {
            EnSentenceWord = new HashSet<EnSentenceWord>();
        }

        public int SentenceID { get; set; }
        public string SentenceString { get; set; }
        public string Tags { get; set; }

        public virtual ICollection<EnSentenceWord> EnSentenceWord { get; set; }
    }
}
