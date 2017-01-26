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
        [Required]
        public Int32 ObjectID { get; set; }
        [Required]
        public DateTime LearnDate { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }

    public class LearnHistoryUIViewModel : LearnHistoryViewModel
    {
        public string UserDisplayAs { get; set; }
        public string ObjectName { get; set; }
    }
}
