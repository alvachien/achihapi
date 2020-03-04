using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_LEARN_OBJ")]
    public sealed class LearnObject : BaseModel
    {
        [Key]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }
        
        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("CATEGORY", TypeName = "INT")]
        public Int32 CategoryID { get; set; }

        [Required]
        [StringLength(45)]
        [Column("NAME", TypeName = "NVARCHAR(45)")]
        public String Name { get; set; }
        
        [Required]
        [Column("CONTENT", TypeName = "NVARCHAR(MAX)")]
        public String Content { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public LearnObject() : base()
        {
        }

        public override bool IsValid(hihDataContext context)
        {
            var bValid = base.IsValid(context);
            if (!bValid)
                return false;

            if (bValid && HomeID <= 0)
                return false;

            if (bValid && CategoryID <= 0)
                return false;

            if (bValid && String.IsNullOrEmpty(Name))
                bValid = false;

            if (bValid && String.IsNullOrEmpty(Content))
                bValid = false;

            if (bValid && context != null)
            {
                bValid = context.HomeDefines.Where(p => p.ID == HomeID).Count() == 1;
                if (bValid)
                {
                    bValid = context.LearnCategories
                        .Where(p => p.ID == CategoryID && (p.HomeID == null || p.HomeID == HomeID))
                        .Count() == 1;
                }
            }
            return bValid;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            return base.IsDeleteAllowed(context);
        }
    }
}
