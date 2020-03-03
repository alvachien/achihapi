using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.OData.Edm;

namespace hihapi.Models
{
    public enum FinancePlanTypeEnum : Byte
    {
        Account         = 0,
        AccountCategory = 1,
        ControlCenter   = 2,
        TranType        = 3,
    }

    [Table("T_FIN_PLAN")]
    public sealed class FinancePlan : BaseModel
    {
        [Key]
        [Column("ID", TypeName = "INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("PTYPE", TypeName = "TINYINT")]
        public FinancePlanTypeEnum PlanType { get; set; }

        [Column("ACCOUNTID", TypeName = "INT")]
        public Int32? AccountID { get; set; }

        [Column("ACNTCTGYID", TypeName = "INT")]
        public Int32? AccountCategoryID { get; set; }

        [Column("CCID", TypeName = "INT")]
        public Int32? ControlCenterID { get; set; }

        [Column("TTID", TypeName = "INT")]
        public Int32? TranTypeID { get; set; }

        [Required]
        [Column("STARTDATE", TypeName = "DATE")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("TGTDATE", TypeName = "DATE")]
        public DateTime TargetDate { get; set; }

        [Required]
        [Column("TGTBAL", TypeName = "DECIMAL(17, 2)")]
        public Decimal TargetBalance { get; set; }

        [Required]
        [StringLength(5)]
        [Column("TRANCURR", TypeName = "NVARCHAR(5)")]
        public String TranCurr { get; set; }

        [Required]
        [StringLength(50)]
        [Column("DESP", TypeName = "NVARCHAR(50)")]
        public String Description { get; set; }

        public FinancePlan(): base()
        {
            this.PlanType = FinancePlanTypeEnum.Account;
        }

        public override bool IsValid(hihDataContext context)
        {
            var isValid = base.IsValid(context);
            if (!isValid)
                return false;

            // Perform own check logic
            // Home ID
            if (HomeID <= 0)
                isValid = false;
            // Currency
            if (isValid)
            {
                if (String.IsNullOrEmpty(TranCurr))
                    isValid = false;
            }
            // Description
            if (isValid)
            {
                if (String.IsNullOrEmpty(Description))
                    isValid = false;
            }
            // Date Range
            if (isValid)
            {
                if (TargetDate < StartDate)
                    isValid = false;
            }
            // Plan type and its value
            if (isValid)
            {
                switch (PlanType)
                {
                    case FinancePlanTypeEnum.Account:
                        if (AccountID.HasValue)
                        {
                            var acntexist = context.FinanceAccount.Where(p => p.ID == AccountID.Value && p.HomeID == HomeID).Count();
                            if (acntexist < 1)
                                isValid = false;
                        }
                        else
                            isValid = false;
                        break;

                    case FinancePlanTypeEnum.AccountCategory:
                        if (AccountCategoryID.HasValue)
                        {
                            var ctgexist = context.FinAccountCategories.Where(p => p.ID == AccountCategoryID.Value).Count();
                            if (ctgexist < 1)
                                isValid = false;
                        }
                        else
                            isValid = false;
                        break;

                    case FinancePlanTypeEnum.ControlCenter:
                        if (ControlCenterID.HasValue)
                        {
                            var ccexist = context.FinanceControlCenter.Where(p => p.ID == ControlCenterID.Value).Count();
                            if (ccexist < 1)
                                isValid = false;
                        }
                        else
                            isValid = false;
                        break;

                    case FinancePlanTypeEnum.TranType:
                        if (TranTypeID.HasValue)
                        {
                            var ttexist = context.FinTransactionType.Where(p => p.ID == TranTypeID.Value).Count();
                            if (ttexist < 1)
                                isValid = false;
                        }
                        else
                            isValid = false;
                        break;

                    default:
                        isValid = false;
                        break;
                }
            }

            return isValid;
        }

        public HomeDefine CurrentHome { get; set; }
    }
}
