using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_DBVERSION")]
    public class DBVersion
    {
        [Key]
        [Column("VERSIONID", TypeName="INT")]
        public Int32 VersionID { get; set; }
        public DateTime ReleasedDate { get; set; }
        public DateTime AppliedDate { get; set; }
    }
}
