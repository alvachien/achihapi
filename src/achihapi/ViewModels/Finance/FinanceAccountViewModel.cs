using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public enum FinanceAccountStatus : Byte
    {
        Normal = 0,
        Closed = 1,
        Frozen = 2
    }

    public class FinanceAccountViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        public Int32 CtgyID { get; set; }
        [StringLength(30)]
        public String Name { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
        [StringLength(40)]
        public String Owner { get; set; }
        public FinanceAccountStatus Status { get; set; }

        // Ext ADP
        public FinanceAccountExtDPViewModel ExtraInfo_ADP { get; set; }
        // Ext Asset
        public FinanceAccountExtASViewModel ExtraInfo_AS { get; set; }
        // Ext Loan
        public FinanceAccountExtLoanViewModel ExtraInfo_Loan { get; set; }
    }

    public class FinanceAccountUIViewModel : FinanceAccountViewModel
    {
        public string CtgyName { get; set; }
    }

    public abstract class FinanceAccountExtViewModel
    {
        public Int32 AccountID { get; set; }
    }

    public enum RepeatFrequency : Byte
    {
        Month = 0,
        Fortnight = 1,
        Week = 2,
        Day = 3,
        Quarter = 4,
        HalfYear = 5,
        Year = 6,
        Manual = 7,
    }

    // Account extra: advance payment
    public sealed class FinanceAccountExtDPViewModel: FinanceAccountExtViewModel
    {
        public Boolean Direct { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public RepeatFrequency RptType { get; set; }
        public Int32 RefDocID { get; set; }
        [StringLength(100)]
        public String DefrrDays { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
        // Tmp. docs
        public List<FinanceTmpDocDPViewModel> DPTmpDocs { get; }

        public FinanceAccountExtDPViewModel()
        {
            this.DPTmpDocs = new List<FinanceTmpDocDPViewModel>();
        }
    }

    // Account extra: Assert
    public sealed class FinanceAccountExtASViewModel: FinanceAccountExtViewModel
    {
        public Int32 CategoryID { get; set; }
        [StringLength(50)]
        [Required]
        public String Name { get; set; }
        [StringLength(50)]
        public String Comment { get; set; }
        [Required]
        public Int32 RefDocForBuy { get; set; }
        public Int32? RefDocForSold { get; set; }

        // Only for creation mode, will not save to DB!
        public Boolean? LegacyAsset { get; set; }
        public DateTime? AssetStartDate { get; set; }
        public Decimal? AssetValueInBaseCurrency { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
    }

    public enum LoanRepaymentMethod
    {
        EqualPrincipalAndInterset = 1,  // Equal principal & interest
        EqualPrincipal = 2,  // Equal principal
        DueRepayment = 3  // Due repayment
    }

    // Account extra: Loan (Borrow from, or Lend to)
    public sealed class FinanceAccountExtLoanViewModel: FinanceAccountExtViewModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        public Decimal? AnnualRate { get; set; }
        public Boolean? InterestFree { get; set; }
        public LoanRepaymentMethod? RepaymentMethod { get; set; }
        public Int16? TotalMonths { get; set; }
        [Required]
        public Int32 RefDocID { get; set; }
        [StringLength(100)]
        public String Others { get; set; }
        public DateTime? EndDate { get; set; }
        //[Required]
        //public Boolean IsLendOut { get; set; }
        public Int32? PayingAccount { get; set; }
        [StringLength(50)]
        public String Partner { get; set; }
        // Tmp. docs
        public List<FinanceTmpDocLoanViewModel> LoanTmpDocs { get; }

        public FinanceAccountExtLoanViewModel()
        {
            //// Default
            //this.IsLendOut = false;
            this.LoanTmpDocs = new List<FinanceTmpDocLoanViewModel>();
        }
    }

    // Account extra: Creditcard
    public sealed class FinanceAccountExtCCViewModel : FinanceAccountExtViewModel
    {
        [Required]
        public Int16 BillDayInMonth { get; set; }
        [Required] 
        public Int16 LastPayDayInMonth { get; set; }
        [Required]
        [StringLength(20)]
        public String CardNumber { get; set; }
        [StringLength(50)]
        public String Bank { get; set; }
        [StringLength(100)]
        public String Others { get; set; }
        public DateTime? ValidDate { get; set; }
    }
}
