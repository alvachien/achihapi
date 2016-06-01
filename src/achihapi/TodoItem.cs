using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class TodoItem
    {
        public int ToDoID { get; set; }
        public string ItemName { get; set; }
        public int Priority { get; set; }
        public string Assignee { get; set; }
        public string Dependence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ItemContent { get; set; }
        public string Tags { get; set; }
    }
}
