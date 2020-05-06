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
        
        [Required]
        [Column("RELEASEDDATE")]
        [DataType(DataType.Date)]
        public DateTime ReleasedDate { get; set; }

        [Required]
        [Column("APPLIEDDATE")]
        [DataType(DataType.Date)]
        public DateTime AppliedDate { get; set; }
    }

    public sealed class CheckVersionResult
    {
        [Key]
        public string StorageVersion { get; set; }
        public string APIVersion { get; set; }
    }
}
