using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public String StartTimePoint
        {
            get { return this._startTimePoint.ToString(HIHAPIConstants.DateFormatPattern); }
            set { this._startTimePoint = DateTime.ParseExact(value, HIHAPIConstants.DateFormatPattern, null); }
        }
        private DateTime _startTimePoint;
        public DateTime StartTimePoint_DT {
            get { return this._startTimePoint; }
        }
        [Required]
        public String EndTimePoint
        {
            get { return this._endTimePoint.ToString(HIHAPIConstants.DateFormatPattern); }
            set { this._endTimePoint = DateTime.ParseExact(value, HIHAPIConstants.DateFormatPattern, null); }
        }
        private DateTime _endTimePoint;
        public DateTime EndTimePoint_DT { get { return this._endTimePoint; } }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Owner { get; set; }
        public Int32? RefID { get; set; }

        public String Tags { get; set; }
    }
}
