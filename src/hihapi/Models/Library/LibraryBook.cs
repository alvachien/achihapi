using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    //[Table("T_LIB_BOOK_DEF")]
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

        [Column("ISCHN", TypeName = "BIT")]
        public Boolean NativeIsChinese { get; set; }

        [StringLength(20)]
        [Column("ISBN", TypeName = "NVARCHAR(200)")]
        public String ISBN { get; set; }

        [Column("PublishedYear", TypeName = "DATE")]
        public Int32? PublishedYear { get; set; }

        [Column("DETAIL", TypeName = "NVARCHAR(200)")]
        public string Detail { get; set; }

        [Column("ORIGIN_LANG", TypeName = "INT")]
        public Int32? OriginLangID { get; set; }
        [Column("BOOK_LANG", TypeName = "INT")]
        public Int32? BookLangID { get; set; }

        [Column("PAGE_COUNT", TypeName = "INT")]
        public Int32? PageCount { get; set; }

        public IList<LibraryBookCategory> Categories { get; set; }
    }
}
