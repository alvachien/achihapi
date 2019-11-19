using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnEnsentWord
    {
        public int SentId { get; set; }
        public int WordId { get; set; }

        public TLearnEnsent Sent { get; set; }
    }
}
