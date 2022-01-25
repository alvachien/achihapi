using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public class LibraryBook
    {
        public Int32 Id { get; set; }
        public string NativeName { get; set; }
        public String ChineseName { get; set; }
    }
}
