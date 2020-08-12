using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.OData.Edm;

namespace hihapi.Models
{
    [Table("T_FIN_DOCUMENT")]
    public sealed class FinanceDocument: BaseModel
    {
        public FinanceDocument(): base()
        {
            TranDate = DateTime.Today;

            Items = new HashSet<FinanceDocumentItem>();
        }

        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("DOCTYPE", TypeName="SMALLINT")]
        public Int16 DocType { get; set; }
        
        [Required]
        [Column("TRANDATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime TranDate { get; set; }
        
        [Required]
        [StringLength(5)]
        [Column("TRANCURR", TypeName="NVARCHAR(5)")]
        public String TranCurr { get; set; }
        
        [Required]
        [StringLength(45)]
        [Column("DESP", TypeName="NVARCHAR(45)")]
        public String Desp { get; set; }
        
        [Column("EXGRATE", TypeName= "DECIMAL(17, 4)")]
        public Decimal ExgRate { get; set; }
        
        [Column("EXGRATE_PLAN", TypeName="BIT")]
        public Boolean ExgRate_Plan { get; set; }
        
        [StringLength(5)]
        [Column("TRANCURR2", TypeName="NVARCHAR(5)")]
        public String TranCurr2 { get; set; }
        
        [Column("EXGRATE2", TypeName = "DECIMAL(17, 4)")]
        public Decimal ExgRate2 { get; set; }
        
        [Column("EXGRATE_PLAN2", TypeName="BIT")]
        public Boolean ExgRate_Plan2 { get; set; }
        
        [NotMapped]
        public Decimal? TranAmount { get; set; }

        public HomeDefine CurrentHome { get; set; }
        public ICollection<FinanceDocumentItem> Items { get; set; }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context))
                return false;

            if (Items.Count == 0)
                return false;

            foreach (var item in Items)
            {
                if (!item.IsValid(context))
                    return false;
            }

            return true;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            return base.IsDeleteAllowed(context);
        }
    }

    [Table("T_FIN_DOCUMENT_ITEM")]
    public sealed class FinanceDocumentItem
    {
        public FinanceDocumentItem()
        {
        }

        [Key]
        [Column("DOCID", TypeName="INT")]
        public Int32 DocID { get; set; }
     
        [Key]
        [Column("ITEMID", TypeName="INT")]
        public Int32 ItemID { get; set; }
        
        [Required]
        [Column("ACCOUNTID", TypeName="INT")]
        public Int32 AccountID { get; set; }
        
        [Required]
        [Column("TRANTYPE", TypeName="INT")]
        public Int32 TranType { get; set; }
        
        [Required]
        [Column("TRANAMOUNT", TypeName="DECIMAL(17, 2)")]
        public Decimal TranAmount { get; set; }
        
        [Column("USECURR2", TypeName = "BIT")]
        public Boolean UseCurr2 { get; set; }

        [Column("CONTROLCENTERID", TypeName="INT")]
        public Int32? ControlCenterID { get; set; }

        [Column("ORDERID", TypeName = "INT")]
        public Int32? OrderID { get; set; }

        [StringLength(45)]
        [Column("DESP", TypeName = "NVARCHAR(45)")]
        public String Desp { get; set; }

        public FinanceDocument DocumentHeader { get; set; }

        public bool IsValid(hihDataContext context)
        {
            var isvalid = true;
            if (ItemID <= 0)
                isvalid = false;
            if (isvalid)
            {
                if (AccountID <= 0)
                    isvalid = false;
                else
                    isvalid = context.FinanceAccount.Where(p => p.ID == AccountID).Count() == 1;
            }
            if (isvalid)
            {
                if (TranType <= 0)
                    isvalid = false;
                else
                    isvalid = context.FinTransactionType.Where(p => p.ID == TranType).Count() == 1;
            }

            return isvalid;
        }
    }

    public sealed class FinanceADPDocumentCreateContext
    {
        public FinanceADPDocumentCreateContext()
        {
        }

        public FinanceDocument DocumentInfo { get; set; }
        public FinanceAccount AccountInfo { get; set; }
    }

    #region Loan related
    public sealed class FinanceLoanDocumentCreateContext
    {
        public FinanceDocument DocumentInfo { get; set; }
        public FinanceAccount AccountInfo { get; set; }
    }

    public sealed class FinanceLoanRepayDocumentCreateContext
    {
        public int LoanTemplateDocumentID { get; set; }
        public int HomeID { get; set; }
        public FinanceDocument DocumentInfo { get; set; }
    }
    public sealed class FinanceLoanPrepayDocumentCreateContext
    {
        public int HomeID { get; set; }
        public int LoanAccountID { get; set; }
        public FinanceDocument DocumentInfo { get; set; }
    }
    #endregion

    #region Asset related
    public abstract class FinanceAssetDocumentCreateContext
    {
        [Required]
        public Int32 HID { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        [StringLength(5)]
        public String TranCurr { get; set; }
        [Required]
        [StringLength(45)]
        public String Desp { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }

        // Items
        public List<FinanceDocumentItem> Items = new List<FinanceDocumentItem>();
    }

    // Asset: Buyin
    public sealed class FinanceAssetBuyDocumentCreateContext : FinanceAssetDocumentCreateContext
    {
        [Required]
        public Decimal TranAmount { get; set; }
        public Boolean? IsLegacy { get; set; }

        [Required]
        [StringLength(40)]
        public String AccountOwner { get; set; }

        public FinanceAccountExtraAS ExtraAsset { get; set; }
    }

    // Asset: Soldout
    public sealed class FinanceAssetSellDocumentCreateContext : FinanceAssetDocumentCreateContext
    {
        [Required]
        public Decimal TranAmount { get; set; }
        [Required]
        public Int32 AssetAccountID { get; set; }
    }

    // Asset: value change
    public sealed class FinanceAssetRevaluationDocumentCreateContext : FinanceAssetDocumentCreateContext
    {
        [Required]
        public Int32 AssetAccountID { get; set; }
    }
    #endregion

}
