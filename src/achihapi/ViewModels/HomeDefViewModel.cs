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
        [MaxLength(40)]
        public String Host { get; set; }
        [MaxLength(50)]
        public String UserNameInCreation { get; set; }

        // Members
        public List<HomeMemViewModel> Members = new List<HomeMemViewModel>();
    }
}
