using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_BLOG_POST_TAG")]
    public class BlogPostTag
    {
        [Key]
        [Required]
        [Column("PostID", TypeName = "INT")]
        public int PostID { get; set; }

        [Key]
        [Required]
        [Column("Tag", TypeName = "NVARCHAR(20)")]
        public string Tag { get; set; }

        public BlogPost BlogPost { get; set; }
    }
}
