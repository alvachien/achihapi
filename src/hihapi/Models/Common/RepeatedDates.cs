using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace hihapi.Models
{
    public class RepeatedDates
    {
        [DataType(DataType.Date)]
        public Date StartDate { get; set; }
        [DataType(DataType.Date)]
        public Date EndDate { get; set; }
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
        [DataType(DataType.Date)]
        public Date StartDate { get; set; }
        [DataType(DataType.Date)]
        public Date EndDate { get; set; }
        public RepeatFrequency RepeatType { get; set; }
    }

    public sealed class RepeatDatesWithAmountCalculationInput : RepeatDatesCalculationInput
    {
        public Decimal TotalAmount { get; set; }
        public String Desp { get; set; }
    }

    public sealed class RepeatedDatesWithAmount
    {
        [DataType(DataType.Date)]
        public Date TranDate { get; set; }
        public Decimal TranAmount { get; set; }
        public String Desp { get; set; }
    }
}
