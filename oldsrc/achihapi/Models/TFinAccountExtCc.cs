using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinAccountExtCc
    {
        public int Accountid { get; set; }
        public short Billdayinmonth { get; set; }
        public short Repaydayinmonth { get; set; }
        public string Cardnum { get; set; }
        public string Others { get; set; }
        public string Bank { get; set; }
        public DateTime? Validdate { get; set; }

        public TFinAccount Account { get; set; }
    }
}
