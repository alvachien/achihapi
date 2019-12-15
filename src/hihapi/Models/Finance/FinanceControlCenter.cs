using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_CONTROLCENTER")]
    public sealed class FinanceControlCenter: BaseModel
    {
        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [Column("PARID", TypeName="INT")]
        public Int32? ParentID { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }

        [StringLength(40)]
        [Column("OWNER", TypeName="NVARCHAR(40)")]
        public String Owner { get; set; }

        public HomeDefine CurrentHome { get; set; }
    }
}
