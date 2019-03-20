using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using achihapi.ViewModels;

namespace achihapi.test.ViewModels
{
    [TestClass]
    public class FinanceDocumentViewModelTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_HeaderChange_NullInput()
        {
            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(null, null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_HeaderChange_SameInstance()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_HeaderChange_ID()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 2;
            doc2.HID = 1;

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_HeaderChange_HID()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 2;
            doc2.HID = 1;

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_HeaderChange_DocType()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Transfer;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
        }

        [TestMethod]
        public void DeltaUpdate_HeaderChange_Desp()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
        }

        [TestMethod]
        public void DeltaUpdate_HeaderChange_DespAndDate()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today.AddDays(1);
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
            Assert.AreEqual(2, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Test2") == 0);
            Assert.IsTrue(rst.ContainsKey("TranDate"));
            Assert.IsTrue(rst["TranDate"] is DateTime);
        }

        [TestMethod]
        public void DeltaUpdate_ItemChange_AddItem()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 100;
            doc2.Items.Add(item2);
            item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 2";
            item2.ItemID = 2;
            item2.TranAmount = 200;
            doc2.Items.Add(item2);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Items2"));
            Assert.IsTrue(rst["Items2"] is FinanceDocumentItemUIViewModel);
        }

        [TestMethod]
        public void DeltaUpdate_ItemChange_RemoveItem()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 100;
            doc2.Items.Add(item2);
            item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 2";
            item2.ItemID = 2;
            item2.TranAmount = 200;
            doc2.Items.Add(item2);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc2, doc1);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Items2"));
            Assert.IsNull(rst["Items2"]);
        }

        [TestMethod]
        public void DeltaUpdate_ItemChange_ChangeItem()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 110;
            doc2.Items.Add(item2);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForUpdate(doc1, doc2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Items1"));
            Assert.IsTrue(rst["Items1"] is Dictionary<String, Object>);
        }
    }
}

