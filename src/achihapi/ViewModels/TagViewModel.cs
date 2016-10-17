using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class TagViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
    }

    public class TagLinkViewModel
    {
        public Int32 TagID { get; set; }
        [Required]
        [StringLength(3)]
        public String Module { get; set; }
        [Required]
        public Int32 ObjID { get; set; }
    }
}
