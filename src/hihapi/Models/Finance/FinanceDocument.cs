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

            // Check must on db.
            if (context == null) return false;

            // 1. Used in DP Created doc:
            //      1.1 If any of DP template document got posted,
            //          Not allowed;
            //      1.2 If DP document has no posted document(means new created)
            //          Allowed;
            var dpacnt = getDPAccountIfCurrentDocUsedInCreating(context);
            if (dpacnt != null)
            {
                var tmpdoccnt = (from dpdoc in context.FinanceTmpDPDocument
                                 where dpdoc.ReferenceDocumentID != null
                                    && dpdoc.AccountID == dpacnt.AccountID
                                 select dpdoc).Count();
                return tmpdoccnt == 0;
            }

            // 2. Used in DP template document
            //      Allowed
            var tmpdpdoc = getDPTmpDocForCurrentDocIfExist(context);
            if (tmpdpdoc != null) return true;

            // 3. Used in Loan created doc:
            //    3.1 If Loan document got posted already
            //        Not allowed
            //    3.2 If Loan document has no posted document
            //        Allowed
            var loanacnt = getLoanAccountIfCurrentDocUsedInCreating(context);
            if (loanacnt != null)
            {
                var tmpdoccnt = (from tmpdoc in context.FinanceTmpLoanDocument
                                 where tmpdoc.ReferenceDocumentID != null
                                    && tmpdoc.AccountID == loanacnt.AccountID
                                 select tmpdoc).Count();
                return tmpdoccnt == 0;
            }

            // 4. Used in loan template document
            //      Allowed
            var loantmpdoc = getLoanTmpDocForCurrentDocIfExist(context);
            if (loantmpdoc != null) return true;

            // 5. Used in Asset Buy Doc:
            //    5.1 If asset value change doc or asset sold douc exist
            //        Not allowed;
            //    5.2 If no asset value change doc and no asset sold doc exist
            //        Allowed
            // 6. Used in Asset value change doc
            //    6.1 If asset value change doc(after that date) or asset sold doc exists
            //        Not allowed
            //    6.2 If no asset value change doc(after that date) or asset sold doc exists
            //        Allowed
            if (this.DocType == FinanceDocumentType.DocType_AssetBuyIn || this.DocType == FinanceDocumentType.DocType_AssetValChg)
            {
                var assetacnt = getAccountIfCurrentDocUsedInAssetTransaction(context);
                if (assetacnt != null)
                {
                    var doccnt = (from docitem in context.FinanceDocumentItem
                                  join docheader in context.FinanceDocument
                                  on docitem.DocID equals docheader.ID
                                  where docitem.AccountID == assetacnt.ID
                                    && docheader.HomeID == this.HomeID
                                    && (docheader.DocType == FinanceDocumentType.DocType_AssetSoldOut || docheader.DocType == FinanceDocumentType.DocType_AssetValChg)
                                    && docheader.TranDate > this.TranDate
                                  select docheader).Count();
                    return doccnt == 0;
                }
            }

            // 7. Used in Asset Sold Doc:
            //    Allowed
            if (this.DocType == FinanceDocumentType.DocType_AssetSoldOut)
            {
                //var assetacnt = (from item in context.FinanceDocumentItem
                //                 join acntheader in context.FinanceAccount
                //                 on item.AccountID equals acntheader.ID
                //                 where item.DocID == this.ID && acntheader.CategoryID == FinanceAccountCategory.AccountCategory_Asset
                //                 select acntheader).SingleOrDefault();
                //if (assetacnt != null)
                //{
                //    var doccnt = (from docitem in context.FinanceDocumentItem
                //                  join docheader in context.FinanceDocument
                //                  on docitem.DocID equals docheader.ID
                //                  where docitem.AccountID == assetacnt.ID
                //                    && docheader.HomeID == this.HomeID
                //                    && (docheader.DocType == FinanceDocumentType.DocType_AssetSoldOut || docheader.DocType == FinanceDocumentType.DocType_AssetValChg)
                //                    && docheader.TranDate > this.TranDate
                //                  select docheader).Count();
                //    return doccnt == 0;
                //}
                return true;
            }

            return true;
        }

        public bool IsChangeAllowed(hihDataContext context)
        {
            // Doc type: Only normal doc allows the change
            if (this.DocType != FinanceDocumentType.DocType_Normal) return false;

            // 1. Used in DP created doc:
            //    Not allowed(exclude the description)
            // Doc type check shall prevent it happens...
            //var dpacnt = getDPAccountIfCurrentDocUsedInCreating(context);
            //if (dpacnt != null) return false;

            // 2. Used in DP template document
            //    Not allowed(exclude the description)
            var tmpdpdoc = getDPTmpDocForCurrentDocIfExist(context);
            if (tmpdpdoc != null) return false;

            // 3. Used in Loan created doc:
            //    Not allowed(exclude the description)
            // Doc type check shall prevent it happens...
            //var loanacnt = getLoanAccountIfCurrentDocUsedInCreating(context);
            //if (loanacnt != null) return false;

            // 4. Used in Loan template document
            //    Not allowed(exclude the description)
            var loantmpdoc = getLoanTmpDocForCurrentDocIfExist(context);
            if(loantmpdoc != null) return false;

            // 5. Used in Asset buy Doc
            //    Not allowed(exclude description)
            // 6. Used in asset value change doc
            //    Not allowed(exclude description)
            // 7. Used in Asset Sold doc
            //    Not allowed(exclude description)
            // Doc type check shall prevent it happens...
            //var assetacnt = getAccountIfCurrentDocUsedInAssetTransaction(context);
            //if (assetacnt != null) return false;

            return true;
        }

        private FinanceAccountExtraDP getDPAccountIfCurrentDocUsedInCreating(hihDataContext context)
        {
            return (from acnt in context.FinanceAccountExtraDP
                    join acntheader in context.FinanceAccount
                        on acnt.AccountID equals acntheader.ID
                    where acntheader.HomeID == this.HomeID && acnt.RefenceDocumentID == this.ID
                    select acnt).SingleOrDefault();
        }

        private FinanceTmpDPDocument getDPTmpDocForCurrentDocIfExist(hihDataContext context)
        {
            return (from tmpdoc in context.FinanceTmpDPDocument
                    where tmpdoc.ReferenceDocumentID == this.ID && tmpdoc.HomeID == this.HomeID
                    select tmpdoc).SingleOrDefault();
        }

        private FinanceAccountExtraLoan getLoanAccountIfCurrentDocUsedInCreating(hihDataContext context)
        {
            return (from acnt in context.FinanceAccountExtraLoan
                    join acntheader in context.FinanceAccount
                      on acnt.AccountID equals acntheader.ID
                    where acntheader.HomeID == this.HomeID && acnt.RefDocID == this.ID
                    select acnt).SingleOrDefault();
        }

        private FinanceTmpLoanDocument getLoanTmpDocForCurrentDocIfExist(hihDataContext context)
        {
            return (from tmpdoc in context.FinanceTmpLoanDocument
                    where tmpdoc.ReferenceDocumentID == this.ID && tmpdoc.HomeID == this.HomeID
                    select tmpdoc).SingleOrDefault();
        }

        private FinanceAccount getAccountIfCurrentDocUsedInAssetTransaction(hihDataContext context)
        {
            return (from item in context.FinanceDocumentItem
                    join acntheader in context.FinanceAccount
                    on item.AccountID equals acntheader.ID
                    where item.DocID == this.ID && acntheader.CategoryID == FinanceAccountCategory.AccountCategory_Asset
                    select acntheader).SingleOrDefault();
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
        public Boolean? IsLegacy { get; set; }
    }

    public sealed class FinanceLoanRepayDocumentCreateContext
    {
        public int? LoanTemplateDocumentID { get; set; }
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
