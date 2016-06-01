using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnWord
    {
        public EnWord()
        {
            EnSentenceWord = new HashSet<EnSentenceWord>();
            EnWordExplain = new HashSet<EnWordExplain>();
        }

        public int WordID { get; set; }
        public string WordString { get; set; }
        public string Tags { get; set; }

        public virtual ICollection<EnSentenceWord> EnSentenceWord { get; set; }
        public virtual ICollection<EnWordExplain> EnWordExplain { get; set; }
    }
}
