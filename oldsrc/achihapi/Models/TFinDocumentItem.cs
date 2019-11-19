using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinDocumentItem
    {
        public int Docid { get; set; }
        public int Itemid { get; set; }
        public int Accountid { get; set; }
        public int Trantype { get; set; }
        public decimal Tranamount { get; set; }
        public bool? Usecurr2 { get; set; }
        public int? Controlcenterid { get; set; }
        public int? Orderid { get; set; }
        public string Desp { get; set; }

        public TFinDocument Doc { get; set; }
    }
}
