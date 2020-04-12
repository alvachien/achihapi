using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_BLOG_FORMAT")]
    public class BlogFormat
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int ID { get; set; }

        [Required]
        [StringLength(10)]
        [Column("Name", TypeName = "NVARCHAR(10)")]
        public string Name { get; set; }

        [StringLength(50)]
        [Column("Comment", TypeName = "NVARCHAR(50)")]
        public string Comment { get; set; }
    }
}
