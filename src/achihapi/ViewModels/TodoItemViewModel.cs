using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class TodoItemViewModel
    {
        public Int32 TodoID { get; set; }
        [Required]
        [StringLength(50)]
        public String ItemName { get; set; }
        [Required]
        public Int32 Priority { get; set; }
        [StringLength(50)]
        public String Assignee { get; set; }
        [StringLength(50)]
        public String Dependence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String ItemContent { get; set; }
        [StringLength(50)]
        public String Tags { get; set; }
    }
}
