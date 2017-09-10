using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceTranTypeViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32? HID { get; set; }
        [Required]
        [StringLength(30)]
        public String Name { get; set; }
        public Boolean Expense { get; set; }
        public Int32 ParID { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }
}
