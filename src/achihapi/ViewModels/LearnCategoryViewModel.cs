using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LearnCategoryViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 ParID { get; set; }
        [Required]
        [StringLength(45)]
        public String Name { get; set; }
        [StringLength(50)]
        public String Comment { get; set; }
        public Boolean SysFlag { get; set; }
    }
}
