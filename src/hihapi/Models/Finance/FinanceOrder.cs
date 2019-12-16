using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_ORDER")]
    public sealed class FinanceOrder: BaseModel
    {
        public FinanceOrder(): base()
        {
            ValidFrom = DateTime.Today;
            ValidTo = DateTime.Today;

            SRule = new HashSet<FinanceOrderSRule>();
        }
        
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

        [Required]
        [Column("VALID_FROM")]
        [DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }
        
        [Required]
        [Column("VALID_TO")]
        [DataType(DataType.Date)]
        public DateTime ValidTo { get; set; }
        
        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context))
                return false;

            // Check Validility
            var ts = ValidTo - ValidFrom;
            if (ts.TotalSeconds < 0)
                return false;

            // SRule must exist
            if (SRule.Count <= 0)
                return false;

            // Percentage checks
            var total = 0;
            foreach (var rule in SRule)
            {
                if (!rule.IsValid())
                    return false;

                total += rule.Precent;
            }
            if (total != 100)
                return false;

            return true;
        }
        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context)) return false;

            var refcnt = 0;
            // Document items
            refcnt = context.FinanceDocumentItem.Where(p => p.OrderID == this.ID).Count();
            if (refcnt > 0) return false;

            return true;
        }

        public ICollection<FinanceOrderSRule> SRule { get; set; }
        public HomeDefine CurrentHome { get; set; }
    }

    [Table("T_FIN_ORDER_SRULE")]
    public sealed class FinanceOrderSRule
    {
        [Key]
        [Column("ORDID", TypeName="INT")]
        public Int32 OrderID { get; set; }

        [Key]
        [Column("RULEID", TypeName="INT")]
        public Int32 RuleID { get; set; }

        [Required]
        [Column("CONTROLCENTERID", TypeName="INT")]
        public Int32 ControlCenterID { get; set; }

        [Required]
        [Column("PRECENT", TypeName="INT")]
        public Int32 Precent { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }

        public Boolean IsValid()
        {
            if (this.Precent <= 0)
                return false;

            return true;
        }

        public FinanceOrder Order { get; set; }
    }
}
