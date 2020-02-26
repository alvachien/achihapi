using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_ACCOUNT_CTGY")]
    public sealed class FinanceAccountCategory : BaseModel
    {
        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName="INT")]
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

        public FinanceAccountCategory(): base()
        {
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
            refcnt = context.FinanceAccount.Where(p => p.CategoryID == this.ID).Count();
            if (refcnt > 0) return false;

            return true;
        }
    }
}