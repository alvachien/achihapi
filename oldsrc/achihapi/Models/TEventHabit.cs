using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TEventHabit
    {
        public TEventHabit()
        {
            TEventHabitCheckin = new HashSet<TEventHabitCheckin>();
            TEventHabitDetail = new HashSet<TEventHabitDetail>();
        }

        public int Id { get; set; }
        public int Hid { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte Rpttype { get; set; }
        public bool IsPublic { get; set; }
        public string Content { get; set; }
        public int Count { get; set; }
        public string Assignee { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public ICollection<TEventHabitCheckin> TEventHabitCheckin { get; set; }
        public ICollection<TEventHabitDetail> TEventHabitDetail { get; set; }
    }
}
