using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceTranTypeViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32? HID { get; set; }
        [Required]
        [StringLength(30)]
        public String Name { get; set; }
        public Boolean Expense { get; set; }
        public Int32 ParID { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }

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
    }
}
