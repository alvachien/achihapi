using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_BLOG_POST")]
    public class BlogPost
    {
        public const int BlogPostStatus_Draft = 1;
        public const int BlogPostStatus_PublishAsPublic = 2;
        public const int BlogPostStatus_PublishAsPrivate = 3;
        public const int BlogPostStatus_Deleted = 4;

        public BlogPost()
        {
            BlogPostCollections = new HashSet<BlogPostCollection>();
            BlogPostTags = new HashSet<BlogPostTag>();
        }

        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int ID { get; set; }

        [Required]
        [Column("Owner", TypeName = "NVARCHAR(40)")]
        public string Owner { get; set; }

        [Required]
        [Column("FORMAT", TypeName = "INT")]
        public int Format { get; set; }

        [Required]
        [Column("Title", TypeName = "NVARCHAR(50)")]
        public string Title { get; set; }

        [Required]
        [Column("Brief", TypeName = "NVARCHAR(100)")]
        public string Brief { get; set; }

        [Required]
        [Column("Content", TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; }

        [Required]
        [Column("Status", TypeName = "INT")]
        public int Status { get; set; }

        [Column("CREATEDAT")]
        [DataType(DataType.Date)]
        public DateTime? CreatedAt { get; set; }

        [Column("UPDATEDAT")]
        [DataType(DataType.Date)]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<BlogPostCollection> BlogPostCollections { get; set; }
        public ICollection<BlogPostTag> BlogPostTags { get; set; }
    }
}
