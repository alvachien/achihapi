using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace achihapi.ViewModels
{
    public sealed class FinanceDocItemSearchResultViewModel
    {
        public Int32 DocID { get; set; }
        [Required]
        public Int32 ItemID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        public String DocDesp { get; set; }
        [Required]
        public Int32 AccountID { get; set; }
        public Int32 TranType { get; set; }
        public String TranTypeName { get; set; }
        public Boolean TranType_Exp { get; set; }
        public Boolean UseCurr2 { get; set; }
        public String TranCurr { get; set; }
        [Required]
        public Decimal TranAmount { get; set; }
        public Decimal TranAmount_Org { get; set; }
        public Decimal TranAmount_LC { get; set; }
        public Int32 ControlCenterID { get; set; }
        public Int32 OrderID { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
    }

    public sealed class FinanceDocItemSearchResultListViewModel
    {
        // Runtime information
        public Int32 TotalCount { get; set; }
        public List<FinanceDocItemSearchResultViewModel> ContentList = new List<FinanceDocItemSearchResultViewModel>();

        public void Add(FinanceDocItemSearchResultViewModel tObj)
        {
            this.ContentList.Add(tObj);
        }

        public List<FinanceDocItemSearchResultViewModel>.Enumerator GetEnumerator()
        {
            return this.ContentList.GetEnumerator();
        }
    }
}
