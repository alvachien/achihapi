using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceCurrencyViewModel : BaseViewModel
    {
        [Required]
        [Key]
        [StringLength(5)]
        public String Curr { get; set; }
        [Required]
        [StringLength(45)]
        public String Name { get; set; }
        [StringLength(30)]
        public String Symbol { get; set; }
    }
}
