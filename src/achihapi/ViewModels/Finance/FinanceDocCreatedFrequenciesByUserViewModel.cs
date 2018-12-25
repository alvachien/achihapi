using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public enum FinanceDocCreatedFrequencyType : Byte
    {
        Daily   = 1,
        Weekly  = 2,
        Monthly = 3
    }

    public sealed class FinanceDocCreatedFrequenciesByUserViewModel
    {
        public String UserID { get; set; }
        public Int32? Year { get; set; }
        public Int32? Month { get; set; }
        public Int32? Week { get; set; }
        public Int32 AmountOfDocuments { get; set; }
    }
}
