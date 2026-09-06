using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    [Table("T_LIB_BOOK_READING_RECORD")]
    public class LibraryBookReadingRecord : BaseModel
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INTEGER")]
        public Int32 Id { get; set; }

        [Required]
        [Column("HID", TypeName = "INTEGER")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("BOOK_ID", TypeName = "INTEGER")]
        public int BookId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("USER", TypeName = "NVARCHAR(40)")]
        public String User { get; set; }

        [Column("FROMDATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Column("TODATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Column("COMMENT", TypeName = "NVARCHAR(50)")]
        [MaxLength(50)]
        public String Comment { get; set; }

        public override bool IsValid(hihDataContext context)
        {
            bool isvalid = base.IsValid(context);
            if (isvalid)
            {
                if (HomeID == 0)
                    isvalid = false;
                if (BookId == 0)
                    isvalid = false;
                if (String.IsNullOrEmpty(User))
                    isvalid = false;
                if (FromDate != null && ToDate != null)
                {
                    // Equal dates are allowed: a same-day reading is valid.
                    if (ToDate.Value < FromDate.Value)
                        isvalid = false;
                }
            }

            return isvalid;
        }
    }
}
