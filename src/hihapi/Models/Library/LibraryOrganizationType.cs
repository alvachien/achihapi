using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public enum LibraryOrganizationTypeEnum
    {
        OwnDefined = 0,
        PublishHouse = 1
    }

    //[Table("T_LIB_ORGTYPE")]
    public class LibraryOrganizationType
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
        [Column("VALUE", TypeName = "INT")]
        public LibraryOrganizationTypeEnum CategoryValue { get; set; }

        public HomeDefine CurrentHome { get; set; }

        //public ICollection<LibraryBook> Books { get; set; }
    }
}
