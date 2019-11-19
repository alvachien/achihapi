using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnHist
    {
        public int Hid { get; set; }
        public string Userid { get; set; }
        public int Objectid { get; set; }
        public DateTime Learndate { get; set; }
        public string Comment { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
    }
}
