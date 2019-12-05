using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    public abstract class BaseModel {

        [Column("CREATEDAT")]
        [DataType(DataType.Date)]
        public DateTime? CreatedAt { get; set; }
        [Column("CREATEDBY", TypeName = "NVARCHAR(40)")]
        [StringLength(40)]
        public String Createdby { get; set; }
        [Column("UPDATEDAT")]
        [DataType(DataType.Date)]
        public DateTime? UpdatedAt { get; set; }
        [Column("UPDATEDBY", TypeName = "NVARCHAR(40)")]
        [StringLength(40)]
        public String Updatedby { get; set; }

        // IsValid
        // Check current model is valid from business perspective
        public virtual Boolean IsValid()
        {
            return true;
        }
    }
}
