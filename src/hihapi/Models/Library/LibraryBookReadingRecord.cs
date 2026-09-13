using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    /// <summary>
    /// Lifecycle state of a reading record: Reading is open-ended (no ToDate),
    /// Completed and Aborted are terminal and can only be reached from Reading.
    /// </summary>
    public enum LibraryBookReadingStatus : Byte
    {
        Reading = 0,
        Completed = 1,
        Aborted = 2,
    }

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
        [MaxLength(40)]   // aligned with the NVARCHAR(40) column (was 50)
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

        [Required]
        [Column("STATUS", TypeName = "INTEGER")]
        public LibraryBookReadingStatus Status { get; set; }

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

                // A reading record always knows when it started; the end date is
                // governed by the lifecycle status below.
                if (FromDate == null)
                {
                    isvalid = false;
                }
                else
                {
                    switch (Status)
                    {
                        case LibraryBookReadingStatus.Reading:
                            // Open-ended by definition: the end date arrives via
                            // CompleteReading / AbortReading.
                            if (ToDate != null)
                                isvalid = false;
                            break;

                        case LibraryBookReadingStatus.Completed:
                            if (ToDate == null)
                                isvalid = false;
                            break;

                        case LibraryBookReadingStatus.Aborted:
                            // ToDate optional: an abandoned book may have no end date.
                            break;

                        default:
                            isvalid = false;
                            break;
                    }

                    // Equal dates are allowed: a same-day reading is valid.
                    if (isvalid && ToDate != null && ToDate.Value < FromDate.Value)
                        isvalid = false;
                }
            }

            return isvalid;
        }

        /// <summary>
        /// Only a Reading record can be finalized (completed or aborted);
        /// Completed and Aborted are terminal.
        /// </summary>
        public bool IsFinalizeAllowed(hihDataContext context)
        {
            _ = context;
            return Status == LibraryBookReadingStatus.Reading
                && FromDate != null;
        }
    }
}
