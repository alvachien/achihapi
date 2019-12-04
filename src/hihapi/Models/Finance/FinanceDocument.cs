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
        
        public Decimal ExgRate { get; set; }
        
        public Boolean ExgRate_Plan { get; set; }
        
        [StringLength(5)]
        public String TranCurr2 { get; set; }
        
        public Decimal ExgRate2 { get; set; }
        
        public Boolean ExgRate_Plan2 { get; set; }
        
        public Decimal TranAmount { get; set; }

        public ICollection<FinanceDocumentItem> Items { get; set; }
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
        [Column("TRANAMOUNT", TypeName="INT")]
        public Decimal TranAmount { get; set; }
        
        public Boolean UseCurr2 { get; set; }

        public Int32? ControlCenterID { get; set; }

        public Int32? OrderID { get; set; }

        [StringLength(45)]
        public String Desp { get; set; }
    }
}
