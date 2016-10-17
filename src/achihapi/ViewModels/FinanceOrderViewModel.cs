using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceOrderViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        [StringLength(30)]
        public String Name { get; set; }
        [Required]
        public DateTime Valid_From { get; set; }
        [Required]
        public DateTime Valid_To { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }

    public class FinanceOrderSRuleViewModel
    {
        public Int32 OrdID { get; set; }
        [Required]
        public Int32 RuleID { get; set; }
        [Required]
        public Int32 ControlCenterID { get; set; }
        [Required]
        public Int32 Precent { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }
}
