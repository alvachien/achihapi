using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceDocTypeViewModel : BaseViewModel
    {
        public Int16 ID { get; set; }
        public Int32? HID { get; set; }
        [Required]
        [StringLength(30)]
        public String Name { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }

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
        public const Int16 DocType_Insurece = 13;
    }
}
