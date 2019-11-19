using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TDbversion
    {
        public int VersionId { get; set; }
        public DateTime ReleasedDate { get; set; }
        public DateTime AppliedDate { get; set; }
    }
}
