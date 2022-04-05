using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_DOCUMENT")]
    public sealed class FinanceDocument: BaseModel
    {
        public FinanceDocument(): base()
        {
            TranDate = DateTime.Today;

            Items = new List<FinanceDocumentItem>();
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
        public Decimal? ExgRate { get; set; }
        
        [Column("EXGRATE_PLAN", TypeName="BIT")]
        public Boolean? ExgRate_Plan { get; set; }
        
        [StringLength(5)]
        [Column("TRANCURR2", TypeName="NVARCHAR(5)")]
        public String TranCurr2 { get; set; }
        
        [Column("EXGRATE2", TypeName = "DECIMAL(17, 4)")]
        public Decimal? ExgRate2 { get; set; }
        
        [Column("EXGRATE_PLAN2", TypeName="BIT")]
        public Boolean? ExgRate_Plan2 { get; set; }
        
        [NotMapped]
        public Decimal? TranAmount { get; set; }

        public HomeDefine CurrentHome { get; set; }
        public ICollection<FinanceDocumentItem> Items { get; set; }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context))
                return false;

            // HomeID
            if (this.HomeID <= 0)
                return false;
            // Doc. type
            if (this.DocType <= 0)
                return false;
            // Currency
            if (String.IsNullOrEmpty(this.TranCurr))
                return false;
            // Desp
            if (String.IsNullOrEmpty(this.Desp))
                return false;

            if (Items.Count == 0)
                return false;

            foreach (var item in Items)
            {
                if (!item.IsValid(context))
                    return false;
            }

            // Additional checks based on doc. type.
            if (this.DocType == FinanceDocumentType.DocType_Transfer)
            {
                Decimal inamt = 0;
                Decimal outamt = 0;
                // On Item level
                foreach(var item in Items)
                {
                    if (item.TranType == FinanceTransactionType.TranType_TransferIn)
                        inamt += item.TranAmount;
                    else if (item.TranType == FinanceTransactionType.TranType_TransferOut)
                        outamt += item.TranAmount;
                    else
                        return false;
                }

                // Total amount
                if (inamt != outamt)
                    return false;
            }

            return true;
        }

        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context))
                return false;

            // Doc has been used in Tempalte DP
            if (UsedInDPCreating(context)) return false;

            // Doc has been used in Template Asset
            if (UsedInAsset(context)) return false;

            return true;
        }

        public bool IsChangeAllowed(hihDataContext context)
        {
            // Doc type: Only normal doc allows the change
            if (this.DocType != FinanceDocumentType.DocType_Normal)
                return false;

            // Doc has been used in Template DP?
            var usedInDP = (from dp in context.FinanceTmpDPDocument
                            join doc in context.FinanceDocument
                            on dp.ReferenceDocumentID equals doc.ID
                            where doc.ID == this.ID && dp.HomeID == this.HomeID && doc.HomeID == this.HomeID
                            select doc).Count();
            if (usedInDP > 0) return false;

            // Doc has been used in Template Loan
            var usedInLoan = (from dp in context.FinanceTmpLoanDocument
                              join doc in context.FinanceDocument
                              on dp.ReferenceDocumentID equals doc.ID
                              where doc.ID == this.ID && dp.HomeID == this.HomeID && doc.HomeID == this.HomeID
                              select doc).Count();
            if (usedInLoan > 0) return false;

            // Doc has been used in Template Asset
            if (UsedInAsset(context))
                return false;

            return true;
        }

        private bool UsedInDPCreating(hihDataContext context)
        {
            var doccnt = (from acnt in context.FinanceAccountExtraDP
                          where acnt.RefenceDocumentID == this.ID
                          select acnt).Count();
            if (doccnt > 0)
            {
                var tmpdoccnt = (from dpdoc in context.FinanceTmpDPDocument
                                where dpdoc.ReferenceDocumentID != null
                                select dpdoc).Count();
                if(tmpdoccnt > 0) return true;
            }

            return false;
        }

        private bool UsedInAsset(hihDataContext context)
        {
            var usedInAsset = (from dp in context.FinanceAccountExtraAS
                               join acnt in context.FinanceAccount
                                on dp.AccountID equals acnt.ID
                               select new
                               {
                                   HomeID = acnt.HomeID,
                                   RefDoc = dp.RefenceBuyDocumentID
                               } into assetacounts
                               join doc in context.FinanceDocument
                               on assetacounts.RefDoc equals doc.ID
                               where doc.ID == this.ID && assetacounts.HomeID == this.HomeID && doc.HomeID == this.HomeID
                               select doc).Count();
            if (usedInAsset > 0) return true;

            usedInAsset = (from dp in context.FinanceAccountExtraAS
                           join acnt in context.FinanceAccount
                            on dp.AccountID equals acnt.ID
                           select new
                           {
                               HomeID = acnt.HomeID,
                               RefDoc = dp.RefenceSoldDocumentID
                           } into assetacounts
                           join doc in context.FinanceDocument
                           on assetacounts.RefDoc equals doc.ID
                           where doc.ID == this.ID && assetacounts.HomeID == this.HomeID && doc.HomeID == this.HomeID
                           select doc).Count();
            if (usedInAsset > 0) return true;

            return false;
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
        public Boolean? UseCurr2 { get; set; }

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
            if (ItemID <= 0)
                return false;
            if (AccountID <= 0)
                return false;
            else if (context.FinanceAccount.Where(p => p.ID == AccountID).Count() != 1)
                return false;
            if (TranType <= 0)
                return false;
            else if (context.FinTransactionType.Where(p => p.ID == TranType).Count() != 1)
                return false;
            if (TranAmount == 0)
                return false;

            return true;
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
        public List<FinanceDocumentItem> Items { get; set; }
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
