using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnWordExplainT
    {
        public int WordID { get; set; }
        public int ExplainID { get; set; }
        public string LangID { get; set; }
        public string ExplainString { get; set; }

        public virtual EnWordExplain EnWordExplain { get; set; }
    }
}
