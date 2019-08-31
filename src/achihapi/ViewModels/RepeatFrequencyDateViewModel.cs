using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class RepeatFrequencyDateInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RepeatFrequency RptType { get; set; }
    }

    public class RepeatFrequencyDateViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
