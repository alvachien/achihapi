using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_HOMEMEM")]
    public partial class HomeMember : BaseModel
    {
        [Key]
        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Key]
        [Required]
        [MaxLength(50)]
        [Column("USER", TypeName = "NVARCHAR(50)")]
        public String User { get; set; }

        [MaxLength(50)]
        [Column("DISPLAYAS", TypeName = "NVARCHAR(50)")]
        public String DisplayAs { get; set; }

        [Required]
        [Column("RELT", TypeName = "SMALLINT")]
        public Int16 Relation { get; set; } 

        public HomeDefine HomeDefinition { get; set; }
    }
}
