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

        public String LastError { get; protected set; }

        public FinanceDocumentItemViewModel()
        {
            this.TagTerms = new List<String>();
        }

        public Boolean IsValid()
        {
            if (ItemID <= 0)
            {
                LastError = "Invalid Item ID";
                return false;
            }
            if (AccountID <= 0)
            {
                LastError = "Invalid Account";
                return false;
            }
            if (TranType <= 0)
            {
                LastError = "Invalid Tran. type";
                return false;
            }
            if (TranAmount == 0)
            {
                LastError = "Invalid amount";
                return false;
            }
            if (ControlCenterID <= 0)
            {
                if (OrderID <= 0)
                {
                    LastError = "Invalid controlling info";
                    return false;
                }
            }
            else
            {
                if (OrderID > 0)
                {
                    LastError = "Invalid controlling info";
                    return false;
                }
            }
            if (String.IsNullOrEmpty(Desp))
            {
                LastError = "Invalid Desp";
                return false;
            }

            LastError = "";
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
                else
                {
                    // TagTerms
                    if (oldItem.TagTerms.Count == 0 && newItem.TagTerms.Count > 0)
                    {
                        // Just add the new tags
                        dictDelta.Add(item.Name, newItem.TagTerms);
                    }
                    else if(oldItem.TagTerms.Count > 0 && newItem.TagTerms.Count == 0)
                    {
                        // Just delete the existing tags
                        dictDelta.Add(item.Name, null);
                    }
                    else if(oldItem.TagTerms.Count > 0 && newItem.TagTerms.Count > 0)
                    {
                        Dictionary<String, Int32> tagids = new Dictionary<String, Int32>();
                        oldItem.TagTerms.ForEach(o => tagids.Add(o, 1));
                        newItem.TagTerms.ForEach(o =>
                        {
                            if (tagids.ContainsKey(o))
                                tagids[o] = 2;
                            else
                                tagids.Add(o, 3);
                        });

                        Dictionary<String, Object> dictTagDelta = new Dictionary<string, Object>();
                        List<String> listDeletes = new List<string>();
                        List<String> listInserts = new List<string>();
                        foreach(var tagid in tagids)
                        {
                            if (tagid.Value == 1)
                            {
                                // Need be delete
                                listDeletes.Add(tagid.Key);
                            }
                            else if(tagid.Value == 3)
                            {
                                // Need be insert
                                listInserts.Add(tagid.Key);
                            }
                        }

                        if (listInserts.Count > 0 || listDeletes.Count > 0)
                        {
                            if (listDeletes.Count > 0)
                                dictTagDelta.Add("D", listDeletes);
                            if (listInserts.Count > 0)
                                dictTagDelta.Add("I", listInserts);
                            dictDelta.Add(item.Name, dictTagDelta);
                        }
                    }
                }
            }

            return dictDelta;
        }
        public static List<String> WorkoutDeltaUpdateSqlStrings(FinanceDocumentItemUIViewModel oldItem, FinanceDocumentItemUIViewModel newItem, Int32 hid)
        {
            List<String> listRst = new List<string>();
            List<String> listProp = new List<string>();
            var diffs = WorkoutDeltaUpdate(oldItem, newItem);

            foreach (var diff in diffs)
            {
                if (diff.Key != "TagTerms")
                {
                    if (diff.Value is DateTime)
                        listProp.Add("[" + diff.Key.ToString() + "] = " + ((DateTime)diff.Value).ToString("YYYY-MM-SS"));
                    else if (diff.Value is Boolean)
                        listProp.Add("[" + diff.Key.ToString() + "] = " + (((Boolean)diff.Value) ? "1" : "0"));
                    else if (diff.Value is String)
                        listProp.Add("[" + diff.Key.ToString() + "] = N'" + diff.Value + "'");
                    else if (diff.Value is Int32)
                    {
                        if ((diff.Key == "ControlCenterID" || diff.Key == "OrderID") && (Int32)diff.Value == 0)
                        {
                            listProp.Add("[" + diff.Key.ToString() + "] = NULL");
                        }
                        else
                            listProp.Add("[" + diff.Key.ToString() + "] = " + diff.Value.ToString());
                    }
                    else
                        listProp.Add("[" + diff.Key.ToString() + "] = " + diff.Value.ToString());
                } 
                else
                {
                    if (diff.Value == null)
                    {
                        // Delete
                        listRst.Add(@"DELETE FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString()
                            + " AND [TagType] = " + ((Int32)(HIHTagTypeEnum.FinanceDocumentItem)).ToString()
                            + " AND [TagID] = " + oldItem.DocID.ToString()
                            + " AND [TagSubID] = " + oldItem.ItemID.ToString());
                    }
                    else if (diff.Value is List<String>)
                    {
                        // Insert
                        foreach(var term in (List<String>)diff.Value)
                        {
                            listRst.Add(@"INSERT INTO [dbo].[t_tag] ([HID],[TagType],[TagID],[TagSubID],[Term]) VALUES ("
                                + string.Join(",", new string[] {
                                    hid.ToString(),
                                    ((Int32)(HIHTagTypeEnum.FinanceDocumentItem)).ToString(),
                                    oldItem.DocID.ToString(),
                                    oldItem.ItemID.ToString(),
                                    "N'" + term + "'"})
                                + ")");
                        }
                    }
                    else
                    {
                        var dictStrs = (Dictionary<String,Object>)diff.Value;
                        foreach (var dictstr in dictStrs)
                        {
                            if (dictstr.Key == "D")
                            {
                                var listterms = (List<String>)dictstr.Value;
                                foreach(var term in listterms)
                                {
                                    listRst.Add(@"DELETE FROM [dbo].[t_tag] WHERE [HID] = " + hid.ToString()
                                        + " AND [TagType] = " + ((Int32)(HIHTagTypeEnum.FinanceDocumentItem)).ToString()
                                        + " AND [TagID] = " + oldItem.DocID.ToString()
                                        + " AND [TagSubID] = " + oldItem.ItemID.ToString()
                                        + " AND [Term] = N'" + term + "')");
                                }
                            }
                            else if (dictstr.Key == "I")
                            {
                                var listterms = (List<String>)dictstr.Value;
                                foreach (var term in listterms)
                                {
                                    listRst.Add(@"INSERT INTO [dbo].[t_tag] ([HID],[TagType],[TagID],[TagSubID],[Term]) VALUES ("
                                        + string.Join(",", new string[] {
                                                hid.ToString(),
                                                ((Int32)(HIHTagTypeEnum.FinanceDocumentItem)).ToString(),
                                                oldItem.DocID.ToString(),
                                                oldItem.ItemID.ToString(),
                                                "N'" + term + "'"})
                                        + ")");
                                }
                            }
                        }
                    }
                }
            }
            if (listProp.Count > 0)
            {
                listRst.Add(@"UPDATE [dbo].[t_fin_document_item] SET " + string.Join(", ", listProp) 
                    + " WHERE [DocID] = " + oldItem.DocID.ToString() + " AND [ItemID] = " + oldItem.ItemID.ToString());
            }

            return listRst;
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

    public sealed class FinanceNormalDocMassCreateViewModel
    {
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        public Int32 AccountID { get; set; }
        [Required]
        public Int32 TranType { get; set; }
        [Required]
        public Decimal TranAmount { get; set; }
        [Required]
        public String TranCurrency { get; set; }

        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }

        // Tag
        public List<String> TagTerms { get; }

        public String LastError { get; private set; }

        public Boolean IsValid()
        {
            if (AccountID <= 0)
            {
                LastError = "Invalid Account";
                return false;
            }
            if (TranType <= 0)
            {
                LastError = "Invalid Tran. Type";
                return false;
            }
            if (TranAmount == 0)
            {
                LastError = "Invalid Amount";
                return false;
            }
            if (String.IsNullOrEmpty(TranCurrency))
            {
                LastError = "Invalid Currency";
                return false;
            }
            if (!ControlCenterID.HasValue || ControlCenterID.Value <= 0)
            {
                if (!OrderID.HasValue || OrderID.Value <= 0)
                {
                    LastError = "Invalid controlling";
                    return false;
                }
            }
            else
            {
                if (OrderID.HasValue && OrderID.Value > 0)
                {
                    LastError = "Invalid controlling";
                    return false;
                }
            }
            if (String.IsNullOrEmpty(Desp))
            {
                LastError = "Invalid Desp";
                return false;
            }

            LastError = "";
            return true;
        }

        public FinanceNormalDocMassCreateViewModel()
        {
            this.TagTerms = new List<string>();
        }
    }
}
