using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnPOST
    {
        public string POSAbb { get; set; }
        public string LangID { get; set; }
        public string POSName { get; set; }

        public virtual ENPOS POSAbbNavigation { get; set; }
    }
}
