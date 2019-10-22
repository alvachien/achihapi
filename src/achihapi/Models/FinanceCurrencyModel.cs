using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.Models
{
    public class FinanceCurrencyModel : BaseModel
    {
        [Required]
        [Key]
        [StringLength(5)]
        public String Curr { get; set; }
        [Required]
        [StringLength(45)]
        public String Name { get; set; }
        [StringLength(30)]
        public String Symbol { get; set; }
    }
}
