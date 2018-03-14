using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LearnAwardViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        [StringLength(40)]
        public String UserID { get; set; }
        [Required]
        public DateTime AwardDate { get; set; }
        [Required]
        public Int32 Score { get; set; }
        [Required]
        [StringLength(40)]
        public String Reason { get; set; }
    }
}
