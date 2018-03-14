using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class LearnReportCtgyDateViewModel
    {
        public Int32 HID { get; set; }
        public Int32 Category { get; set; }
        public DateTime LearnDate { get; set; }
        public Int32 LearnCount { get; set; }
    }
}
