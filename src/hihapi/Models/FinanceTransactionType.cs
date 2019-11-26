using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_TRAN_TYPE")]
    public partial class FinanceTransactionType : BaseModel
    {
        // Constants
        public const Int32 TranType_OpeningAsset = 1;
        public const Int32 TranType_OpeningLiability = 82;

        public const Int32 TranType_TransferIn = 37;
        public const Int32 TranType_TransferOut = 60;

        public const Int32 TranType_BorrowFrom = 80;        
        public const Int32 TranType_LendTo = 81;
        public const Int32 TranType_RepaymentOut = 86;
        public const Int32 TranType_RepaymentIn = 87;
        public const Int32 TranType_AdvancePaymentOut = 88;
        public const Int32 TranType_AdvanceReceiveIn = 91;

        public const Int32 TranType_InterestOut = 55;
        public const Int32 TranType_InterestIn = 8;

        public const Int32 TranType_AssetValueDecrease = 89;
        public const Int32 TranType_AssetValueIncrease = 90;

        public const Int32 TranType_AssetSoldout = 92;
        public const Int32 TranType_AssetSoldoutIncome = 93;

        public const Int32 TranType_InsuranceReturn = 36;
        public const Int32 TranType_InsurancePaymentOut = 34;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID", TypeName="int")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName="INT")]
        public Int32? HID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [Required]
        [Column("EXPENSE", TypeName="BIT")]
        public Boolean Expense { get; set; }

        [Column("PARID", TypeName="INT")]
        public Int32? ParID { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }
    }
}
