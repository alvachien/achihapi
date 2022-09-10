using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    [Table("T_LIB_PERSON_DEF")]
    public class LibraryPerson : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int Id { get; set; }

        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("NATIVE_NAME", TypeName = "NVARCHAR(100)")]
        public string NativeName { get; set; }

        [StringLength(100)]
        [Column("CHINESE_NAME", TypeName = "NVARCHAR(100)")]
        public string ChineseName { get; set; }

        [Column("ISCHN", TypeName = "BIT")]
        public Boolean? NativeIsChinese { get; set; }

        [Column("DETAIL", TypeName = "NVARCHAR(200)")]
        public string Detail { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryPersonRole> Roles { get; set; }
        public IList<LibraryPersonRoleLinkage> PersonRoles { get; set; }
        public IList<LibraryBook> WritenBooks { get; set; }
        public IList<LibraryBookAuthorLinkage> WrittenBooksByAuthor { get; set; }
        public IList<LibraryBook> TranslatedBooks { get; set; }
        public IList<LibraryBookTranslatorLinkage> TranslatedBooksByTranslator { get; set; }
    }
}
