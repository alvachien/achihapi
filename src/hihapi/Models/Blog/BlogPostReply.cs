using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_BLOG_POST_REPLY")]
    public class BlogPostReply
    {
        [Key]
        [Required]
        [Column("PostID", TypeName = "INT")]
        public int PostID { get; set; }

        [Key]
        [Required]
        [Column("ReplyID", TypeName = "INT")]
        public int ReplyID { get; set; }

        [Required]
        [MaxLength(40)]
        [Column("RepliedBy", TypeName = "NVARCHAR(40)")]
        public string RepliedBy { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("RepliedIP", TypeName = "NVARCHAR(20)")]
        public string RepliedIP { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Title", TypeName = "NVARCHAR(100)")]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Content", TypeName = "NVARCHAR(200)")]
        public string Content { get; set; }

        [Column("RefReplyID", TypeName = "INT")]
        public int? RefReplyID { get; set; }

        [Column("CREATEDAT")]
        [DataType(DataType.Date)]
        public DateTime? CreatedAt { get; set; }
    }
}
