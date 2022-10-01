using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Event
{
    public class NormalEvent : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int Id { get; set; }

        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("EVENT_NAME", TypeName = "NVARCHAR(100)")]
        public string EventName { get; set; }

        [Required]
        public DateTime StartTimePoint { get; set; }
        public DateTime? EndTimePoint { get; set; }
        public DateTime? CompleteTimePoint { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Assignee { get; set; }
        public Int32? RefRecurrID { get; set; }

    }
}
