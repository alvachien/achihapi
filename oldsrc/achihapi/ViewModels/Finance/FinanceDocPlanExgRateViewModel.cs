using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class FinanceDocPlanExgRateViewModel
    {
        public Int32 HID { get; set; }
        public Int32 DocID { get; set; }
        public Int16 DocType { get; set; }
        public DateTime TranDate { get; set; }
        public String Desp { get; set; }
        public String TranCurr { get; set; }
        public Decimal ExgRate { get; set; }
        public Boolean ExgRate_Plan { get; set; }

        public String TranCurr2 { get; set; }
        public Decimal ExgRate2 { get; set; }
        public Boolean ExgRate_Plan2 { get; set; }

    }

    public class FinanceDocPlanExgRateForUpdViewModel
    {
        public Int32 HID { get; set; }
        public String TargetCurrency { get; set; }
        public Decimal ExchangeRate { get; set; }
        public List<Int32> DocIDs = new List<int>();
    }
}
