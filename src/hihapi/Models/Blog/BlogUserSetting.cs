using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Blog
{
    [Table("T_BLOG_SETTING")]
    public class BlogUserSetting
    {
        [Key]
        [Required]
        [Column("Owner", TypeName = "NVARCHAR(40)")]
        public string Owner { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Name", TypeName = "NVARCHAR(50)")]
        public string Name { get; set; }

        [StringLength(50)]
        [Column("Comment", TypeName = "NVARCHAR(50)")]
        public string Comment { get; set; }

        [Column("ALLOWCOMMENT", TypeName = "BIT")]
        public bool? AllowComment { get; set; }
    }
}
