using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinPlan
    {
        public int Id { get; set; }
        public int Hid { get; set; }
        public byte Ptype { get; set; }
        public int? Accountid { get; set; }
        public int? Acntctgyid { get; set; }
        public int? Ccid { get; set; }
        public int? Ttid { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Tgtdate { get; set; }
        public decimal Tgtbal { get; set; }
        public string Trancurr { get; set; }
        public string Desp { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
    }
}
