using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class KnowledgeTypeViewModel
    {
        public Int32 ID { get; set; }
        public Int32? ParentID { get; set; }
        [Required]
        public String Name { get; set; }
        public String Comment { get; set; }
    }
}
