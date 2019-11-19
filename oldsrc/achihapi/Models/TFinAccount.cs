using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinAccount
    {
        public int Id { get; set; }
        public int Hid { get; set; }
        public int Ctgyid { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Owner { get; set; }
        public byte? Status { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public TFinAccountExtCc TFinAccountExtCc { get; set; }
        public TFinAccountExtDp TFinAccountExtDp { get; set; }
        public TFinAccountExtLoan TFinAccountExtLoan { get; set; }
        public TFinAccountExtLoanH TFinAccountExtLoanH { get; set; }
    }
}
