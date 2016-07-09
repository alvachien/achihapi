using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class EnWordExplain
    {
        public EnWordExplain()
        {
            EnWordExplainT = new HashSet<EnWordExplainT>();
        }

        public int WordId { get; set; }
        public int ExplainId { get; set; }
        public string Posabb { get; set; }

        public virtual ICollection<EnWordExplainT> EnWordExplainT { get; set; }
        public virtual EnWord Word { get; set; }
    }
}
