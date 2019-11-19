using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TFinDocType
    {
        public short Id { get; set; }
        public int? Hid { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
    }
}
