using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class LanguageViewModel
    {
        public Int32 LCID { get; set; }
        public String ISOName { get; set; }
        public String EnglishName { get; set; }
        public String NativeName { get; set; }
    }
}
