using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    public enum FinanceAccountStatus : Byte
    {
        Normal = 0,
        Closed = 1,
        Frozen = 2
    }

    [Table("T_FIN_ACCOUNT")]
    public partial class FinanceAccount : BaseModel
    {
        [Key]
        [Column("ID", TypeName="int")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="int")]
        public Int32 HID { get; set; }

        [Required]
        [Column("CTGYID", TypeName="int")]
        public Int32 CategoryID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }
        
        [StringLength(40)]
        public String Owner { get; set; }
        public FinanceAccountStatus Status { get; set; }
    }
}
