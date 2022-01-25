using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_DOC_TYPE")]
    public sealed class FinanceDocumentType : BaseModel
    {
        // Constants
        public const Int16 DocType_Normal = 1;
        public const Int16 DocType_Transfer = 2;
        public const Int16 DocType_CurrExchange = 3;
        public const Int16 DocType_AdvancePayment = 5;
        public const Int16 DocType_AssetBuyIn = 7;
        public const Int16 DocType_AssetSoldOut = 8;
        public const Int16 DocType_BorrowFrom = 9;
        public const Int16 DocType_LendTo = 10;
        public const Int16 DocType_Repay = 11;
        public const Int16 DocType_AdvanceReceive = 12;
        public const Int16 DocType_AssetValChg = 13;
        public const Int16 DocType_Insurance = 14;
        public const Int16 DocType_LoanPrepayment = 15;

        [Key]
        [Column("ID", TypeName="INT")]
        public Int16 ID { get; set; }

        [Column("HID", TypeName="INT")]
        public Int32? HomeID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public FinanceDocumentType(): base()
        {
        }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context))
                return false;
            if (String.IsNullOrEmpty(Name))
                return false;

            return true;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context)) return false;

            var refcnt = 0;
            // Documents
            refcnt = context.FinanceDocument.Where(p => p.DocType == this.ID).Count();
            if (refcnt > 0) return false;

            return true;
        }
    }
}
