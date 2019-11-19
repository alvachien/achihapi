using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class THomemsg
    {
        public int Id { get; set; }
        public int Hid { get; set; }
        public string Userto { get; set; }
        public DateTime Senddate { get; set; }
        public string Userfrom { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Readflag { get; set; }
        public bool? SendDel { get; set; }
        public bool? RevDel { get; set; }

        public THomedef H { get; set; }
    }
}
