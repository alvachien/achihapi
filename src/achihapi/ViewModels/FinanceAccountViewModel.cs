using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public Byte Status { get; set; }

        // Ext ADP
        public FinanceAccountExtDPViewModel ExtraInfo_ADP { get; set; }
        // Ext Asset
        public FinanceAccountExtASViewModel ExtraInfo_AS { get; set; }
    }

    public class FinanceAccountUIViewModel : FinanceAccountViewModel
    {
        public string CtgyName { get; set; }
    }

    public abstract class FinanceAccountExtViewModel
    {
        public Int32 AccountID { get; set; }
    }

    public sealed class FinanceAccountExtDPViewModel: FinanceAccountExtViewModel
    {
        public Boolean Direct { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public Byte RptType { get; set; }
        public Int32 RefDocID { get; set; }
        [StringLength(100)]
        public String DefrrDays { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }

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
    }
}
