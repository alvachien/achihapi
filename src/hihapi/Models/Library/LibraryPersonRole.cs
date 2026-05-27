using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public enum LibraryPersonRoleEnum
    {
        OwnDefined = 0,
        Author = 1,
        Actor = 2,
        Director = 3,
    }

    [Table("t_lib_personrole_def")]
    public class LibraryPersonRole : BaseModel
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

        //[StringLength(45)]
        //[Column("VALUE", TypeName = "INTEGER")]
        //public LibraryPersonRoleEnum CategoryValue { get; set; }

        [ForeignKey("HomeID")]
        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryPerson> Persons { get; set; }
        public IList<LibraryPersonRoleLinkage> PersonRoles { get; set; }
    }

    [Table("t_lib_person_role")]
    public class LibraryPersonRoleLinkage
    {
        [Key]
        [Required]
        [Column("PERSON_ID", TypeName = "INTEGER")]
        public int PersonId { get; set; }

        [Key]
        [Required]
        [Column("ROLE_ID", TypeName = "INTEGER")]
        public int RoleId { get; set; }

        public LibraryPerson Person { get; set; }
        public LibraryPersonRole Role { get; set; }
    }
}
