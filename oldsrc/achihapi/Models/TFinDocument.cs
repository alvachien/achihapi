using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinDocument
    {
        public TFinDocument()
        {
            TFinDocumentItem = new HashSet<TFinDocumentItem>();
        }

        public int Id { get; set; }
        public int Hid { get; set; }
        public short Doctype { get; set; }
        public DateTime Trandate { get; set; }
        public string Trancurr { get; set; }
        public string Desp { get; set; }
        public decimal? Exgrate { get; set; }
        public bool? ExgratePlan { get; set; }
        public bool? ExgratePlan2 { get; set; }
        public string Trancurr2 { get; set; }
        public decimal? Exgrate2 { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public ICollection<TFinDocumentItem> TFinDocumentItem { get; set; }
    }
}
