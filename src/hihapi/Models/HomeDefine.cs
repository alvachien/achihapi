using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_HOMEDEF")]
    public partial class HomeDefine : BaseModel
    {
        [Key]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("NAME", TypeName = "NVARCHAR(50)")]
        public String Name { get; set; }

        [MaxLength(50)]
        [Column("DETAILS", TypeName = "NVARCHAR(50)")]
        public String Details { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("HOST", TypeName = "NVARCHAR(50)")]
        public String Host { get; set; }
        
        [Required]
        [MaxLength(5)]
        [Column("BASECURR", TypeName = "NVARCHAR(5)")]
        public String BaseCurrency { get; set; }

        // For creation - which need create a home member automatically
        [NotMapped]
        public String CreatorDisplayAs { get; set; }

        public ICollection<HomeMember> HomeMembers { get; set; }
        //public ICollection<FinanceAccountCategory> FinanceAccountCategories { get; set; }
        //public ICollection<FinanceAssetCategory> FinanceAssetCategories { get; set; }
        //public ICollection<FinanceDocumentType> FinanceDocumentTypes { get; set; }
        //public ICollection<FinanceTransactionType> FinanceTransactionTypes { get; set; }
        public ICollection<FinanceAccount> FinanceAccounts { get; set; }
        //public ICollection<FinanceAccountExtraDP> FinanceAccountExtraDPs { get; set; }
        //public ICollection<FinanceAccountExtraAS> FinanceAccountExtraASs { get; set; }
        //public ICollection<FinanceControlCenter> FinanceControlCenters { get; set; }        
        //public ICollection<FinanceOrder> FinanceOrders { get; set; }
        //public ICollection<FinanceDocument> FinanceDocuments { get; set; }
    }
}
