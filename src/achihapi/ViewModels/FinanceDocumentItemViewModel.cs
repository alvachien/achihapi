using System;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceDocumentItemViewModel
    {
        public Int32 DocID { get; set; }
        [Required]
        public Int32 ItemID { get; set; }
        [Required]
        public Int32 AccountID { get; set; }
        public Int32 TranType { get; set; }
        [Required]
        public Decimal TranAmount { get; set; }
        public Boolean UseCurr2 { get; set; }
        public Int32 ControlCenterID { get; set; }
        public Int32 OrderID { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
    }

    public class FinanceDocumentItemUIViewModel : FinanceDocumentItemViewModel
    {
        public String AccountName { get; set; }
        public String TranTypeName { get; set; }
        public String ControlCenterName { get; set; }
        public String OrderName { get; set; }

        public DateTime TranDate { get; set; }
        public String DocDesp { get; set; }
    }

    public class FinanceDocumentItemWithBalanceUIViewModel : FinanceDocumentItemUIViewModel
    {
        public Boolean TranType_Exp { get; set; }
        public String TranCurr { get; set; }
        public Decimal TranAmount_Org { get; set; }
        public Decimal TranAmount_LC { get; set; }
        public Decimal Balance { get; set; }
    }
}
