using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public DateTime StartTimePoint { get; set; }        
        public DateTime? EndTimePoint { get; set; }
        public DateTime? CompleteTimePoint { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Assignee { get; set; }
        public Int32? RefRecurrID { get; set; }

        public String Tags { get; set; }
    }
}
