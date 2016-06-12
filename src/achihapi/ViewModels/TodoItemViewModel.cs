using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class TodoItemViewModel
    {
        public Int32 TodoID { get; set; }
        [Required]
        [StringLength(50)]
        public String ItemName { get; set; }
        [Required]
        public Int32 Priority { get; set; }
        [StringLength(50)]
        public String Assignee { get; set; }
        [StringLength(50)]
        public String Dependence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String ItemContent { get; set; }
        [StringLength(50)]
        public String Tags { get; set; }

        public TodoItemViewModel()
        {
        }

        public TodoItemViewModel(TodoItem tdi) : this()
        {
            this.Assignee = tdi.Assignee;
            this.Dependence = tdi.Dependence;
            this.EndDate = tdi.EndDate;
            this.ItemContent = tdi.ItemContent;
            this.ItemName = tdi.ItemName;
            this.Priority = tdi.Priority;
            this.StartDate = tdi.StartDate;
            this.Tags = tdi.Tags;
            this.TodoID = tdi.ToDoID;
        }

        public TodoItem Convert2DB()
        {
            TodoItem tdi = new TodoItem();
            tdi.Assignee = this.Assignee;
            tdi.Dependence = this.Dependence;
            tdi.EndDate = this.EndDate;
            tdi.ItemContent = this.ItemContent;
            tdi.ItemName = this.ItemName;
            tdi.Priority = this.Priority;
            tdi.StartDate = this.StartDate;
            tdi.Tags = this.Tags;
            tdi.ToDoID = this.TodoID;

            return tdi;
        }
    }
}
