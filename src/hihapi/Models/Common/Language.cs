using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_LANGUAGE")]
    public partial class Language
    {        
        [Key]
        [Column("LCID", TypeName = "INT")]
        public int Lcid { get; set; }

        [Required]
        [StringLength(20)]
        [Column("ISONAME", TypeName = "NVARCHAR(20)")]
        public string ISOName { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ENNAME", TypeName = "NVARCHAR(100)")]
        public string EnglishName { get; set; }

        [Required]
        [StringLength(100)]
        [Column("NAVNAME", TypeName = "NVARCHAR(100)")]
        public string NativeName { get; set; }

        [Column("APPFLAG", TypeName = "BIT")]
        public bool? AppFlag { get; set; }
    }
}