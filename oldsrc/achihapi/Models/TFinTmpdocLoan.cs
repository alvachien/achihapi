using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinTmpdocLoan
    {
        public int Docid { get; set; }
        public int Hid { get; set; }
        public int? Refdocid { get; set; }
        public int Accountid { get; set; }
        public DateTime Trandate { get; set; }
        public decimal Tranamount { get; set; }
        public decimal? Interestamount { get; set; }
        public int? Controlcenterid { get; set; }
        public int? Orderid { get; set; }
        public string Desp { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
    }
}
