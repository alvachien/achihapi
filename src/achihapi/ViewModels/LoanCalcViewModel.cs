using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class LoanCalcViewModel
    {
        public Decimal TotalAmount { get; set; }
        public Int16 TotalMonths { get; set; }
        public Decimal InterestRate { get; set; }        
        public DateTime StartDate { get; set; }
        public Boolean InterestFreeLoan { get; set; }
    }

    public sealed class LoanCalcResult
    {
        public DateTime TranDate { get; set; }
        public Decimal TranAmount { get; set; }
        public Decimal InterestAmount { get; set; }
    }    
}
