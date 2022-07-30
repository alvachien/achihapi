using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public class LibraryBook
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public Int32 Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column("NativeName", TypeName = "NVARCHAR(200)")]
        public String NativeName { get; set; }

        [StringLength(200)]
        [Column("ChineseName", TypeName = "NVARCHAR(200)")]
        public String ChineseName { get; set; }

        [StringLength(20)]
        [Column("ISBN", TypeName = "NVARCHAR(200)")]
        public String ISBN { get; set; }

        [Column("PublishedYear", TypeName = "DATE")]
        public Int32? PublishedYear { get; set; }

        public ICollection<LibraryBookCategory> Categories { get; set; }
    }
}
