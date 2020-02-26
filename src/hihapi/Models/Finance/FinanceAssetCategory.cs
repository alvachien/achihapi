using System;
using System.Collections.Generic;
using System.Linq;
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
        public ICollection<FinanceAccountExtraAS> AccountExtraAsset { get; set; }

        public FinanceAssetCategory(): base()
        {
            AccountExtraAsset = new HashSet<FinanceAccountExtraAS>();
        }

        public override bool IsValid(hihDataContext context)
        {
            return base.IsValid(context);
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context)) return false;

            // Account
            var refcnt = 0;
            // Documents
            refcnt = context.FinanceAccountExtraAS.Where(p => p.CategoryID == this.ID).Count();
            if (refcnt > 0) return false;

            return true;
        }
    }
}
