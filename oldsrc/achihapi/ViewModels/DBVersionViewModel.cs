using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public sealed class DBVersionViewModel
    {
        public Int32 VersionID { get; set; }
        public DateTime ReleasedDate { get; set; }
        public DateTime AppliedDate { get; set; }
    }
}
