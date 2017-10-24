using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceReportOrderViewModel
    {
        [Key]
        public Int32 OrderID { get; set; }
        [StringLength(30)]
        public String OrderName { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public Decimal DebitBalance { get; set; }
        public Decimal CreditBalance { get; set; }
        public Decimal Balance { get; set; }
    }
}
