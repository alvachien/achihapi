using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceReportTranTypeViewModel
    {
        [Key]
        public Int32 TranType { get; set; }
        public DateTime TranDate { get; set; }
        public String Name { get; set; }
        public Boolean ExpenseFlag { get; set; }
        public Decimal TranAmount { get; set; }
    }
}
