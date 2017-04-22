using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceReportCCViewModel
    {
        [Key]
        public Int32 ControlCenterID { get; set; }
        [StringLength(30)]
        public String ControlCenterName { get; set; }
        public Decimal DebitBalance { get; set; }
        public Decimal CreditBalance { get; set; }
        public Decimal Balance { get; set; }
    }
}
