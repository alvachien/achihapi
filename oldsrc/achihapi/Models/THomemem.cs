using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace achihapi.Models
{
    public partial class THomemem : BaseModel
    {
        [Key]
        [Required]
        public int Hid { get; set; }
        [Key]
        [Required]
        public string User { get; set; }
        [Required]
        public string Displayas { get; set; }
        public short Relt { get; set; }

        public THomedef H { get; set; }
    }
}
