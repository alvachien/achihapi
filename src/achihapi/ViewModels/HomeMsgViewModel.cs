using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class HomeMsgViewModel : BaseViewModel
    {
        public Int32? ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        [MaxLength(50)]
        public String UserTo { get; set; }
        [Required]
        public DateTime SendDate { get; set; }
        [Required]
        [MaxLength(50)]
        public String UserFrom { get; set; }
        [Required]
        [MaxLength(20)]
        public String Title { get; set; }
        [MaxLength(50)]
        public String Content { get; set; }
        [Required]
        public Boolean ReadFlag { get; set; }
    }
}
