using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Models
{
    public class RepeatedDates
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public enum RepeatFrequency : Byte
    {
        Month = 0,
        Fortnight = 1,
        Week = 2,
        Day = 3,
        Quarter = 4,
        HalfYear = 5,
        Year = 6,
        Manual = 7,
    }

    public class RepeatDatesCalculationInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RepeatFrequency RptType { get; set; }
    }
}
