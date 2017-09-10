using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceDocTypeViewModel : BaseViewModel
    {
        public Int16 ID { get; set; }
        public Int32? HID { get; set; }
        [Required]
        [StringLength(30)]
        public String Name { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }

        // Constants
        public const Int32 DocType_Transfer = 2;
        public const Int32 DocType_AdvancePayment = 5;
    }
}
