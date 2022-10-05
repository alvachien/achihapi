using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Event
{
    [Table("T_EVENT")]
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
        [Column("NAME", TypeName = "NVARCHAR(100)")]
        public string Name { get; set; }

        [Required]
        [Column("StartTime", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime StartDate{ get; set; }
        
        [Column("EndTime", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? EndDate{ get; set; }

        [Column("CompleteTime", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? CompleteDate { get; set; }

        [Column("Content", TypeName = "NVARCHAR(MAX)")]
        public String Content { get; set; }

        [Column("IsPublic", TypeName = "BIT")]
        public Boolean IsPublic { get; set; }

        [StringLength(40)]
        [Column("Assignee", TypeName = "NVARCHAR(40)")]
        public String Assignee { get; set; }

        [Column("RefRecurID", TypeName = "INT")]
        public Int32? RefRecurrID { get; set; }

        public HomeDefine CurrentHome { get; set; }
        public RecurEvent CurrentRecurEvent { get; set; }

        public NormalEvent()
        {
            this.IsPublic = true;
            this.StartDate = DateTime.Today;
        }
    }
}
