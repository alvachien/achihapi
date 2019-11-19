using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace achihapi.Models
{
    public partial class TFinCurrency : BaseModel
    {
        [Key]
        [Required]
        [StringLength(5)]
        public string Curr { get; set; }
        [Required]
        [StringLength(45)]
        public string Name { get; set; }
        [StringLength(30)]
        public string Symbol { get; set; }
    }
}
