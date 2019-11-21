using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_ASSET_CTGY")]
    public partial class FinanceAssetCategory : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName="INT")]
        public Int32? HID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("NAME", TypeName="NVARCHAR(50)")]
        public String Name { get; set; }

        [StringLength(50)]
        [Column("DESP", TypeName="NVARCHAR(50)")]
        public String Desp { get; set; }
    }
}
