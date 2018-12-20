using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public enum FinancePlanTypeEnum : Byte
    {
        Account         = 0,
        AccountCategory = 1,
        ControlCenter   = 2,
        TranType        = 3,
    }

    public sealed class FinancePlanViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        public FinancePlanTypeEnum PlanType { get; set; }
        public Int32? AccountID { get; set; }
        public Int32? AccountCategoryID { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? TranTypeID { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime TargetDate { get; set; }
        [Required]
        public Decimal TargetBalance { get; set; }
        [Required]
        [StringLength(5)]
        public String TranCurr { get; set; }
        [Required]
        [StringLength(50)]
        public String Description { get; set; }
    }
}
