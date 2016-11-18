using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public abstract class BaseViewModel
    {
        [StringLength(40)]
        public String CreatedBy { get; set;  }
        public DateTime CreatedAt { get; set; }
        [StringLength(40)]
        public String UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BaseListViewModel<T> where T : BaseViewModel
    {
        // Runtime information
        public Int32 TotalCount { get; set; }
        public List<T> ContentList = new List<T>();

        public void Add(T tObj)
        {
            this.ContentList.Add(tObj);
        }
    }
}
