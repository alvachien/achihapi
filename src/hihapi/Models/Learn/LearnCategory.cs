using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_LEARN_CTGY")]
    public sealed class LearnCategory: BaseModel
    {
        [Key]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }

        [Column("HID", TypeName = "INT")]
        public Int32? HomeID { get; set; }

        [Column("PARID", TypeName = "INT")]
        public Int32? ParentID { get; set; }

        [Required]
        [StringLength(45)]
        [Column("NAME", TypeName = "NVARCHAR(45)")]
        public String Name { get; set; }
        
        [StringLength(50)]
        [Column("COMMENT", TypeName = "NVARCHAR(50)")]
        public String Comment { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public LearnCategory(): base()
        {
        }

        public override bool IsValid(hihDataContext context)
        {
            var bValid = base.IsValid(context);
            if (!bValid)
                return false;

            if (String.IsNullOrEmpty(Name))
                bValid = false;

            //if (bValid && !HomeID.HasValue)
            //    bValid = false;

            if (bValid && ParentID.HasValue)
            {
                if (!HomeID.HasValue)
                    bValid = false;
            }

            if (bValid && context != null && (HomeID.HasValue || ParentID.HasValue))
            {
                if (HomeID.HasValue)
                    bValid = context.HomeDefines.Where(p => p.ID == HomeID.Value).Count() == 1;
                if (bValid && ParentID.HasValue)
                    bValid = context.LearnCategories
                        .Where(p => p.ID == ParentID.Value && (p.HomeID == null || p.HomeID == HomeID.Value))
                        .Count() == 1;
            }

            return bValid;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context))
                return false;

            return true;
        }
    }
}
