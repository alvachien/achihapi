using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class POSViewModel
    {
        [Required]
        [StringLength(10)]
        public string POSAbb { get; set; }
        [Required]
        [StringLength(50)]
        public string POSName { get; set; }
        [Required]
        [StringLength(5)]
        public string LangID { get; set; }
        public string POSNativeName { get; set; }
    }
}
