using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinAccountExtLoan
    {
        public int Accountid { get; set; }
        public DateTime Startdate { get; set; }
        public decimal? Annualrate { get; set; }
        public bool? Interestfree { get; set; }
        public byte? Repaymethod { get; set; }
        public short? Totalmonth { get; set; }
        public int Refdocid { get; set; }
        public string Others { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Payingaccount { get; set; }
        public string Partner { get; set; }

        public TFinAccount Account { get; set; }
    }
}
