using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_LEARN_CTGY")]
    public sealed class LearnCategory: BaseModel
    {
        [Key]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName = "INT")]
        public Int32? HomeID { get; set; }

        [Column("PARID", TypeName = "INT")]
        public Int32? ParentID { get; set; }

        [Required]
        [StringLength(45)]
        [Column("NAME", TypeName = "NVARCHAR(45)")]
        public String Name { get; set; }
        
        [StringLength(50)]
        [Column("COMMENT", TypeName = "NVARCHAR(50)")]
        public String Comment { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public LearnCategory(): base()
        {
        }
    }
}
