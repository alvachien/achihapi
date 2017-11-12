using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace achihapi.ViewModels
{
    public class ADPGenerateViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RepeatFrequency RptType { get; set; }
        public Decimal TotalAmount { get; set; }
        public String Desp { get; set; }
    }

    public class ADPGenerateResult
    {
        public DateTime TranDate { get; set; }
        public Decimal TranAmount { get; set; }
        public String Desp { get; set; }
    }
}
