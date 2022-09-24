using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    [Table("T_LIB_BOOK_BORROW_RECORD")]
    public class LibraryBookBorrowRecord: BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public Int32 Id { get; set; }

        [Required]
        public Int32 HomeID { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("USER", TypeName = "NVARCHAR(40)")]
        public String User { get; set; }

        [Column("FROMORG", TypeName = "INT")]
        public int? FromOrganization { get; set; }

        [Column("FROMDATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Column("TODATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Required]
        [Column("ISRETURNED", TypeName = "BIT")]
        public Boolean IsReturned { get; set; }

        [Column("COMMENT", TypeName = "NVARCHAR(50)")]
        [MaxLength(50)]
        public String Comment { get; set; }
    }
}
