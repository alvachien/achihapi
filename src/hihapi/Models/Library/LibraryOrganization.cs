using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public class LibraryOrganization
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int Id { get; set; }

        [Column("NATIVE_NAME", TypeName = "NVARCHAR(100)")]
        public string NativeName { get; set; }
        [Column("CHINESE_NAME", TypeName = "NVARCHAR(200)")]
        public string ChineseName { get; set; }

        [Column("ISCHN", TypeName = "BIT")]
        public Boolean NativeIsChinese { get; set; }

        [Column("DETAIL", TypeName = "NVARCHAR(200)")]
        public string Detail { get; set; }
    }
}
