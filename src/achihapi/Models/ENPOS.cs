using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class Enpos
    {
        public Enpos()
        {
            EnPost = new HashSet<EnPost>();
        }

        public string Posabb { get; set; }
        public string Posname { get; set; }

        public virtual ICollection<EnPost> EnPost { get; set; }
    }
}
