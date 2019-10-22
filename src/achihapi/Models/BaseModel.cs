using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.Models
{
    public abstract class BaseModel
    {
        public BaseModel()
        {
            this.CreatedAt = DateTime.Now;
        }

        [StringLength(40)]
        public String CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        [StringLength(40)]
        public String UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Last error
        public String LastError { get; protected set; }
    }
}
