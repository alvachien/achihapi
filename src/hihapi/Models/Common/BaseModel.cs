using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.OData.Edm;

namespace hihapi.Models
{
    public abstract class BaseModel 
    {
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

        #region Virtual Methods
        // IsValid
        // Check current model is valid from business perspective
        public virtual Boolean IsValid(hihDataContext context)
        {
            return true;
        }

        // IsDeleteAllowed
        // Check current model is allowed to delete
        public virtual Boolean IsDeleteAllowed(hihDataContext context)
        {
            return true;
        }
        #endregion
    }
}
