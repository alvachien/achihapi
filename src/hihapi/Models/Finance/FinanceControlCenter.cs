using System;
using System.Collections.Generic;
using System.Linq;
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

        public FinanceControlCenter(): base()
        {
        }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context)) return false;
            if (String.IsNullOrEmpty(Name))
                return false;
            if (HomeID == 0)
                return false;

            return true;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context)) return false;

            var refcnt = 0;
            // Document items
            refcnt = context.FinanceDocumentItem.Where(p => p.ControlCenterID == this.ID).Count();
            if (refcnt > 0) return false;
            // Order srules 
            refcnt = context.FinanceOrderSRule.Where(p => p.ControlCenterID == this.ID).Count();
            if (refcnt > 0) return false;

            return true;
        }
    }
}
