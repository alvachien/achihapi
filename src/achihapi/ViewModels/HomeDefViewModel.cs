using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class HomeDefViewModel : BaseViewModel
    {
        public Int32? ID { get; set; }
        [Required]
        [MaxLength(50)]
        public String Name { get; set; }
        [MaxLength(50)]
        public String Details { get; set; }
        [Required]
        [MaxLength(50)]
        public String Host { get; set; }
        [Required]
        [MaxLength(5)]
        public String BaseCurrency { get; set; }

        // For creation - which need create a home member automatically
        public String CreatorDisplayAs { get; set; }

        // Members
        public List<HomeMemViewModel> Members = new List<HomeMemViewModel>();
    }
}
