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

    [Table("T_LIB_ORGTYPE_DEF")]
    public class LibraryOrganizationType : BaseModel
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

        [StringLength(100)]
        [Column("COMMENT", TypeName = "NVARCHAR(100)")]
        public String Comment { get; set; }

        //[StringLength(45)]
        //[Column("VALUE", TypeName = "INT")]
        //public LibraryOrganizationTypeEnum CategoryValue { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryOrganization> Organizations { get; set; }

        public IList<LibraryOrganizationTypeLinkage> OrganizationTypes { get; set; }
    }

    [Table("t_lib_org_type")]
    public class LibraryOrganizationTypeLinkage
    {
        [Key]
        [Required]
        [Column("ORG_ID", TypeName = "INT")]
        public int OrganizationId { get; set; }

        [Key]
        [Required]
        [Column("TYPE_ID", TypeName = "INT")]
        public int TypeId { get; set; }

        public LibraryOrganization Organization { get; set; }
        public LibraryOrganizationType OrganizationType { get; set; }
    }
}
