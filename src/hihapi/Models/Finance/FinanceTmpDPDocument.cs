using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_TMPDOC_DP")]
    public class FinanceTmpDPDocument: BaseModel
    {
        [Key]
        [Column("DOCID", TypeName="INT")]
        public Int32 DocumentID { get; set; }

        [Key]
        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HomeID { get; set; }

        [Column("REFDOCID", TypeName="INT")]
        public Int32? ReferenceDocumentID { get; set; }

        [Key]
        [Required]
        [Column("ACCOUNTID", TypeName="INT")]
        public Int32 AccountID { get; set; }

        [Required]
        [Column("TRANDATE")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column("TRANTYPE", TypeName="INT")]
        public Int32 TransactionType { get; set; }

        [Required]
        [Column("TRANAMOUNT", TypeName="DECIMAL(17, 2)")]
        public Decimal TranAmount { get; set; }

        [Column("CONTROLCENTERID", TypeName="INT")]
        public Int32? ControlCenterID { get; set; }

        [Column("ORDERID", TypeName="INT")]
        public Int32? OrderID { get; set; }
        
        [StringLength(45)]
        [Column("DESP", TypeName="NVARCHAR(45)")]
        public String Description { get; set; }

        public FinanceAccountExtraDP AccountExtra { get; set; }
    }
}
