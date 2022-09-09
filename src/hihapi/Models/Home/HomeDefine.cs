using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hihapi.Models.Library;

namespace hihapi.Models
{
    [Table("T_HOMEDEF")]
    public partial class HomeDefine : BaseModel
    {
        public HomeDefine(): base()
        {
            HomeMembers = new List<HomeMember>();
            FinanceAccountCategories = new List<FinanceAccountCategory>();
            FinanceAssetCategories = new List<FinanceAssetCategory>();
            FinanceDocumentTypes = new List<FinanceDocumentType>();
            FinanceTransactionTypes = new List<FinanceTransactionType>();
            FinanceAccounts = new List<FinanceAccount>();
            FinanceControlCenters = new List<FinanceControlCenter>();
            FinanceOrders = new List<FinanceOrder>();
            FinancePlans = new List<FinancePlan>();
            FinanceDocuments = new List<FinanceDocument>();
            PersonRoles = new List<LibraryPersonRole>();
            Persons = new List<LibraryPerson>();
        }

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

        public ICollection<HomeMember> HomeMembers { get; set; }
        // Finance
        public ICollection<FinanceAccountCategory> FinanceAccountCategories { get; set; }
        public ICollection<FinanceAssetCategory> FinanceAssetCategories { get; set; }
        public ICollection<FinanceDocumentType> FinanceDocumentTypes { get; set; }
        public ICollection<FinanceTransactionType> FinanceTransactionTypes { get; set; }
        public ICollection<FinanceAccount> FinanceAccounts { get; set; }
        public ICollection<FinanceControlCenter> FinanceControlCenters { get; set; }        
        public ICollection<FinanceOrder> FinanceOrders { get; set; }
        public ICollection<FinancePlan> FinancePlans { get; set; }
        public ICollection<FinanceDocument> FinanceDocuments { get; set; }
        public IList<LibraryPersonRole> PersonRoles { get; set; }
        public IList<LibraryPerson> Persons { get; set; }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context))
                return false;

            if (String.IsNullOrEmpty(Host))
                return false;
            if (String.IsNullOrEmpty(Name))
                return false;
            if (String.IsNullOrEmpty(BaseCurrency))
                return false;
            if (HomeMembers.Count <= 0)
                return false;
            var self = HomeMembers.First(p => p.Relation == HomeMemberRelationType.Self);
            if (self == null)
                return false;
            else
            {
                if (self.User != this.Host)
                    return false;
            }

            return true;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context))
                return false;

            // Check whether there still data exists
            // Account category
            var refcnt = 0;
            refcnt = context.FinAccountCategories.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Asset category
            refcnt = context.FinAssetCategories.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Document type
            refcnt = context.FinDocumentTypes.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Transaction type
            refcnt = context.FinTransactionType.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Accounts
            refcnt = context.FinanceAccount.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Control centers
            refcnt = context.FinanceControlCenter.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Orders
            refcnt = context.FinanceOrder.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;
            // Documents
            refcnt = context.FinanceDocument.Where(p => p.HomeID == this.ID).Count();
            if (refcnt > 0) return false;

            // TBD: library part

            return true;
        }
    }
}
