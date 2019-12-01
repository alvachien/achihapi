using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    public enum FinanceAccountStatus : Byte
    {
        Normal = 0,
        Closed = 1,
        Frozen = 2
    }
    public enum RepeatFrequency : Byte
    {
        Month       = 0,
        Fortnight   = 1,
        Week        = 2,
        Day         = 3,
        Quarter     = 4,
        HalfYear    = 5,
        Year        = 6,
        Manual      = 7,
    }
    public enum LoanRepaymentMethod
    {
        EqualPrincipalAndInterset   = 1,  // Equal principal & interest
        EqualPrincipal              = 2,  // Equal principal
        DueRepayment                = 3  // Due repayment
    }

    [Table("T_FIN_ACCOUNT")]
    public partial class FinanceAccount : BaseModel
    {
        [Key]
        [Column("ID", TypeName="int")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="int")]
        public Int32 HID { get; set; }

        [Required]
        [Column("CTGYID", TypeName="int")]
        public Int32 CategoryID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName="NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(45)]
        [Column("COMMENT", TypeName="NVARCHAR(45)")]
        public String Comment { get; set; }
        
        [StringLength(40)]
        public String Owner { get; set; }
        public FinanceAccountStatus Status { get; set; }
    }

    public abstract class FinanceAccountExtra
    {
        public Int32 AccountID { get; set; }
        public abstract bool IsValid();
        public abstract string GetDBFieldName(string field);
    }

    // Account extra: advance payment
    [Table("T_FIN_ACCOUNT_EXT_DP")]
    public sealed class FinanceAccountExtDPViewModel: FinanceAccountExtra
    {
        public Boolean Direct { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public RepeatFrequency RptType { get; set; }
        public Int32 RefDocID { get; set; }
        [StringLength(100)]
        public String DefrrDays { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
        // Tmp. docs
        public List<FinanceTmpDocDPViewModel> DPTmpDocs { get; }

        public static Dictionary<String, String> dictFieldNames = new Dictionary<string, string>();
        public FinanceAccountExtDPViewModel(): base()
        {
            this.DPTmpDocs = new List<FinanceTmpDocDPViewModel>();
        }
        static FinanceAccountExtDPViewModel()
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
        public static Dictionary<String, Object> WorkoutDeltaForUpdate(FinanceAccountExtDPViewModel oldAcnt,
            FinanceAccountExtDPViewModel newAcnt)
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

            Type t = typeof(FinanceAccountExtDPViewModel);
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
        public static string WorkoutDeltaStringForUpdate(FinanceAccountExtDPViewModel oldAcnt,
            FinanceAccountExtDPViewModel newAcnt)
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
                (@"UPDATE [dbo].[t_fin_account_ext_dp] SET " + string.Join(",", listHeaderSqls) + " WHERE [ACCOUNTID] = " + oldAcnt.AccountID.ToString());
        }
    }

    // Account extra: Assert
    [Table("T_FIN_ACCOUNT_EXT_AS")]
    public sealed class FinanceAccountExtASViewModel: FinanceAccountExtra
    {
        public Int32 CategoryID { get; set; }
        [StringLength(50)]
        [Required]
        public String Name { get; set; }
        [StringLength(50)]
        public String Comment { get; set; }
        [Required]
        public Int32 RefDocForBuy { get; set; }
        public Int32? RefDocForSold { get; set; }

        public FinanceAccountExtASViewModel(): base()
        {
        }
        public static Dictionary<String, String> dictFieldNames = new Dictionary<string, string>();
        static FinanceAccountExtASViewModel()
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
            if (RefDocForBuy <= 0)
                return false;
            if (CategoryID <= 0)
                return false;

            return true;
        }

        public static Dictionary<String, Object> WorkoutDeltaForUpdate(FinanceAccountExtASViewModel oldAcnt,
            FinanceAccountExtASViewModel newAcnt)
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

            Type t = typeof(FinanceAccountExtASViewModel);
            PropertyInfo[] listProperties = t.GetProperties();
            var listSortedProperties = listProperties.OrderBy(o => o.Name);

            foreach (PropertyInfo item in listSortedProperties)
            {
                if (item.Name == "RefDocForSold")
                {
                    if (oldAcnt.RefDocForSold.HasValue)
                    {
                        if (newAcnt.RefDocForSold.HasValue)
                        {
                            if (oldAcnt.RefDocForSold.Value != newAcnt.RefDocForSold.Value)
                            {
                                dictDelta.Add(item.Name, newAcnt.RefDocForSold.Value);
                            }
                        }
                        else
                        {
                            dictDelta.Add(item.Name, null);
                        }
                    }
                    else
                    {
                        if (newAcnt.RefDocForSold.HasValue)
                        {
                            dictDelta.Add(item.Name, newAcnt.RefDocForSold.Value);
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
        public static string WorkoutDeltaStringForUpdate(FinanceAccountExtASViewModel oldAcnt,
            FinanceAccountExtASViewModel newAcnt)
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
                (@"UPDATE [dbo].[t_fin_account_ext_as] SET " + string.Join(",", listHeaderSqls) + " WHERE [ACCOUNTID] = " + oldAcnt.AccountID.ToString());
        }
    }

}
