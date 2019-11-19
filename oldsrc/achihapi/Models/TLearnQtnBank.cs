using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnQtnBank
    {
        public TLearnQtnBank()
        {
            TLearnQtnBankSub = new HashSet<TLearnQtnBankSub>();
        }

        public int Id { get; set; }
        public int Hid { get; set; }
        public byte Type { get; set; }
        public string Question { get; set; }
        public string BriefAnswer { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public ICollection<TLearnQtnBankSub> TLearnQtnBankSub { get; set; }
    }
}
