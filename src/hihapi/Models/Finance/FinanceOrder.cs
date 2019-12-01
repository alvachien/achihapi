using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_ORDER")]
    public class FinanceOrder: BaseModel
    {
        public FinanceOrder(): base()
        {
            SRule = new HashSet<FinanceOrderSRule>();
        }
        
        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HID { get; set; }

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

        public ICollection<FinanceOrderSRule> SRule { get; set; }
    }

    [Table("T_FIN_ORDER_SRULE")]
    public class FinanceOrderSRule
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

        public FinanceOrder Order { get; set; }
    }
}
