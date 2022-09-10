using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public enum LibraryBookLocationTypeEnum
    {
        PaperBook = 0,
        Ebook = 1,
    }

    [Table("t_lib_bookloc_def")]
    public class LibraryBookLocation : BaseModel
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

        [Required]
        [Column("LOCTYPE", TypeName = "INT")]
        public int LocationType { get; set; }

        [StringLength(100)]
        [Column("COMMENT", TypeName = "NVARCHAR(100)")]
        public String Comment { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryBook> Books { get; set; }
        public IList<LibraryBookLocationLinkage> BooksInLocation { get; set; }
    }
}
