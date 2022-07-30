using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public class LibraryBookCategory
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int Id { get; set; }

        [Column("HID", TypeName = "INT")]
        public Int32? HomeID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName = "NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName = "NVARCHAR(45)")]
        public String Comment { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public ICollection<LibraryBook> Books { get; set; }
    }
}
