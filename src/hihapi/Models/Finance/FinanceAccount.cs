using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;

namespace hihapi.Models
{
    public enum FinanceAccountStatus : Byte
    {
        Normal = 0,
        Closed = 1,
        Frozen = 2
    }

    public enum LoanRepaymentMethod
    {
        EqualPrincipalAndInterset   = 1,    // Equal principal & interest
        EqualPrincipal              = 2,    // Equal principal
        DueRepayment                = 3     // Due repayment
    }

    [Table("T_FIN_ACCOUNT")]
    public sealed class FinanceAccount : BaseModel
    {
        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HomeID { get; set; }

        [Required]
        [Column("CTGYID", TypeName="INT")]
        public Int32 CategoryID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }
        
        [StringLength(40)]
        [Column("OWNER", TypeName="NVARCHAR(40)")]
        public String Owner { get; set; }

        [Column("STATUS", TypeName="TINYINT")]
        public FinanceAccountStatus Status { get; set; }

        public FinanceAccount(): base()
        {
        }

        public override bool IsValid(hihDataContext context)
        {
            if (!base.IsValid(context))
                return false;

            if (HomeID == 0 || CategoryID == 0)
                return false;
            if (String.IsNullOrEmpty(Name))
                return false;

            switch(CategoryID)
            {
                case FinanceAccountCategory.AccountCategory_AdvancePayment:
                    if (ExtraDP == null)
                        return false;
                    else
                        return ExtraDP.IsValid();

                case FinanceAccountCategory.AccountCategory_BorrowFrom:
                    if (ExtraLoan == null)
                        return false;
                    else
                        return ExtraLoan.IsValid();

                case FinanceAccountCategory.AccountCategory_Asset:
                    if (ExtraAsset == null)
                        return false;
                    else
                        return ExtraAsset.IsValid();

                default:
                    break;
            }
            return true;
        }
        public override bool IsDeleteAllowed(hihDataContext context)
        {
            if (!base.IsDeleteAllowed(context))
                return false;

            var refcnt = 0;
            // Documents
            refcnt = context.FinanceDocumentItem.Where(p => p.AccountID == this.ID).Count();
            if (refcnt > 0) return false;
            // Plan
            refcnt = context.FinancePlan.Where(p => p.PlanType == FinancePlanTypeEnum.Account && p.AccountID == this.ID).Count();
            if (refcnt > 0) return false;

            return true;
        }
        public bool IsCloseAllowed(hihDataContext context)
        {
            // Check category
            switch (CategoryID)
            {
                //case FinanceAccountCategory.AccountCategory_Cash:
                //case FinanceAccountCategory.AccountCategory_VirtualAccount:
                //    {
                //        // Check balance
                //        return false;
                //    }
                //    break;

                //case FinanceAccountCategory.AccountCategory_Deposit:
                //    {
                //        // Check balance
                //        return false;
                //    }
                //    break;

                //case FinanceAccountCategory.AccountCategory_Creditcard:
                //    {
                //        // Check balance
                //        return false;
                //    }
                //    break;

                case FinanceAccountCategory.AccountCategory_Asset:
                    {
                        // Can be closed directly
                    }
                    break;

                //case FinanceAccountCategory.AccountCategory_AdvancePayment:
                //    break;

                //case FinanceAccountCategory.AccountCategory_AdvanceReceive:
                //    break;

                //case FinanceAccountCategory.AccountCategory_BorrowFrom:
                //    break;

                default:
                    return false;
            }

            // Status
            if (Status != FinanceAccountStatus.Normal) 
                return false;

            return true;
        }

