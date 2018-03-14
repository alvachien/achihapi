using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceSettingViewModel : BaseViewModel
    {
        [Required]
        [StringLength(20)]
        public String SetID { get; set; }
        [StringLength(80)]
        public String SetValue { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }
}
