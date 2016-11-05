using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LearnHistoryViewModel : BaseViewModel
    {
        [Required]
        [StringLength(40)]
        public String UserID { get; set; }
    }
}
