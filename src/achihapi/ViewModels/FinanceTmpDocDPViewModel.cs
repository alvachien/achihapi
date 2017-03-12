using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceTmpDocDPViewModel : BaseViewModel
    {
        public Int32 DocID { get; set; }
        [Required]
        public Int32? RefDocID { get; set; }
        [Required]
        public Int32 AccountID { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        public Int32 TranType { get; set; }
        [Required]
        public Decimal TranAmount { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
    }
}

