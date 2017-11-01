using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class TagViewModel
    {
        public Int32 HID { get; set; }
        [Required]
        public Int16 TagType { get; set; }
        public Int32 TagID { get; set; }
        [Required]
        [StringLength(50)]
        public String Term { get; set; }
    }

    public class TagCountViewModel
    {
        [Required]
        [StringLength(50)]
        public String Term { get; set; }
        [Required]
        public Int32 TermCount { get; set; }
    }
}
