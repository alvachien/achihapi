using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLanguage
    {
        public int Lcid { get; set; }
        public string Isoname { get; set; }
        public string Enname { get; set; }
        public string Navname { get; set; }
        public bool? Appflag { get; set; }
    }
}
