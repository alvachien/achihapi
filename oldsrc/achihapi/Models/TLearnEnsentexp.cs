using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnEnsentexp
    {
        public int SentId { get; set; }
        public short ExpId { get; set; }
        public string LangKey { get; set; }
        public string ExpDetail { get; set; }

        public TLearnEnsent Sent { get; set; }
    }
}
