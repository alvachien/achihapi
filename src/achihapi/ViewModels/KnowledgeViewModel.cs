using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class KnowledgeViewModel
    {
        public Int32 ID { get; set; }
        public Int16? TypeID { get; set; }
        [Required]
        [StringLength(50)]
        public String Title { get; set; }
        [Required]
        public String Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        [StringLength(100)]
        public String Tags { get; set; }

        // Runtime info

    }
}
