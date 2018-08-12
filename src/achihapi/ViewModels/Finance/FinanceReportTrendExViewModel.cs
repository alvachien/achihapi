using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public enum FinanceReportTrendExType: Int32
    {
        Daily       = 1,
        Weekly      = 2,
        Monthly     = 3
    }

    public class FinanceReportTrendExViewModel
    {
        public DateTime? TranDate { get; set; }
        public String TranWeek { get; set; }
        public String TranMonth { get; set; }
        public Decimal TranAmount { get; set; }
    }
}
