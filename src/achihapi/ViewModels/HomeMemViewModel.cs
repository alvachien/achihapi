using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class HomeMemViewModel: BaseViewModel
    {
        [Required]
        public Int32 HomeID { get; set; }
        [Required]
        [MaxLength(50)]
        public String User { get; set; }
        [MaxLength(50)]
        public String DisplayAs { get; set; }
        [Required]
        public Int16 Relation { get; set; }
    }
}
