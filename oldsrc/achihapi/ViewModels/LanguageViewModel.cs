using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LanguageViewModel
    {
        [Required]
        public Int32 Lcid { get; set; }
        [Required]
        [StringLength(20)]
        public String ISOName { get; set; }
        [Required]
        [StringLength(100)]
        public String EnglishName { get; set; }
        [Required]
        [StringLength(100)]
        public String NativeName { get; set; }
        public Boolean AppFlag { get; set; }
    }
}
