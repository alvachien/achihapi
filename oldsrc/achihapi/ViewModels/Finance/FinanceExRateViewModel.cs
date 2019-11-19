using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceExRateViewModel : BaseViewModel
    {
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        [StringLength(5)]
        public String Curr { get; set; }
        [Required]
        public Decimal Rate { get; set; }
        public Int32 RefDocID { get; set; }        
    }
}
