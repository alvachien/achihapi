using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_BLOG_POST_COLL")]
    public class BlogPostCollection
    {
        [Key]
        [Required]
        [Column("PostID", TypeName = "INT")]
        public int PostID { get; set; }

        [Key]
        [Required]
        [Column("CollID", TypeName = "INT")]
        public int CollectionID { get; set; }
    }
}
