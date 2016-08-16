using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class EnPost
    {
        public string Posabb { get; set; }
        public string LangId { get; set; }
        public string Posname { get; set; }

        public virtual Enpos PosabbNavigation { get; set; }
    }
}
