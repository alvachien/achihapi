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
        public Int32? TypeID { get; set; }
        [Required]
        public String Title { get; set; }
        [Required]
        public String Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public String Tags { get; set; }

        // Runtime info

    }
}
