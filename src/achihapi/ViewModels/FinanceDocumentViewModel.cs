using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceDocumentViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int16 DocType { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        [StringLength(5)]
        public String TranCurr { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
        public Byte ExgRate { get; set; }
        public Boolean ExgRate_Plan { get; set; }
        [StringLength(5)]
        public String TranCurr2 { get; set; }
        public Byte ExgRate2 { get; set; }
    }

    public class FinanceDocumentItemViewModel
    {
        public Int32 DocID { get; set; }
        [Required]
        public Int32 ItemID { get; set; }
        [Required]
        public Int32 AccountID { get; set; }
        public Int32 TranType { get; set; }
        [Required]
        public Decimal TranAmount { get; set; }
        public Boolean UseCurr2 { get; set; }
        public Int32 ControlCenterID { get; set; }
        public Int32 OrderID { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
    }
}
