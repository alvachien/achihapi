using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class KnowledgeTypeViewModel
    {
        [Required]
        public Int16 ID { get; set; }
        public Int16? ParentID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        [StringLength(100)]
        public String Comment { get; set; }
    }
}
