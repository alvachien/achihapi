using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnWordExplain
    {
        public EnWordExplain()
        {
            EnWordExplainT = new HashSet<EnWordExplainT>();
        }

        public int WordID { get; set; }
        public int ExplainID { get; set; }
        public int POSID { get; set; }

        public virtual ICollection<EnWordExplainT> EnWordExplainT { get; set; }
        public virtual EnWord Word { get; set; }
    }
}
