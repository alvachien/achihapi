using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class UserDetailViewModel
    {
        [Required]
        [StringLength(40)]
        public String UserID { get; set; }
        [Required]
        [StringLength(50)]
        public String DisplayAs { get; set; }
        [StringLength(100)]
        [EmailAddress]
        public String Email { get; set; }
        [StringLength(100)]
        public String Others { get; set; }
    }
}
