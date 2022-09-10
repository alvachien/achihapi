using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    [Table("T_LIB_ORG_DEF")]
    public class LibraryOrganization : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int Id { get; set; }

        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("NATIVE_NAME", TypeName = "NVARCHAR(100)")]
        public string NativeName { get; set; }
        [Column("CHINESE_NAME", TypeName = "NVARCHAR(200)")]
        public string ChineseName { get; set; }

        [Column("ISCHN", TypeName = "BIT")]
        public Boolean? NativeIsChinese { get; set; }

        [Column("DETAIL", TypeName = "NVARCHAR(200)")]
        public string Detail { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryOrganizationType> Types{ get; set; }
        public IList<LibraryOrganizationTypeLinkage> OrganizationTypes { get; set; }
        public IList<LibraryBook> Books { get; set; }
        public IList<LibraryBookPressLinkage> PressedBooks { get; set; }
    }
}
