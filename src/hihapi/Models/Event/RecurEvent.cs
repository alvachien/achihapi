using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Event
{
    [Table("T_EVENT_RECUR")]
    public class RecurEvent : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INTEGER")]
        public Int32 Id { get; set; }

        [Required]
        [Column("HID", TypeName = "INTEGER")]
        public Int32 HomeID { get; set; }

        [StringLength(50)]
        [Column("NAME", TypeName = "NVARCHAR(50)")]
        public String Name { get; set; }

        [Required]
        [Column("STARTDATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("ENDDATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("RPTTYPE", TypeName = "INTEGER")]
        public RepeatFrequency RecurType { get; set; }

        [Column("CONTENT", TypeName = "TEXT")]
        public String Content { get; set; }

        [Column("ISPUBLIC", TypeName = "INTEGER")]
        public Boolean IsPublic { get; set; }

        [StringLength(40)]
        [Column("ASSIGNEE", TypeName = "NVARCHAR(40)")]
        public String Assignee { get; set; }

        [ForeignKey("HomeID")]
        public HomeDefine CurrentHome { get; set; }
        public IList<NormalEvent> RelatedEvents { get; set; }

        public RecurEvent()
        {
            this.IsPublic = true;
            this.StartDate = DateTime.Today;
            this.RelatedEvents = new List<NormalEvent>();
        }
    }
}
