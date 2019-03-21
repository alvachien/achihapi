using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;

namespace achihapi.ViewModels
{
    public class FinanceDocumentItemViewModel
    {
        public Int32 DocID { get; set; }
        [Required]
        public Int32 ItemID { get; set; }
        [Required]
        public Int32 AccountID { get; set; }
        [Required]
        public Int32 TranType { get; set; }
        [Required]
        public Decimal TranAmount { get; set; }
        public Boolean UseCurr2 { get; set; }
        // The following fields had better be replaced with Nullable<Int32>
        //  It has been kept as following due to the efforts reason;
        public Int32 ControlCenterID { get; set; }
        public Int32 OrderID { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }

        // Tag
        public List<String> TagTerms { get; }

        public FinanceDocumentItemViewModel()
        {
            this.TagTerms = new List<String>();
        }

        public Boolean IsValid()
        {
            if (ItemID <= 0) return false;
            if (AccountID <= 0) return false;
            if (TranType <= 0) return false;
            if (TranAmount == 0) return false;
            if (ControlCenterID <= 0)
            {
                if (OrderID <= 0)
                    return false;
            }
            else
            {
                if (OrderID > 0)
                    return false;
            }
            if (String.IsNullOrEmpty(Desp)) return false;

            return true;
        }

        public string GetDocItemInsertString()
        {
            Dictionary<String, String> dictvals = new Dictionary<string, String>();

            Type t = typeof(FinanceDocumentItemViewModel);
            PropertyInfo[] tProperties = t.GetProperties();
            foreach (PropertyInfo item in tProperties)
            {
                if (item.Name == "DocID" || item.Name == "ItemID" || item.Name == "AccountID"
                    || item.Name == "TranType" || item.Name == "TranAmount")
                {
                    dictvals.Add(item.Name, item.GetValue(this).ToString());
                }
                else if (item.Name == "UseCurr2")
                {
                    if (UseCurr2) dictvals.Add(item.Name, "1");
                }
                else if (item.Name == "ControlCenterID" || item.Name == "OrderID")
                {
                    Int32 nid = (Int32)item.GetValue(this);
                    if (nid > 0)
                        dictvals.Add(item.Name, nid.ToString());
                }
                else if (item.Name == "Desp")
                {
                    dictvals.Add(item.Name, "N'" + Desp + "'");
                }
            }

            return @"INSERT INTO [dbo].[t_fin_document_item] (" + string.Join(',', dictvals.Keys) + ") VALUES(" + string.Join(',', dictvals.Values) + ")"; 
        }
        public List<String> GetDocItemTagInsertString(Int32 hid)
        {
            List<String> listSqls = new List<string>();
            foreach (var term in TagTerms)
            {
                listSqls.Add(@"INSERT INTO [dbo].[t_tag] ([HID],[TagType],[TagID],[TagSubID],[Term]) VALUES (" 
                    + string.Join(",", new string[] {
                        hid.ToString(),
                        ((Int32)(HIHTagTypeEnum.FinanceDocumentItem)).ToString(),
                        DocID.ToString(),
                        ItemID.ToString(),
                        "N'" + term + "'"})
                    + ")");
            }

            return listSqls;
        }
    }

    public class FinanceDocumentItemUIViewModel : FinanceDocumentItemViewModel
    {
        public String AccountName { get; set; }
        public String TranTypeName { get; set; }
        public String ControlCenterName { get; set; }
        public String OrderName { get; set; }

        public DateTime TranDate { get; set; }
        public String DocDesp { get; set; }

