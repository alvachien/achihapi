using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class ContactViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        public String Name2 { get; set; }
        public String NickName { get; set; }
        public byte Gender { get; set; }
        public String LocationInfo { get; set; }
        public String EducationInfo { get; set; }
    }
}
