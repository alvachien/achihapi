using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceDocumentViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [Required]
        public Int16 DocType { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        [StringLength(5)]
        public String TranCurr { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
        public Decimal ExgRate { get; set; }
        public Boolean ExgRate_Plan { get; set; }
        [StringLength(5)]
        public String TranCurr2 { get; set; }
        public Decimal ExgRate2 { get; set; }
        public Boolean ExgRate_Plan2 { get; set; }
        public Decimal TranAmount { get; set; }
    }

    public class FinanceDocumentUIViewModel : FinanceDocumentViewModel
    {
        public String DocTypeName { get; set; }
        
        // Items
        public List<FinanceDocumentItemUIViewModel> Items = new List<FinanceDocumentItemUIViewModel>();
    }

    public sealed class FinanceADPDocumentUIViewModel: FinanceDocumentUIViewModel
    {
        // Account -> Advance payment
        public FinanceAccountViewModel AccountVM = new FinanceAccountViewModel();
    }

    public sealed class FinanceAssetDocumentUIViewModel: FinanceDocumentUIViewModel
    {
        // Account -> Advance payment
        public FinanceAccountViewModel AccountVM = new FinanceAccountViewModel();
    }

    public sealed class FinanceLoanDocumentUIViewModel: FinanceDocumentUIViewModel
    {
        // Account -> Loan account
        public FinanceAccountViewModel AccountVM = new FinanceAccountViewModel();
    }
}

