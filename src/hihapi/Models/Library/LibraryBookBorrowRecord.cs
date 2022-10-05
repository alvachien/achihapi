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
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("BOOK_ID", TypeName = "INT")]
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
                    if (ToDate.Value <= FromDate.Value)
                        isvalid = false;
                }
            }

            return isvalid;
        }
    }
}
