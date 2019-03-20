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

        public static Dictionary<String, Object> WorkoutDeltaForUpdate(FinanceDocumentUIViewModel oldDoc, FinanceDocumentUIViewModel newDoc)
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

            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
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
                if (!Object.Equals(oldValue, newValue))
                {
                    dictDelta.Add(item.Name, newValue);
                }
            }

            // Items
            // Sort it
            // List<FinanceDocumentItemUIViewModel> oItems = oldDoc.Items.OrderBy(o => o.ItemID).ToList();
            // List<FinanceDocumentItemUIViewModel> nItems = newDoc.Items.OrderBy(o => o.ItemID).ToList();

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
                    dictDelta.Add("Items" + itemid.Key.ToString(), null);
                }
                else if(itemid.Value == 2)
                {
                    var item1 = oldDoc.Items.Find(o => o.ItemID == itemid.Key);
                    var item2 = newDoc.Items.Find(o => o.ItemID == itemid.Key);

                    var diffs = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
                    if (diffs.Count > 0)
                        dictDelta.Add("Items" + itemid.Key.ToString(), diffs);
                }
                else if(itemid.Value == 3)
                {
                    // Only right, INSERT!
                    var item = newDoc.Items.Find(o => o.ItemID == itemid.Key);
                    dictDelta.Add("Items" + itemid.Key.ToString(), item);
                }
            }

            return dictDelta;
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

