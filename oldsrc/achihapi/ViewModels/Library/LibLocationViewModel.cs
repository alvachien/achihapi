using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LibLocationViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        public Boolean? IsDevice { get; set; }
        [StringLength(50)]
        public String Desp { get; set; }
    }
}
