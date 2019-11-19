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
            this.Createdat = DateTime.Now;
        }

        [StringLength(40)]
        public String Createdby { get; set; }
        public DateTime Createdat { get; set; }
        [StringLength(40)]
        public String Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }
    }
}
