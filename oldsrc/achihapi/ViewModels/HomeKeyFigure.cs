using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class HomeKeyFigure
    {
        public Decimal TotalAsset { get; set; }
        public Decimal TotalLiability { get; set; }
        public Decimal TotalAssetUnderMyName { get; set; }
        public Decimal TotalLiabilityUnderMyName { get; set; }

        public Int32 TotalUnreadMessage { get; set; }

        public Int32 MyUnCompletedEvents { get; set; }
        //public Int32 MyUnCompletedEventInNextWeek { get; set; }
        //public Int32 MyEventInNextMonth { get; set; }
        //public Int32 MyEventInPreviousWeek { get; set; }
        public Int32 MyCompletedEvents { get; set; }
    }
}
