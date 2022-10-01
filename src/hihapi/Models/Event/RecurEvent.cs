using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Event
{
    public class RecurEvent
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HID { get; set; }

        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public DateTime StartTimePoint { get; set; }
        [Required]
        public DateTime EndTimePoint { get; set; }
        public RepeatFrequency RptType { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Assignee { get; set; }

        public String Tags { get; set; }
    }
}
