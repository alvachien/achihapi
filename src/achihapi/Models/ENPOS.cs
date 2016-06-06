using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class ENPOS
    {
        public ENPOS()
        {
            EnPOST = new HashSet<EnPOST>();
        }

        public string POSAbb { get; set; }
        public string POSName { get; set; }

        public virtual ICollection<EnPOST> EnPOST { get; set; }
    }
}
