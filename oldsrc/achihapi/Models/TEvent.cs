using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TEvent
    {
        public int Id { get; set; }
        public int Hid { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CompleteTime { get; set; }
        public string Content { get; set; }
        public bool? IsPublic { get; set; }
        public string Assignee { get; set; }
        public int? RefRecurId { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
    }
}
