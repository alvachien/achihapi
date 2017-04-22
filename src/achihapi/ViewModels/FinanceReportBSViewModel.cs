using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceReportBSViewModel
    {
        [Key]
        public Int32 AccountID { get; set; }
        [StringLength(30)]
        public String AccountName { get; set; }
        public Int32 AccountCategoryID { get; set; }
        [StringLength(30)]
        public String AccountCategoryName { get; set; }
        public Decimal DebitBalance { get; set; }
        public Decimal CreditBalance { get; set; }
        public Decimal Balance { get; set; }
    }
}
