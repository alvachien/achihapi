using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    [Table("T_LIB_BOOK_AUTHOR")]
    public class LibraryBookAuthorLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INTEGER")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("AUTHOR_ID", TypeName = "INTEGER")]
        public int AuthorId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryPerson Author { get; set; }
    }

    [Table("T_LIB_BOOK_TRANSLATOR")]
    public class LibraryBookTranslatorLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INTEGER")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("TRANSLATOR_ID", TypeName = "INTEGER")]
        public int TranslatorId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryPerson Translator { get; set; }
    }

    [Table("T_LIB_BOOK_PRESS")]
    public class LibraryBookPressLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INTEGER")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("PRESS_ID", TypeName = "INTEGER")]
        public int PressId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryOrganization Press { get; set; }
    }

    [Table("T_LIB_BOOK_CTGY")]
    public class LibraryBookCategoryLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INTEGER")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("CTGY_ID", TypeName = "INTEGER")]
        public int CategoryId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryBookCategory Category { get; set; }
    }

    [Table("T_LIB_BOOK_LOCATION")]
    public class LibraryBookLocationLinkage
    {
        [Key]
        [Required]
        [Column("BOOK_ID", TypeName = "INTEGER")]
        public int BookId { get; set; }

        [Key]
        [Required]
        [Column("LOCATION_ID", TypeName = "INTEGER")]
        public int LocationId { get; set; }

        public LibraryBook Book { get; set; }
        public LibraryBookLocation Location { get; set; }
    }

    [Table("T_LIB_BOOK_DEF")]
    public class LibraryBook : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INTEGER")]
        public Int32 Id { get; set; }

        [Column("HID", TypeName = "INTEGER")]
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

        [Column("PUB_YEAR", TypeName = "INTEGER")]
        public Int32? PublishedYear { get; set; }

        [Column("DETAIL", TypeName = "NVARCHAR(200)")]
        public string Detail { get; set; }

        [Column("ORIGIN_LANG", TypeName = "INTEGER")]
        public Int32? OriginLangID { get; set; }
        [Column("BOOK_LANG", TypeName = "INTEGER")]
        public Int32? BookLangID { get; set; }

        [Column("PAGE_COUNT", TypeName = "INTEGER")]
        public Int32? PageCount { get; set; }

        // Physical copies the home currently holds. 0 means the book is gone
        // (lost, discarded, given away) but deliberately KEPT: reading and borrow
        // records reference it, so the title must survive. NULL is "not recorded"
        // (rows written before this column existed, or a create that omitted it)
        // and is read as still-owned - a missing value must never mean "gone", so
        // every consumer tests == 0 rather than falsy. On an update an omitted
        // value keeps whatever is recorded instead of resetting the column to
        // NULL, because a payload cannot say whether it meant to omit it
        // (see LibraryBooksController.Put).
        [Column("COPY_COUNT", TypeName = "INTEGER")]
        public Int32? CopyCount { get; set; }

        [ForeignKey("HomeID")]
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

        // Business-rule validation, called explicitly by the controller: the OData
        // binding path only enforces the DataAnnotations, and CopyCount's constraint
        // is not one of them (no [Range] on a value whose meaning lives in three
        // states rather than in an interval).
        public override bool IsValid(hihDataContext context)
        {
            bool isvalid = base.IsValid(context);

            // CopyCount has exactly three states - 0 (gone), NULL (not recorded) and
            // >0 (owned) - so a negative count is outside all of them. Left through,
            // it would drag LibraryOverviewKeyFigure.TotalCopies, documented as the
            // physical books on the shelf, below TotalBooks or below zero, and the
            // retired-row query (CopyCount eq 0) would never list the book.
            if (isvalid && CopyCount < 0)
            {
                isvalid = false;
            }

            return isvalid;
        }
    }
}
