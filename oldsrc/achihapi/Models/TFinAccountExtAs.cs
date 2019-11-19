using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinAccountExtAs
    {
        public int Accountid { get; set; }
        public int Ctgyid { get; set; }
        public string Name { get; set; }
        public int RefdocBuy { get; set; }
        public string Comment { get; set; }
        public int? RefdocSold { get; set; }

        public TFinAssetCtgy Ctgy { get; set; }
    }
}
