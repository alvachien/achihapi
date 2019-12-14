using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_ASSET_CTGY")]
    public sealed class FinanceAssetCategory : BaseModel
    {
        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName="INT")]
        public Int32? HomeID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("NAME", TypeName="NVARCHAR(50)")]
        public String Name { get; set; }

        [StringLength(50)]
        [Column("DESP", TypeName="NVARCHAR(50)")]
        public String Desp { get; set; }

        public HomeDefine CurrentHome { get; set; }
        //public ICollection<FinanceAccountExtraAS> AccountExtraAsset { get; set; }
    }
}
