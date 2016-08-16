using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnWordExplainT
    {
        public int WordId { get; set; }
        public int ExplainId { get; set; }
        public string LangId { get; set; }
        public string ExplainString { get; set; }

        public virtual EnWordExplain EnWordExplain { get; set; }
    }
}
