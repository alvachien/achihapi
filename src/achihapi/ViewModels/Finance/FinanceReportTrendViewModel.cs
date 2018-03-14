using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public enum FinanceTrendReportEnum: Int32
    {
        Base = 0,
        TranType = 1
    }

    public class FinanceReportTrendBaseViewModel
    {
        public Int32 Year { get; set; }
        public Int32 Month { get; set; }
        public Boolean Expense { get; set; }
        public Decimal TranAmount { get; set; }
    }
}
