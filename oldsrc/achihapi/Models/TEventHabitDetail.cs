using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TEventHabitDetail
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TEventHabit Habit { get; set; }
    }
}
