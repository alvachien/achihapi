using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public enum EventTypeEnum: Byte
    {
        Normal = 1,
        HabitStyle = 2  // Not specify a fix date
    }

    public sealed class EventViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public DateTime StartTimePoint { get; set; }        
        public DateTime? EndTimePoint { get; set; }
        public DateTime? CompleteTimePoint { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Assignee { get; set; }
        public Int32? RefRecurrID { get; set; }

        public String Tags { get; set; }
    }

    public class RecurEventViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public DateTime StartTimePoint { get; set; }
        [Required]
        public DateTime EndTimePoint { get; set; }
        public RepeatFrequency RptType { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Assignee { get; set; }

        public String Tags { get; set; }
    }
    public sealed class RecurUIEventViewModel: RecurEventViewModel
    {
        public List<EventViewModel> EventList { get; private set; }
        public RecurUIEventViewModel()
        {
            EventList = new List<EventViewModel>();
        }           
    }

    public sealed class EventGenerationInputViewModel
    {
        public DateTime StartTimePoint { get; set; }
        public DateTime EndTimePoint { get; set; }
        public RepeatFrequency RptType { get; set; }
        public String Name { get; set; }
    }

    public sealed class EventGenerationResultViewModel
    {
        public DateTime StartTimePoint { get; set; }
        public DateTime EndTimePoint { get; set; }
        public String Name { get; set; }
    }

    public sealed class EventHabitDetail
    {
        public Int32 ID { get; set; }
        public Int32 HabitID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public sealed class EventHabitViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public RepeatFrequency RptType { get; set; }
        public Int32 Count { get; set; }
        public String Content { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(40)]
        public String Assignee { get; set; }

        public String Tags { get; set; }

        public List<EventHabitDetail> Details { get; private set; }
        public List<EventHabitCheckInViewModel> CheckInLogs { get; private set; }

        public EventHabitViewModel()
        {
            Details = new List<EventHabitDetail>();
            CheckInLogs = new List<EventHabitCheckInViewModel>();
        }
    }

    public sealed class EventHabitCheckInViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        public Int32 HabitID { get; set; }
        [Required]
        public Int32 Score { get; set; }
        [StringLength(50)]
        public String Comment { get; set; }
    }
}
