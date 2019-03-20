﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using achihapi.ViewModels;

namespace achihapi.test.ViewModels
{
    [TestClass]
    public class FinanceDocumentItemViewModelTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_NullInput()
        {
            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_DocIDIsDiff()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 2;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_ItemIDIsDiff()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.Desp = "Item 1";
            item2.ItemID = 2;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_SameInstance()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item1);
        }

        [TestMethod]
        public void DeltaUpdate_ChangeDesp()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.Desp = "Item 2";
            item2.ItemID = 1;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Item 2") == 0);
        }


        [TestMethod]
        public void DeltaUpdate_ChangeTranType()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.TranType = 2;
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 3;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("TranType"));
            Assert.AreEqual(3, rst["TranType"]);
        }

        [TestMethod]
        public void DeltaUpdate_ChangeAccountID()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 3;
            item2.ControlCenterID = 3;
            item2.Desp = "Item 2";
            item2.ItemID = 1;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
            Assert.AreEqual(2, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Item 2") == 0);
            Assert.IsTrue(rst.ContainsKey("AccountID"));
            Assert.AreEqual((Int32)rst["AccountID"], 3);
        }

        [TestMethod]
        public void DeltaUpdate_ChangeTranAmount()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 3;
            item2.ControlCenterID = 3;
            item2.Desp = "Item 2";
            item2.ItemID = 1;
            item2.TranAmount = 200;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Item 2") == 0);
            Assert.IsTrue(rst.ContainsKey("AccountID"));
            Assert.AreEqual((Int32)rst["AccountID"], 3);
            Assert.IsTrue(rst.ContainsKey("TranAmount"));
            Assert.AreEqual((Decimal)rst["TranAmount"], 200);
        }

        [TestMethod]
        public void DeltaUpdate_ChangeControlCenterID()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 2;
            item2.Desp = "Item 2";
            item2.ItemID = 1;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
            Assert.AreEqual(2, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Item 2") == 0);
            Assert.IsTrue(rst.ContainsKey("ControlCenterID"));
            Assert.AreEqual((Int32)rst["ControlCenterID"], 2);
        }

        [TestMethod]
        public void DeltaUpdate_ChangeOrderID()
        {
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;

            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.OrderID = 1;
            item2.Desp = "Item 2";
            item2.ItemID = 1;
            item2.TranAmount = 100;

            var rst = FinanceDocumentItemUIViewModel.WorkoutDeltaUpdate(item1, item2);
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Item 2") == 0);
            Assert.IsTrue(rst.ContainsKey("ControlCenterID"));
            Assert.AreEqual((Int32)rst["ControlCenterID"], 0);
            Assert.IsTrue(rst.ContainsKey("OrderID"));
            Assert.AreEqual((Int32)rst["OrderID"], 1);
        }
    }
}
