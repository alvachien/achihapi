using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TTag
    {
        public int Hid { get; set; }
        public short TagType { get; set; }
        public int TagId { get; set; }
        public string Term { get; set; }
        public int TagSubId { get; set; }
    }
}
