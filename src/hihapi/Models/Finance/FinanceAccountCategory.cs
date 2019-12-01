using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_ACCOUNT_CTGY")]
    public partial class FinanceAccountCategory : BaseModel
    {
        [Key]
        [Column("ID", TypeName="int")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName="int")]
        public Int32? HomeID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [Required]
        [Column("ASSETFLAG", TypeName="BIT")]
        public Boolean AssetFlag { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }

        public HomeDefine CurrentHome { get; set; }
    }
}