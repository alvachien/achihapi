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
        public Int32 HID { get; set; }
        [Required]
        [StringLength(30)]
        public String Name { get; set; }
        [Required]
        public DateTime ValidFrom { get; set; }
        [Required]
        public DateTime ValidTo { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }

        public readonly List<FinanceOrderSRuleUIViewModel> SRuleList = new List<FinanceOrderSRuleUIViewModel>();
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

    public class FinanceOrderSRuleUIViewModel : FinanceOrderSRuleViewModel
    {
        public String ControlCenterName { get; set; }
    }
}
