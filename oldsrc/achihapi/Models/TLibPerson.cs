using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLibPerson
    {
        public int Id { get; set; }
        public int Hid { get; set; }
        public string NativeName { get; set; }
        public string EnglishName { get; set; }
        public bool? EnglishIsNative { get; set; }
        public byte? Gender { get; set; }
        public string ShortIntro { get; set; }
        public string ExtLink1 { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
    }
}
