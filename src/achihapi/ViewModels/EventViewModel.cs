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
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public DateTime StartTimePoint { get; set; }
        [Required]
        public DateTime EndTimePoint { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Owner { get; set; }
        public Int32? RefID { get; set; }

        public String Tags { get; set; }
    }
}
