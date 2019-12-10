using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_DOCUMENT")]
    public sealed class FinanceDocument: BaseModel
    {
        public FinanceDocument(): base()
        {
            TranDate = DateTime.Today;

            Items = new HashSet<FinanceDocumentItem>();
        }

        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("DOCTYPE", TypeName="SMALLINT")]
        public Int16 DocType { get; set; }
        
        [Required]
        [Column("TRANDATE")]
        [DataType(DataType.Date)]
        public DateTime TranDate { get; set; }
        
        [Required]
        [StringLength(5)]
        [Column("TRANCURR", TypeName="NVARCHAR(5)")]
        public String TranCurr { get; set; }
        
        [Required]
        [StringLength(45)]
        [Column("DESP", TypeName="NVARCHAR(45)")]
        public String Desp { get; set; }
        
        [Column("EXGRATE", TypeName= "DECIMAL(17, 4)")]
        public Decimal ExgRate { get; set; }
        
        [Column("EXGRATE_PLAN", TypeName="BIT")]
        public Boolean ExgRate_Plan { get; set; }
        
        [StringLength(5)]
        [Column("TRANCURR2", TypeName="NVARCHAR(5)")]
        public String TranCurr2 { get; set; }
        
        [Column("EXGRATE2", TypeName = "DECIMAL(17, 4)")]
        public Decimal ExgRate2 { get; set; }
        
        [Column("EXGRATE_PLAN2", TypeName="BIT")]
        public Boolean ExgRate_Plan2 { get; set; }
        
        [NotMapped]
        public Decimal? TranAmount { get; set; }

        public ICollection<FinanceDocumentItem> Items { get; set; }
        public HomeDefine CurrentHome { get; set; }

        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;

            if (Items.Count == 0)
                return false;

            foreach (var item in Items)
            {
                if (!item.IsValid())
                    return false;
            }

            return true;
        }
    }

    [Table("T_FIN_DOCUMENT_ITEM")]
    public sealed class FinanceDocumentItem
    {
        [Key]
        [Column("DOCID", TypeName="INT")]
        public Int32 DocID { get; set; }
     
        [Key]
        [Column("ITEMID", TypeName="INT")]
        public Int32 ItemID { get; set; }
        
        [Required]
        [Column("ACCOUNTID", TypeName="INT")]
        public Int32 AccountID { get; set; }
        
        [Required]
        [Column("TRANTYPE", TypeName="INT")]
        public Int32 TranType { get; set; }
        
        [Required]
        [Column("TRANAMOUNT", TypeName="DECIMAL(17, 2)")]
        public Decimal TranAmount { get; set; }
        
        [Column("USECURR2", TypeName = "BIT")]
        public Boolean UseCurr2 { get; set; }

        [Column("CONTROLCENTERID", TypeName="INT")]
        public Int32? ControlCenterID { get; set; }

        [Column("ORDERID", TypeName = "INT")]
        public Int32? OrderID { get; set; }

        [StringLength(45)]
        [Column("DESP", TypeName = "NVARCHAR(45)")]
        public String Desp { get; set; }

        public FinanceDocument DocumentHeader { get; set; }

        public bool IsValid()
        {
            if (ItemID <= 0)
                return false;
            if (AccountID <= 0)
                return false;
            return true;
        }
    }
}
