using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace achihapi.ViewModels
{
    public class FinanceDocumentViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 HID { get; set; }
        [Required]
        public Int16 DocType { get; set; }
        [Required]
        public DateTime TranDate { get; set; }
        [Required]
        [StringLength(5)]
        public String TranCurr { get; set; }
        [StringLength(45)]
        public String Desp { get; set; }
        public Decimal ExgRate { get; set; }
        public Boolean ExgRate_Plan { get; set; }
        [StringLength(5)]
        public String TranCurr2 { get; set; }
        public Decimal ExgRate2 { get; set; }
        public Boolean ExgRate_Plan2 { get; set; }
        public Decimal TranAmount { get; set; }

        public virtual Boolean IsValid()
        {
            if (HID <= 0) return false;
            if (DocType <= 0) return false;
            if (String.IsNullOrEmpty(TranCurr)) return false;
            if (String.IsNullOrEmpty(Desp)) return false;

            return true;
        }
    }

    public class FinanceDocumentUIViewModel : FinanceDocumentViewModel
    {
        public String DocTypeName { get; set; }
        
        // Items
        public List<FinanceDocumentItemUIViewModel> Items = new List<FinanceDocumentItemUIViewModel>();

        public override Boolean IsValid()
        {
            if (!base.IsValid()) return false;
            if (Items.Count <= 0) return false;

            Dictionary<Int32, Object> itemids = new Dictionary<int, object>();
            Boolean bErrorOccurred = false;
            Items.ForEach(o =>
            {
                if (!bErrorOccurred)
                {
                    if (!o.IsValid())
                    {
                        bErrorOccurred = true;
                    }

                    if (itemids.ContainsKey(o.ItemID))
                    {
                        bErrorOccurred = true;
                    } else
                    {
                        itemids.Add(o.ItemID, null);
                    }
                }
            });
            if (bErrorOccurred) return false;

            return true;
        }

        public static Dictionary<String, Object> WorkoutDeltaForHeaderUpdate(FinanceDocumentUIViewModel oldDoc, 
            FinanceDocumentUIViewModel newDoc, String usrName)
        {
            Dictionary<String, Object> dictDelta = new Dictionary<string, Object>();

            if (oldDoc == null || newDoc == null || Object.ReferenceEquals(oldDoc, newDoc)
                || oldDoc.ID != newDoc.ID || oldDoc.HID != newDoc.HID || oldDoc.DocType != newDoc.DocType)
            {
                throw new ArgumentException("Invalid inputted parameter Or ID/HID/DocType change is not allowed");
            }
            if (!oldDoc.IsValid() || !newDoc.IsValid())
            {
                throw new Exception("Document is invalid");
            }

            // Header
            Type t = typeof(FinanceDocumentUIViewModel);
            Type parent = typeof(BaseViewModel);
            PropertyInfo[] parentProperties = parent.GetProperties();
            Dictionary<String, Object> dictParentProperties = new Dictionary<string, object>();
            foreach (var prop in parentProperties)
                dictParentProperties.Add(prop.Name, null);

            PropertyInfo[] listProperties = t.GetProperties();
            var listSortedProperties = listProperties.OrderBy(o => o.Name);

            foreach (PropertyInfo item in listSortedProperties)
            {
                if (dictParentProperties.ContainsKey(item.Name))
                    continue;

                if (item.Name == "ID" || item.Name == "HID" || item.Name == "DocType"
                    || item.Name == "DocTypeName" || item.Name == "Items"
                    || item.Name == "TranAmount")
                {
                    continue;
                }

                object oldValue = item.GetValue(oldDoc, null);
                object newValue = item.GetValue(newDoc, null);
                if (item.PropertyType == typeof(Decimal))
                {
                    if (Decimal.Compare((Decimal)oldValue, (Decimal)newValue) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if(item.PropertyType == typeof(String))
                {
                    if (String.CompareOrdinal((string)oldValue, (string)newValue) != 0) dictDelta.Add(item.Name, newValue);
                }
                else if(item.PropertyType == typeof(DateTime))
                {
                    if (DateTime.Compare(((DateTime)oldValue).Date, ((DateTime)newValue).Date) != 0) dictDelta.Add(item.Name, newValue);
                }
                else
                {
                    if (!Object.Equals(oldValue, newValue))
                        dictDelta.Add(item.Name, newValue);
                }
            }
            if (dictDelta.Count > 0)
            {
                dictDelta.Add("UpdatedAt", DateTime.Today);
                dictDelta.Add("UpdatedBy", usrName);
            }
            return dictDelta;
        }
        public static String WorkoutDeltaForHeaderUpdateSqlString(FinanceDocumentUIViewModel oldDoc, 
            FinanceDocumentUIViewModel newDoc,
            String usrName)
        {
            var diffs = WorkoutDeltaForHeaderUpdate(oldDoc, newDoc, usrName);

            List<String> listHeaderSqls = new List<string>();
            foreach (var diff in diffs)
            {
                if (diff.Value == null)
                {
                    listHeaderSqls.Add("[" + diff.Key.ToString() + "] = NULL");
                }
                else
                {
                    if (diff.Value is DateTime)
                        listHeaderSqls.Add("[" + diff.Key.ToString() + "] = '" + ((DateTime)diff.Value).ToString("yyyy-MM-dd") + "'");
                    else if (diff.Value is Boolean)
                        listHeaderSqls.Add("[" + diff.Key.ToString() + "] = " + (((Boolean)diff.Value) ? "1" : "NULL"));
                    else if (diff.Value is String)
                    {
                        if (String.IsNullOrEmpty((string)diff.Value) && diff.Key == "TranCurr2")
                            listHeaderSqls.Add("[" + diff.Key.ToString() + "] = NULL");
                        else
                            listHeaderSqls.Add("[" + diff.Key.ToString() + "] = N'" + diff.Value + "'");
                    }
                    else if (diff.Value is Decimal)
                    {
                        if (Decimal.Compare((Decimal)diff.Value, 0) == 0)
                        {
                            listHeaderSqls.Add("[" + diff.Key.ToString() + "] = NULL");
                        }
                        else
                            listHeaderSqls.Add("[" + diff.Key.ToString() + "] = " + diff.Value.ToString());
                    }
                    else
                        listHeaderSqls.Add("[" + diff.Key.ToString() + "] = " + diff.Value.ToString());
                }
            }

            return listHeaderSqls.Count == 0? 
                String.Empty : 
                (@"UPDATE [dbo].[t_fin_document] SET " + string.Join(",", listHeaderSqls) + " WHERE [ID] = " + oldDoc.ID.ToString());
        }
        public static Dictionary<Int32, Object> WorkoutDeltaForItemUpdate(FinanceDocumentUIViewModel oldDoc, FinanceDocumentUIViewModel newDoc)
        {
            Dictionary<Int32, Object> dictDelta = new Dictionary<Int32, Object>();

            if (oldDoc == null || newDoc == null || Object.ReferenceEquals(oldDoc, newDoc)
                || oldDoc.ID != newDoc.ID || oldDoc.HID != newDoc.HID || oldDoc.DocType != newDoc.DocType)
            {
                throw new ArgumentException("Invalid inputted parameter Or ID/HID/DocType change is not allowed");
            }
            if (!oldDoc.IsValid() || !newDoc.IsValid())
            {
                throw new Exception("Document is invalid");
            }

            // Items
            Dictionary<Int32, Int32> itemids = new Dictionary<int, Int32>();
            oldDoc.Items.ForEach(o => itemids.Add(o.ItemID, 1));
            newDoc.Items.ForEach(o =>
            {
                if (itemids.ContainsKey(o.ItemID))
                    itemids[o.ItemID] = 2;
                else
                    itemids.Add(o.ItemID, 3);
            });

            // Only left: 1
            // Both: 2
            // Only right: 3
            foreach(var itemid in itemids)
            {
                if (itemid.Value == 1)
                {
                    // ONLY left, DELETE
                    var item = oldDoc.Items.Find(o => o.ItemID == itemid.Key);
                    dictDelta.Add(itemid.Key, null);
                }
                else if(itemid.Value == 2)
                {
                    var item1 = oldDoc.Items.Find(o => o.ItemID == itemid.Key);
                    var item2 = newDoc.Items.Find(o => o.ItemID == itemid.Key);

                    var diffs = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
                    if (diffs.Count > 0)
                        dictDelta.Add(itemid.Key, diffs);
                }
                else if(itemid.Value == 3)
                {
                    // Only right, INSERT!
                    var item = newDoc.Items.Find(o => o.ItemID == itemid.Key);
                    dictDelta.Add(itemid.Key, item);
                }
            }

            return dictDelta;
        }
        public static List<String> WorkoutDeltaForItemUpdateSqlString(FinanceDocumentUIViewModel oldDoc, FinanceDocumentUIViewModel newDoc)
        {
            var diffs = WorkoutDeltaForItemUpdate(oldDoc, newDoc);
            List<String> listItemSqls = new List<string>();

            foreach(var diff in diffs)
            {
                if (diff.Value == null)
                {
                    var oitem = oldDoc.Items.Find(o => o.ItemID == diff.Key);
                    listItemSqls.Add(@"DELETE FROM [dbo].[t_fin_document_item] WHERE [DOCID] = " + oldDoc.ID.ToString() + " AND [ITEMID] = " + diff.Key.ToString());
                    // If there are tags, need delete them too
                    if (oitem.TagTerms.Count > 0)
                    {
                        listItemSqls.Add(@"DELETE FROM [dbo].[t_tag] WHERE [HID] = " + oldDoc.HID.ToString() 
                            + " AND [TagType] = " + ((Int32)(HIHTagTypeEnum.FinanceDocumentItem)).ToString()
                            + " AND [TagID] = " + oldDoc.ID.ToString()
                            + " AND [TagSubID] = " + diff.Key.ToString());
                    }
                }
                else if (diff.Value is FinanceDocumentItemUIViewModel)
                {
                    listItemSqls.Add((diff.Value as FinanceDocumentItemUIViewModel).GetDocItemInsertString());
                    // Tags
                    if ((diff.Value as FinanceDocumentItemUIViewModel).TagTerms.Count > 0)
                    {
                        listItemSqls.AddRange((diff.Value as FinanceDocumentItemUIViewModel).GetDocItemTagInsertString(oldDoc.HID));
                    }
                }
                else
                {
                    // listItemSqls
                    listItemSqls.AddRange(FinanceDocumentItemUIViewModel.WorkoutDeltaUpdateSqlStrings(oldDoc.Items.Find(o => o.ItemID == diff.Key),
                        newDoc.Items.Find(o => o.ItemID == diff.Key), oldDoc.HID));
                }
            }
            return listItemSqls;
        }
    }

    public sealed class FinanceADPDocumentUIViewModel: FinanceDocumentUIViewModel
    {
        // Account -> Advance payment
        public FinanceAccountViewModel AccountVM = new FinanceAccountViewModel();
    }

    #region Asset related
    public abstract class FinanceAssetDocumentCoreViewModel
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
        public List<FinanceDocumentItemUIViewModel> Items = new List<FinanceDocumentItemUIViewModel>();
    }

    // Asset: Buyin
    public sealed class FinanceAssetBuyinDocViewModel: FinanceAssetDocumentCoreViewModel
    {
        [Required]
        public Decimal TranAmount { get; set; }
        public Boolean? IsLegacy { get; set; }
        [Required]
        [StringLength(40)]
        public String AccountOwner { get; set; }

        public FinanceAccountExtASViewModel accountAsset { get; set; }
    }

    // Asset: Soldout
    public sealed class FinanceAssetSoldoutDocViewModel: FinanceAssetDocumentCoreViewModel
    {
        [Required]
        public Decimal TranAmount { get; set; }
        [Required]
        public Int32 AssetAccountID { get; set; }
    }

    // Asset: value change
    public sealed class FinanceAssetValueChangeViewModel : FinanceAssetDocumentCoreViewModel
    {
        [Required]
        public Int32 AssetAccountID { get; set; }
    }
    #endregion

    public sealed class FinanceLoanDocumentUIViewModel: FinanceDocumentUIViewModel
    {
        // Account -> Loan account
        public FinanceAccountViewModel AccountVM = new FinanceAccountViewModel();
    }
}

