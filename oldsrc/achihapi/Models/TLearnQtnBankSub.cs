using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnQtnBankSub
    {
        public int Qtnid { get; set; }
        public string Subitem { get; set; }
        public string Detail { get; set; }
        public string Others { get; set; }

        public TLearnQtnBank Qtn { get; set; }
    }
}
