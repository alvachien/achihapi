using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    [Table("T_LIB_BOOK_AUTHOR")]
    public class LibraryBookAuthorLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INT")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("AUTHOR_ID", TypeName = "INT")]
        public int AuthorId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryPerson Author { get; set; }
    }

    [Table("T_LIB_BOOK_TRANSLATOR")]
    public class LibraryBookTranslatorLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INT")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("TRANSLATOR_ID", TypeName = "INT")]
        public int TranslatorId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryPerson Translator { get; set; }
    }

    [Table("T_LIB_BOOK_PRESS")]
    public class LibraryBookPressLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INT")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("PRESS_ID", TypeName = "INT")]
        public int PressId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryOrganization Press { get; set; }
    }

    [Table("T_LIB_BOOK_CTGY")]
    public class LibraryBookCategoryLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INT")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("CTGY_ID", TypeName = "INT")]
        public int CategoryId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryBookCategory Category { get; set; }
    }

    [Table("T_LIB_BOOK_LOCATION")]
    public class LibraryBookLocationLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INT")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("LOCATION_ID", TypeName = "INT")]
        public int LocationId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryBookLocation Location { get; set; }
    }

    public class LibraryBookReadNote
    {
        [Key]
        [Required]
        //[Column("ID", TypeName = "INT")]
        public Int32 Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        [MaxLength(50)]
        //[Column("USER", TypeName = "NVARCHAR(50)")]
        public String User { get; set; }
    }

    [Table("T_LIB_BOOK_DEF")]
    public class LibraryBook: BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public Int32 Id { get; set; }

        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [StringLength(200)]
        [Column("NATIVE_NAME", TypeName = "NVARCHAR(200)")]
        public String NativeName { get; set; }

        [StringLength(200)]
        [Column("CHINESE_NAME", TypeName = "NVARCHAR(200)")]
        public String ChineseName { get; set; }

        [Column("ISCHN", TypeName = "BIT")]
        public Boolean? NativeIsChinese { get; set; }

        [StringLength(50)]
        [Column("ISBN", TypeName = "NVARCHAR(50)")]
        public String ISBN { get; set; }

        [Column("PUB_YEAR", TypeName = "INT")]
        public Int32? PublishedYear { get; set; }

        [Column("DETAIL", TypeName = "NVARCHAR(200)")]
        public string Detail { get; set; }

        [Column("ORIGIN_LANG", TypeName = "INT")]
        public Int32? OriginLangID { get; set; }
        [Column("BOOK_LANG", TypeName = "INT")]
        public Int32? BookLangID { get; set; }

        [Column("PAGE_COUNT", TypeName = "INT")]
        public Int32? PageCount { get; set; }

        public HomeDefine CurrentHome { get; set; }
        public IList<LibraryBookCategory> Categories { get; set; }
        public IList<LibraryBookCategoryLinkage> BookCategories { get; set; }
        public IList<LibraryBookLocation> Locations { get; set; }
        public IList<LibraryBookLocationLinkage> BookLocations { get; set; }
        public IList<LibraryPerson> Authors { get; set; }
        public IList<LibraryBookAuthorLinkage> BookAuthors { get; set; }
        public IList<LibraryPerson> Translators { get; set; }
        public IList<LibraryBookTranslatorLinkage> BookTranslators { get; set; }
        public IList<LibraryOrganization> Presses { get; set; }
        public IList<LibraryBookPressLinkage> BookPresses { get; set; }
    }
}
