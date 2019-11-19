using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLibLocationDetail
    {
        public int Locid { get; set; }
        public int Seqno { get; set; }
        public byte Contenttype { get; set; }
        public int Contentid { get; set; }
        public string Others { get; set; }

        public TLibLocation Loc { get; set; }
    }
}
