using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class ModuleViewModel
    {
        [Required]
        [StringLength(3)]
        public String Module { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        public Boolean AuthFlag { get; set; }
        public Boolean TagFlag { get; set; }
    }
}