        public static Dictionary<String, Object> WorkoutDeltaUpdate(FinanceDocumentItemUIViewModel oldItem, FinanceDocumentItemUIViewModel newItem)
        {
            Dictionary<String, Object> dictDelta = new Dictionary<string, Object>();
            if (oldItem == null || newItem == null 
                || Object.ReferenceEquals(oldItem, newItem)
                || oldItem.DocID != newItem.DocID
                || oldItem.ItemID != newItem.ItemID)
            {
                throw new ArgumentException("Invalid inputted parameters Or DocID/ItemID is different!");
            }

            Type t = typeof(FinanceDocumentItemUIViewModel);
            Type parent = typeof(FinanceDocumentItemViewModel);
            PropertyInfo[] parentProperties = parent.GetProperties();
            Dictionary<String, Object> dictParentProperties = new Dictionary<string, object>();
            foreach (var prop in parentProperties)
                dictParentProperties.Add(prop.Name, null);

            PropertyInfo[] listProperties = t.GetProperties();
            var listSortedProperties = listProperties.OrderBy(o => o.Name);

            foreach (PropertyInfo item in listSortedProperties)
            {
                // Only care about the properties in the parent class
                if (!dictParentProperties.ContainsKey(item.Name))
                    continue;
                if (item.Name == "DocID")
                    continue;

                if (item.Name != "TagTerms")
                {
                    object oldValue = item.GetValue(oldItem);
                    object newValue = item.GetValue(newItem);

                    if (item.PropertyType == typeof(Decimal))
                    {
                        if (Decimal.Compare((Decimal)oldValue, (Decimal)newValue) != 0) dictDelta.Add(item.Name, newValue);
                    }
                    else if (item.PropertyType == typeof(String))
                    {
                        if (String.CompareOrdinal((string)oldValue, (string)newValue) != 0) dictDelta.Add(item.Name, newValue);
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
        public static String WorkoutDeltaUpdateSqlStrings(FinanceDocumentItemUIViewModel oldItem, FinanceDocumentItemUIViewModel newItem)
        {
            String strSqls = "";

            List<String> listSqls = new List<string>();
            var diffs = WorkoutDeltaUpdate(oldItem, newItem);

            foreach (var diff in diffs)
            {
                if (diff.Value is DateTime)
                    listSqls.Add(" [" + diff.Key.ToString() + "] = " + ((DateTime)diff.Value).ToString("YYYY-MM-SS"));
                else if (diff.Value is Boolean)
                    listSqls.Add(" [" + diff.Key.ToString() + "] = " + (((Boolean)diff.Value) ? "1" : "0"));
                else if (diff.Value is String)
                    listSqls.Add(" [" + diff.Key.ToString() + "] = N'" + diff.Value + "'");
                else if (diff.Value is Int32)
                {
                    if ((diff.Key == "ControlCenterID" || diff.Key == "OrderID") && (Int32)diff.Value == 0)
                    {
                        listSqls.Add(" [" + diff.Key.ToString() + "] = NULL");
                    }
                    else
                        listSqls.Add(" [" + diff.Key.ToString() + "] = " + diff.Value.ToString());
                }
                else
                    listSqls.Add(" [" + diff.Key.ToString() + "] = " + diff.Value.ToString());
            }
            if (listSqls.Count > 0)
            {
                strSqls = @"UPDATE [dbo].[t_fin_document_item] SET " + string.Join(",", listSqls) 
                    + " WHERE [DocID] = " + oldItem.DocID.ToString() + " AND [ItemID] = " + oldItem.ItemID.ToString();
            }

            return strSqls;
        }
    }

    public sealed class FinanceDocumentItemWithBalanceUIViewModel : FinanceDocumentItemUIViewModel
    {
        public Boolean TranType_Exp { get; set; }
        public String TranCurr { get; set; }
        public Decimal TranAmount_Org { get; set; }
        public Decimal TranAmount_LC { get; set; }
        public Decimal Balance { get; set; }
    }

    public sealed class FinanceDocumentItemWithBalanceUIListViewModel
    {
        // Runtime information
        public Int32 TotalCount { get; set; }
        public List<FinanceDocumentItemWithBalanceUIViewModel> ContentList = new List<FinanceDocumentItemWithBalanceUIViewModel>();

        public void Add(FinanceDocumentItemWithBalanceUIViewModel tObj)
        {
            this.ContentList.Add(tObj);
        }

        public List<FinanceDocumentItemWithBalanceUIViewModel>.Enumerator GetEnumerator()
        {
            return this.ContentList.GetEnumerator();
        }
    }
}