        public HomeDefine CurrentHome { get; set; }
        public FinanceAccountExtraDP ExtraDP { get; set; }
        // public FinanceAccountExtraLoan ExtraLoan { get; set; }
        public FinanceAccountExtraAS ExtraAsset { get; set; }
        public FinanceAccountExtraLoan ExtraLoan { get; set; }
    }

    public abstract class FinanceAccountExtra
    {
        [Key]
        [Column("ACCOUNTID", TypeName="INT")]
        public Int32 AccountID { get; set; }

        public abstract bool IsValid();
        
        public abstract string GetDBFieldName(string field);
    }

    // Account extra: advance payment
    [Table("T_FIN_ACCOUNT_EXT_DP")]
    public sealed class FinanceAccountExtraDP: FinanceAccountExtra
    {
        [Required]
        [Column("DIRECT", TypeName="BIT")]
        public Boolean Direct { get; set; }
        
        [Required]
        [Column("STARTDATE")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Column("ENDDATE")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Column("RPTTYPE", TypeName="TINYINT")]
        public RepeatFrequency RepeatType { get; set; }
        
        [Required]
        [Column("REFDOCID", TypeName="INT")]
        public Int32 RefenceDocumentID { get; set; }

        [StringLength(100)]
        [Column("DEFRRDAYS", TypeName="NVARCHAR(100)")]
        public String DefrrDays { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }

        // Tmp. docs
        public ICollection<FinanceTmpDPDocument> DPTmpDocs { get; set; }
        public FinanceAccount AccountHeader { get; set; }

        public static Dictionary<String, String> dictFieldNames = new Dictionary<string, string>();
        public FinanceAccountExtraDP(): base()
        {
            this.DPTmpDocs = new List<FinanceTmpDPDocument>();
        }
        static FinanceAccountExtraDP()
        {
            dictFieldNames.Add("AccountID", "ACCOUNTID");
            dictFieldNames.Add("Direct", "DIRECT");
            dictFieldNames.Add("StartDate", "STARTDATE");
            dictFieldNames.Add("EndDate", "ENDDATE");
            dictFieldNames.Add("RptType", "RPTTYPE");
            dictFieldNames.Add("RefDocID", "REFDOCID");
            dictFieldNames.Add("DefrrDays", "DEFRRDAYS");
            dictFieldNames.Add("Comment", "COMMENT");
        }
        public override String GetDBFieldName(String strField)
        {
            if (!dictFieldNames.ContainsKey(strField))
                return strField;

            return dictFieldNames[strField];
        }
        public override Boolean IsValid()
        {
            if (EndDate.Date.CompareTo(StartDate.Date) <= 0)
                return false;
            if (String.IsNullOrEmpty(Comment))
                return false;
            //if (DPTmpDocs.Count <= 0)
            //    return false;

            return true;
        }
        public static Dictionary<String, Object> WorkoutDeltaForUpdate(FinanceAccountExtraDP oldAcnt,
            FinanceAccountExtraDP newAcnt)
        {
            Dictionary<String, Object> dictDelta = new Dictionary<string, object>();
            if (oldAcnt == null || newAcnt == null || Object.ReferenceEquals(oldAcnt, newAcnt)
                || oldAcnt.AccountID != newAcnt.AccountID)
            {
                throw new ArgumentException("Invalid inputted parameter Or AccountID change is not allowed");
            }
            if (!oldAcnt.IsValid() || !newAcnt.IsValid())
            {
                throw new Exception("Account info is invalid");
            }

            Type t = typeof(FinanceAccountExtraDP);
            PropertyInfo[] listProperties = t.GetProperties();
            var listSortedProperties = listProperties.OrderBy(o => o.Name);

            foreach (PropertyInfo item in listSortedProperties)
            {
                if (item.Name == "Direct" || item.Name == "DefrrDays" || item.Name == "DPTmpDocs")
                {
                    continue;
                }

                object oldValue = item.GetValue(oldAcnt, null);
                object newValue = item.GetValue(newAcnt, null);
                if (item.PropertyType == typeof(Decimal))
                {
                    if (Decimal.Compare((Decimal)oldValue, (Decimal)newValue) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if (item.PropertyType == typeof(String))
                {
                    if (String.CompareOrdinal((string)oldValue, (string)newValue) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if (item.PropertyType == typeof(DateTime))
                {
                    if (DateTime.Compare(((DateTime)oldValue).Date, ((DateTime)newValue).Date) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if(item.PropertyType == typeof(RepeatFrequency))
                {
                    if ((RepeatFrequency)oldValue != (RepeatFrequency)newValue) dictDelta.Add(item.Name, (Byte)newValue);
                }
                else
                {
                    if (!Object.Equals(oldValue, newValue))
                        dictDelta.Add(item.Name, newValue);
                }
            }

            return dictDelta;
        }
        public static string WorkoutDeltaStringForUpdate(FinanceAccountExtraDP oldAcnt,
            FinanceAccountExtraDP newAcnt)
        {
            var diffs = WorkoutDeltaForUpdate(oldAcnt, newAcnt);

            List<String> listHeaderSqls = new List<string>();
            foreach (var diff in diffs)
            {
                var dbfield = newAcnt.GetDBFieldName(diff.Key);

                if (diff.Value == null)
                {
                    listHeaderSqls.Add("[" + dbfield + "] = NULL");
                }
                else
                {
                    if (diff.Value is DateTime)
                        listHeaderSqls.Add("[" + dbfield + "] = '" + ((DateTime)diff.Value).ToString("yyyy-MM-dd") + "'");
                    else if (diff.Value is Boolean)
                        listHeaderSqls.Add("[" + dbfield + "] = " + (((Boolean)diff.Value) ? "1" : "NULL"));
                    else if (diff.Value is String)
                    {
                        if (String.IsNullOrEmpty((string)diff.Value))
                            listHeaderSqls.Add("[" + dbfield + "] = NULL");
                        else
                            listHeaderSqls.Add("[" + dbfield + "] = N'" + diff.Value + "'");
                    }
                    else if (diff.Value is Decimal)
                    {
                        if (Decimal.Compare((Decimal)diff.Value, 0) == 0)
                        {
                            listHeaderSqls.Add("[" + dbfield + "] = NULL");
                        }
                        else
                            listHeaderSqls.Add("[" + dbfield + "] = " + diff.Value.ToString());
                    }
                    else
                        listHeaderSqls.Add("[" + dbfield + "] = " + diff.Value.ToString());
                }
            }

            return listHeaderSqls.Count == 0 ?
                String.Empty :
                (@"UPDATE [t_fin_account_ext_dp] SET " + string.Join(",", listHeaderSqls) + " WHERE [ACCOUNTID] = " + oldAcnt.AccountID.ToString());
        }
    }

    // Account extra: Assert
    [Table("T_FIN_ACCOUNT_EXT_AS")]
    public sealed class FinanceAccountExtraAS: FinanceAccountExtra
    {
        [Required]
        [Column("CTGYID", TypeName="INT")]
        public Int32 CategoryID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("NAME", TypeName="NVARCHAR(50)")]
        public String Name { get; set; }

        [StringLength(50)]
        [Column("COMMENT", TypeName="NVARCHAR(100)")]
        public String Comment { get; set; }

        [Required]
        [Column("REFDOC_BUY", TypeName="INT")]
        public Int32 RefenceBuyDocumentID { get; set; }

        [Column("REFDOC_SOLD", TypeName="INT")]
        public Int32? RefenceSoldDocumentID { get; set; }

        [NotMapped]
        public FinanceAssetCategory AssetCategory { get; set; }

        public FinanceAccount AccountHeader { get; set; }
        
        public FinanceAccountExtraAS(): base()
        {
        }
        public static Dictionary<String, String> dictFieldNames = new Dictionary<string, string>();
        static FinanceAccountExtraAS()
        {
            dictFieldNames.Add("AccountID", "ACCOUNTID");
            dictFieldNames.Add("CategoryID", "CTGYID");
            dictFieldNames.Add("Name", "NAME");
            dictFieldNames.Add("Comment", "COMMENT");
            dictFieldNames.Add("RefDocForBuy", "REFDOC_BUY");
            dictFieldNames.Add("RefDocForSold", "REFDOC_SOLD");
        }
        public override String GetDBFieldName(String strField)
        {
            if (!dictFieldNames.ContainsKey(strField))
                return strField;

            return dictFieldNames[strField];
        }

        public override bool IsValid()
        {
            if (String.IsNullOrEmpty(Name))
                return false;
            if (RefenceBuyDocumentID <= 0)
                return false;
            if (CategoryID <= 0)
                return false;

            return true;
        }

        public static Dictionary<String, Object> WorkoutDeltaForUpdate(FinanceAccountExtraAS oldAcnt,
            FinanceAccountExtraAS newAcnt)
        {
            Dictionary<String, Object> dictDelta = new Dictionary<string, object>();
            if (oldAcnt == null || newAcnt == null || Object.ReferenceEquals(oldAcnt, newAcnt)
                || oldAcnt.AccountID != newAcnt.AccountID)
            {
                throw new ArgumentException("Invalid inputted parameter Or AccountID change is not allowed");
            }
            if (!oldAcnt.IsValid() || !newAcnt.IsValid())
            {
                throw new Exception("Account info is invalid");
            }

            Type t = typeof(FinanceAccountExtraAS);
            PropertyInfo[] listProperties = t.GetProperties();
            var listSortedProperties = listProperties.OrderBy(o => o.Name);

            foreach (PropertyInfo item in listSortedProperties)
            {
                if (item.Name == "RefDocForSold")
                {
                    if (oldAcnt.RefenceSoldDocumentID.HasValue)
                    {
                        if (newAcnt.RefenceSoldDocumentID.HasValue)
                        {
                            if (oldAcnt.RefenceSoldDocumentID.Value != newAcnt.RefenceSoldDocumentID.Value)
                            {
                                dictDelta.Add(item.Name, newAcnt.RefenceSoldDocumentID.Value);
                            }
                        }
                        else
                        {
                            dictDelta.Add(item.Name, null);
                        }
                    }
                    else
                    {
                        if (newAcnt.RefenceSoldDocumentID.HasValue)
                        {
                            dictDelta.Add(item.Name, newAcnt.RefenceSoldDocumentID.Value);
                        }
                    }
                }
                else
                {
                    object oldValue = item.GetValue(oldAcnt, null);
                    object newValue = item.GetValue(newAcnt, null);
                    if (item.PropertyType == typeof(Decimal))
                    {
                        if (Decimal.Compare((Decimal)oldValue, (Decimal)newValue) != 0) dictDelta.Add(item.Name, newValue);
                    }
                    else if (item.PropertyType == typeof(String))
                    {
                        if (String.CompareOrdinal((string)oldValue, (string)newValue) != 0) dictDelta.Add(item.Name, newValue);
                    }
                    else if (item.PropertyType == typeof(DateTime))
                    {
                        if (DateTime.Compare(((DateTime)oldValue).Date, ((DateTime)newValue).Date) != 0) dictDelta.Add(item.Name, newValue);
                    }
                    else
                    {
                        if (!Object.Equals(oldValue, newValue))
                            dictDelta.Add(item.Name, newValue);
                    }
                }
            }

            return dictDelta;
        }
        public static string WorkoutDeltaStringForUpdate(FinanceAccountExtraAS oldAcnt,
            FinanceAccountExtraAS newAcnt)
        {
            var diffs = WorkoutDeltaForUpdate(oldAcnt, newAcnt);

            List<String> listHeaderSqls = new List<string>();
            foreach (var diff in diffs)
            {
                var dbfield = newAcnt.GetDBFieldName(diff.Key);

                if (diff.Value == null)
                {
                    listHeaderSqls.Add("[" + dbfield + "] = NULL");
                }
                else
                {
                    if (diff.Value is DateTime)
                        listHeaderSqls.Add("[" + dbfield + "] = '" + ((DateTime)diff.Value).ToString("yyyy-MM-dd") + "'");
                    else if (diff.Value is Boolean)
                        listHeaderSqls.Add("[" + dbfield + "] = " + (((Boolean)diff.Value) ? "1" : "NULL"));
                    else if (diff.Value is String)
                    {
                        if (String.IsNullOrEmpty((string)diff.Value))
                            listHeaderSqls.Add("[" + dbfield + "] = NULL");
                        else
                            listHeaderSqls.Add("[" + dbfield + "] = N'" + diff.Value + "'");
                    }
                    else if (diff.Value is Decimal)
                    {
                        if (Decimal.Compare((Decimal)diff.Value, 0) == 0)
                        {
                            listHeaderSqls.Add("[" + dbfield + "] = NULL");
                        }
                        else
                            listHeaderSqls.Add("[" + dbfield + "] = " + diff.Value.ToString());
                    }
                    else
                        listHeaderSqls.Add("[" + dbfield + "] = " + diff.Value.ToString());
                }
            }

            return listHeaderSqls.Count == 0 ?
                String.Empty :
                (@"UPDATE [t_fin_account_ext_as] SET " + string.Join(",", listHeaderSqls) + " WHERE [ACCOUNTID] = " + oldAcnt.AccountID.ToString());
        }
    }

    // Account extra: Loan (Borrow from, or Lend to)
    [Table("T_FIN_ACCOUNT_EXT_LOAN")]
    public sealed class FinanceAccountExtraLoan : FinanceAccountExtra
    {
        [Required]
        [Column("STARTDATE")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Column("ANNUALRATE", TypeName = "DECIMAL(17, 2)")]
        public Decimal? AnnualRate { get; set; }

        [Column("INTERESTFREE", TypeName = "BIT")]
        public Boolean? InterestFree { get; set; }

        [Column("REPAYMETHOD", TypeName = "TINYINT")]
        public LoanRepaymentMethod? RepaymentMethod { get; set; }

        [Column("TOTALMONTH", TypeName = "SMALLINT")]
        public Int16? TotalMonths { get; set; }

        [Column("REFDOCID", TypeName = "INT")]
        public Int32 RefDocID { get; set; }

        [Column("OTHERS", TypeName = "NVARCHAR(100)")]
        [StringLength(100)]
        public String Others { get; set; }

        [Column("ENDDATE")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Column("PAYINGACCOUNT", TypeName = "INT")]
        public Int32? PayingAccount { get; set; }

        [Column("PARTNER", TypeName = "NVARCHAR(50)")]
        [StringLength(50)]
        public String Partner { get; set; }

        // Tmp. docs
        public ICollection<FinanceTmpLoanDocument> LoanTmpDocs { get; set; }
        public FinanceAccount AccountHeader { get; set; }

        public FinanceAccountExtraLoan() : base()
        {
            // Default
            this.StartDate = DateTime.Today;
            this.LoanTmpDocs = new List<FinanceTmpLoanDocument>();
        }

        public static Dictionary<String, String> dictFieldNames = new Dictionary<string, string>();
        static FinanceAccountExtraLoan()
        {
            dictFieldNames.Add("AccountID", "ACCOUNTID");
            dictFieldNames.Add("StartDate", "STARTDATE");
            dictFieldNames.Add("AnnualRate", "ANNUALRATE");
            dictFieldNames.Add("InterestFree", "INTERESTFREE");
            dictFieldNames.Add("RepaymentMethod", "REPAYMETHOD");
            dictFieldNames.Add("TotalMonths", "TOTALMONTH");
            dictFieldNames.Add("RefDocID", "REFDOCID");
            dictFieldNames.Add("Others", "OTHERS");
            dictFieldNames.Add("EndDate", "ENDDATE");
            dictFieldNames.Add("PayingAccount", "PAYINGACCOUNT");
            dictFieldNames.Add("Partner", "PARTNER");
        }
        public override String GetDBFieldName(String strField)
        {
            if (!dictFieldNames.ContainsKey(strField))
                return String.Empty;

            return dictFieldNames[strField];
        }

        public override bool IsValid()
        {
            if (RefDocID <= 0)
                return false;
            //if (LoanTmpDocs.Count < 0)
            //    return false;
            if (InterestFree.HasValue && InterestFree.Value == true && AnnualRate.HasValue && AnnualRate.Value >= 0)
                return false; // throw new Exception("Cannot input interest rate for Interest-Free loan");
            if (AnnualRate.HasValue && AnnualRate.Value < 0)
                return false; // throw new Exception("Interest rate can not be negative");
            if (RepaymentMethod.HasValue)
            {
                if (RepaymentMethod.Value == LoanRepaymentMethod.EqualPrincipal
                    || RepaymentMethod.Value == LoanRepaymentMethod.EqualPrincipalAndInterset)
                {
                    if (!TotalMonths.HasValue || (TotalMonths.HasValue && TotalMonths.Value <= 0))
                        return false; //  throw new Exception("Total months must large than zero");
                }
                else if (RepaymentMethod.Value == LoanRepaymentMethod.DueRepayment)
                {
                    if (!EndDate.HasValue)
                        return false; //  throw new Exception("End date must input");
                }
                else
                    return false; //  throw new Exception("Not supported method");
            }

            return true;
        }

        public static Dictionary<String, Object> WorkoutDeltaForUpdate(FinanceAccountExtraLoan oldAcnt,
            FinanceAccountExtraLoan newAcnt)
        {
            Dictionary<String, Object> dictDelta = new Dictionary<string, object>();
            if (oldAcnt == null || newAcnt == null || Object.ReferenceEquals(oldAcnt, newAcnt)
                || oldAcnt.AccountID != newAcnt.AccountID)
            {
                throw new ArgumentException("Invalid inputted parameter Or AccountID change is not allowed");
            }
            if (!oldAcnt.IsValid() || !newAcnt.IsValid())
            {
                throw new Exception("Account info is invalid");
            }

            Type t = typeof(FinanceAccountExtraLoan);
            PropertyInfo[] listProperties = t.GetProperties();
            var listSortedProperties = listProperties.OrderBy(o => o.Name);

            foreach (PropertyInfo item in listSortedProperties)
            {
                if (item.Name == "LoanTmpDocs")
                {
                    continue;
                }

                object oldValue = item.GetValue(oldAcnt, null);
                object newValue = item.GetValue(newAcnt, null);
                if (item.PropertyType == typeof(Nullable<Decimal>))
                {
                    if (Nullable.Compare<Decimal>(oldValue as Nullable<Decimal>, newValue as Nullable<Decimal>) != 0)
                    {
                        if (newValue != null)
                            dictDelta.Add(item.Name, (newValue as Nullable<Decimal>).Value);
                        else
                            dictDelta.Add(item.Name, null);
                    }
                }
                else if (item.PropertyType == typeof(Nullable<DateTime>))
                {
                    if (Nullable.Compare<DateTime>(oldValue as Nullable<DateTime>, newValue as Nullable<DateTime>) != 0)
                    {
                        if (newValue != null)
                            dictDelta.Add(item.Name, (newValue as Nullable<DateTime>).Value);
                        else
                            dictDelta.Add(item.Name, null);
                    }
                }
                else if (item.PropertyType == typeof(Nullable<Boolean>))
                {
                    if (Nullable.Compare<Boolean>(oldValue as Nullable<Boolean>, newValue as Nullable<Boolean>) != 0)
                    {
                        if (newValue != null)
                            dictDelta.Add(item.Name, (newValue as Nullable<Boolean>).Value ? 1 : 0);
                        else
                            dictDelta.Add(item.Name, null);
                    }
                }
                else if (item.PropertyType == typeof(Nullable<Int32>))
                {
                    if (Nullable.Compare<Int32>(oldValue as Nullable<Int32>, newValue as Nullable<Int32>) != 0)
                    {
                        if (newValue != null)
                            dictDelta.Add(item.Name, (newValue as Nullable<Int32>).Value);
                        else
                            dictDelta.Add(item.Name, null);
                    }
                }
                else if (item.PropertyType == typeof(Nullable<Int16>))
                {
                    if (Nullable.Compare<Int16>(oldValue as Nullable<Int16>, newValue as Nullable<Int16>) != 0)
                    {
                        if (newValue != null)
                            dictDelta.Add(item.Name, (newValue as Nullable<Int16>).Value);
                        else
                            dictDelta.Add(item.Name, null);
                    }
                }
                else if (item.PropertyType == typeof(Decimal))
                {
                    if (Decimal.Compare((Decimal)oldValue, (Decimal)newValue) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if (item.PropertyType == typeof(String))
                {
                    if (String.CompareOrdinal((string)oldValue, (string)newValue) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if (item.PropertyType == typeof(DateTime))
                {
                    if (DateTime.Compare(((DateTime)oldValue).Date, ((DateTime)newValue).Date) != 0) dictDelta.Add(item.Name, newValue);
                }
                else
                {
                    if (!Object.Equals(oldValue, newValue))
                        dictDelta.Add(item.Name, newValue);
                }
            }

            return dictDelta;
        }
        public static string WorkoutDeltaStringForUpdate(FinanceAccountExtraLoan oldAcnt,
            FinanceAccountExtraLoan newAcnt)
        {
            var diffs = WorkoutDeltaForUpdate(oldAcnt, newAcnt);

            List<String> listHeaderSqls = new List<string>();
            foreach (var diff in diffs)
            {
                var dbfield = newAcnt.GetDBFieldName(diff.Key);

                if (diff.Value is DateTime)
                    listHeaderSqls.Add("[" + dbfield + "] = '" + ((DateTime)diff.Value).ToString("yyyy-MM-dd") + "'");
                else if (diff.Value is Boolean)
                    listHeaderSqls.Add("[" + dbfield + "] = " + (((Boolean)diff.Value) ? "1" : "NULL"));
                else if (diff.Value is String)
                {
                    if (String.IsNullOrEmpty((string)diff.Value))
                        listHeaderSqls.Add("[" + dbfield + "] = NULL");
                    else
                        listHeaderSqls.Add("[" + dbfield + "] = N'" + diff.Value + "'");
                }
                else if (diff.Value is Decimal)
                {
                    if (Decimal.Compare((Decimal)diff.Value, 0) == 0)
                    {
                        listHeaderSqls.Add("[" + dbfield + "] = NULL");
                    }
                    else
                        listHeaderSqls.Add("[" + dbfield + "] = " + diff.Value.ToString());
                }
                else if (diff.Value is Boolean)
                {
                    if ((bool)diff.Value)
                        listHeaderSqls.Add("[" + dbfield + "] = 1");
                    else
                        listHeaderSqls.Add("[" + dbfield + "] = 0");
                }
                else
                {
                    if (diff.Value == null)
                        listHeaderSqls.Add("[" + dbfield + "] = NULL");
                    else
                        listHeaderSqls.Add("[" + dbfield + "] = " + diff.Value.ToString());
                }
            }

            return listHeaderSqls.Count == 0 ?
                String.Empty :
                (@"UPDATE [t_fin_account_ext_loan] SET " + string.Join(",", listHeaderSqls)
                    + " WHERE [ACCOUNTID] = " + oldAcnt.AccountID.ToString());
        }
    }

    // Account extra: Creditcard
}
