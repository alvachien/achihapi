using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LearnObjectViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 CategoryID { get; set; }
        [Required]
        [StringLength(45)]
        public String Name { get; set; }
        [Required]
        public String Content { get; set; }
    }
}
