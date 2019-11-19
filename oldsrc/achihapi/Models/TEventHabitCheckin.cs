using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TEventHabitCheckin
    {
        public int Id { get; set; }
        public DateTime TranDate { get; set; }
        public int HabitId { get; set; }
        public int? Score { get; set; }
        public string Comment { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public TEventHabit Habit { get; set; }
    }
}
