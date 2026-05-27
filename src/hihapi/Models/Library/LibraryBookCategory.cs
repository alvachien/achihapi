using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public enum LibraryBookCategoryEnum
    {
        OwnDefined = 0,
        Novel = 1
    }

    [Table("T_LIB_BOOKCTGY_DEF")]
    public class LibraryBookCategory : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INTEGER")]
        public int Id { get; set; }

        [Column("HID", TypeName = "INTEGER")]
        public Int32? HomeID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName = "NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(100)]
        [Column("COMMENT", TypeName = "NVARCHAR(100)")]
        public String Comment { get; set; }

        [Column("PARID", TypeName = "INTEGER")]
        public Int32? ParentID { get; set; }

        //[StringLength(45)]
        //[Column("VALUE", TypeName = "INTEGER")]
        //public LibraryBookCategoryEnum CategoryValue { get; set; }

        [ForeignKey("HomeID")]
        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryBook> Books { get; set; }
        public IList<LibraryBookCategoryLinkage> BookCategories { get; set; }
    }
}
