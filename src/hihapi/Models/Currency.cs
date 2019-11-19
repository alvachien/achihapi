using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("TFinCurrency")]
    public partial class Currency : BaseModel
    {
        [Key]
        [Required]
        [StringLength(5)]
        [Column("CURR", TypeName = "NVARCHAR(5)")]
        public string Curr { get; set; }
        [Required]
        [StringLength(45)]
        [Column("Name", TypeName = "NVARCHAR(45)")]
        public string Name { get; set; }
        [StringLength(30)]
        [Column("Symbol", TypeName = "NVARCHAR(30)")]
        public string Symbol { get; set; }
    }
}