using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinOrderSrule
    {
        public int Ordid { get; set; }
        public int Ruleid { get; set; }
        public int Controlcenterid { get; set; }
        public int Precent { get; set; }
        public string Comment { get; set; }

        public TFinOrder Ord { get; set; }
    }
}
