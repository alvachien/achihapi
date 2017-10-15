using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LibBookViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        [StringLength(50)]
        public String NativeName { get; set; }
        [StringLength(50)]
        public String EnglishName { get; set; }
        public Boolean EnglishIsNative { get; set; }
        [StringLength(100)]
        public String ShortIntro { get; set; }
        [StringLength(100)]
        public String ExtLink1 { get; set; }
    }
}
