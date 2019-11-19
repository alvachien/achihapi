using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinAccountExtDp
    {
        public int Accountid { get; set; }
        public bool Direct { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public byte Rpttype { get; set; }
        public int Refdocid { get; set; }
        public string Defrrdays { get; set; }
        public string Comment { get; set; }

        public TFinAccount Account { get; set; }
    }
}
