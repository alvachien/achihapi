using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_LEARN_OBJ")]
    public sealed class LearnObject : BaseModel
    {
        [Key]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }
        
        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("CATEGORY", TypeName = "INT")]
        public Int32 CategoryID { get; set; }

        [Required]
        [StringLength(45)]
        [Column("NAME", TypeName = "NVARCHAR(45)")]
        public String Name { get; set; }
        
        [Required]
        [Column("CONTENT", TypeName = "NVARCHAR(MAX)")]
        public String Content { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public LearnObject() : base()
        {
        }
    }
}
